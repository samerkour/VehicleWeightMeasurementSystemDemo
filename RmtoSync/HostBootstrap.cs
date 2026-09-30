using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace RmtoSync;

public static class HostBootstrap
{
    public static IHost Build(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        var fileLevel = Enum.TryParse<LogEventLevel>(
            builder.Configuration["Serilog:FileLevel"], true, out var fLevel)
            ? fLevel : LogEventLevel.Information;

        var sqlLevel = Enum.TryParse<LogEventLevel>(
            builder.Configuration["Serilog:SqlLevel"], true, out var sLevel)
            ? sLevel : LogEventLevel.Error;

        var minLevel = Enum.TryParse<LogEventLevel>(
            builder.Configuration["Serilog:MinimumLevel"], true, out var level)
            ? level : LogEventLevel.Information;

        var databaseReady = !string.IsNullOrWhiteSpace(connectionString) && IsDatabaseAvailable(connectionString);

        var columnOptions = new ColumnOptions();
        columnOptions.Store.Add(StandardColumn.LogEvent);

        var logConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(minLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, builder.Configuration["Logging:LogPath"] ?? "logs\\rmto-sync-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                restrictedToMinimumLevel: fileLevel);

        if (databaseReady)
        {
            try
            {
                logConfiguration.WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions,
                    restrictedToMinimumLevel: sqlLevel);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Startup] SQL logging unavailable; continuing with file-only logging. {ex.Message}");
            }
        }

        Log.Logger = logConfiguration.CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();
        builder.Services.Configure<RmtoSyncOptions>(builder.Configuration.GetSection(RmtoSyncOptions.SectionName));
        builder.Services.Configure<RahdariOptions>(builder.Configuration.GetSection(RahdariOptions.SectionName));
        // نام سکشن و نام پراپرتی یکسان است؛ بنابراین والدِ سکشن بایند می‌شود تا درخت
        // VehicleClassLabels روی پراپرتی دیکشنری نگاشت شود (بایند کردن خودِ سکشن، کلیدهای ۱..۱۴ را
        // به‌عنوان نام پراپرتی می‌بیند و چیزی بایند نمی‌شود).
        builder.Services.Configure<VehicleClassOptions>(builder.Configuration);

        // مهلت پشتیبان سرویس؛ مهلت اصلی هر رکورد از RmtoSync:SendTimeoutSeconds (پیش‌فرض ۶۰ ثانیه) اعمال می‌شود.
        builder.Services.AddHttpClient("Rahdari", client => client.Timeout = TimeSpan.FromSeconds(90));

        var cameraConnection = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        builder.Services.AddDbContextFactory<RmtoSyncDbContext>(options =>
            options.UseSqlServer(cameraConnection));

        builder.Services.AddSingleton<RahdariSyncStatus>();
        builder.Services.AddSingleton<CameraPhotoQueueRepository>();
        builder.Services.AddSingleton<SchemaValidator>();
        builder.Services.AddSingleton<RahdariHealthCheck>();
        builder.Services.AddSingleton<RahdariTtoClient>();
        builder.Services.AddSingleton<TtoImageService>();
        builder.Services.AddSingleton<RmtoSendService>();

        builder.Services.AddHostedService<DatabaseHealthService>();
        builder.Services.AddHostedService<RmtoSyncWorker>();

        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "RmtoSync";
        });

        return builder.Build();
    }

    public static async Task<int> RunAsync(string[] args)
    {
        var host = Build(args);
        var config = host.Services.GetRequiredService<IConfiguration>();

        try
        {
            Log.Information("RmtoSync starting (Rahdari central terminal)");
            Log.Information("DefaultConnection={Db}", TryGetDataSource(config.GetConnectionString("DefaultConnection")));
            Log.Information("Rahdari={Url}", config[$"{RahdariOptions.SectionName}:ServiceUrl"]);
            Log.Information("AnprOnlyStation={Anpr}", config[$"{RahdariOptions.SectionName}:AnprOnlyStation"]);

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));

            // اعتبارسنجی واقعی اسکیما (نه صرفاً اتصال). ناسازگاری مدل EF با پایگاه‌داده
            // اینجا با فهرست دقیق ستون‌های مفقود گزارش می‌شود.
            var schemaReport = await host.Services.GetRequiredService<SchemaValidator>().ValidateAsync(cts.Token);
            if (schemaReport.IsCompatible)
                Log.Information("RmtoSync database schema matches the EF model");
            else
                Log.Warning(
                    "RmtoSync database schema does NOT match the EF model for {Count} object(s): {Objects}. Run {Script}.",
                    schemaReport.Incompatible.Count(),
                    string.Join(", ", schemaReport.Incompatible.Select(o =>
                        $"{o.ObjectName} (missing: {(o.IsMissingObject ? "whole object" : string.Join("/", o.MissingColumns))})")),
                    SchemaValidator.RemediationScript);

            // "Simple mode": in headless runs we don't keep polling forever.
            // Instead, we run a few sync cycles until the send queue is drained
            // (or until a cycle produces no successful sends).
            var sendService = host.Services.GetRequiredService<RmtoSendService>();
            var queue = host.Services.GetRequiredService<CameraPhotoQueueRepository>();
            var rahdariOptions = host.Services.GetRequiredService<IOptions<RahdariOptions>>().Value;
            var syncOptions = host.Services.GetRequiredService<IOptions<RmtoSyncOptions>>().Value;

            const int maxCycles = 20;
            var totalSent = 0;

            for (var cycle = 1; cycle <= maxCycles; cycle++)
            {
                cts.Token.ThrowIfCancellationRequested();

                var sent = await sendService.RunCycleAsync(cts.Token);
                totalSent += sent;

                Log.Information("Oneshot cycle {Cycle}/{MaxCycles} sent {Sent} (total {TotalSent})",
                    cycle, maxCycles, sent, totalSent);

                if (sent == 0)
                    break;

                if (!await HasAnyPendingAsync(queue, rahdariOptions, syncOptions, cts.Token))
                    break;
            }

            var pending = await HasAnyPendingAsync(queue, rahdariOptions, syncOptions, cts.Token);
            if (pending)
            {
                Log.Warning("Oneshot finished but queue is not empty (some records may be stuck due to validation / missing images). TotalSent={TotalSent}", totalSent);
                return 2;
            }

            Log.Information("Oneshot finished successfully. TotalSent={TotalSent}", totalSent);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "RmtoSync terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            host.Dispose();
        }
    }

    private static async Task<bool> HasAnyPendingAsync(
        CameraPhotoQueueRepository queue,
        RahdariOptions rahdariOptions,
        RmtoSyncOptions syncOptions,
        CancellationToken ct)
    {
        var maxAttempts = syncOptions.EffectiveMaxSendAttempts;
        var backoff = syncOptions.RetryBackoff;

        // TTO pending (metadata not yet accepted by ITS).
        var tto = await queue.GetPendingTtoBatchAsync(1, maxAttempts, backoff, ct);
        if (tto.Count > 0)
            return true;

        // Image pending only matters when images are uploaded in a separate step.
        if (rahdariOptions.SendImagesSeparately)
        {
            var images = await queue.GetPendingImageBatchAsync(1, maxAttempts, backoff, ct);
            return images.Count > 0;
        }

        return false;
    }

    public static string TryGetDataSource(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "(not set)";

        try
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            return csb.DataSource + "/" + csb.InitialCatalog;
        }
        catch
        {
            return "(invalid connection string)";
        }
    }

    private static bool IsDatabaseAvailable(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
