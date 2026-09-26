using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Domain.Services;
using VehicleWeightMeasurementSystemDemo.Infrastructure;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Serial;

namespace VehicleWeightMeasurementSystemDemo
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var builder = Host.CreateApplicationBuilder();

            // 🔹 Load appsettings.json
            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // =========================
            // 🔥 ENSURE DATABASE EXISTS
            // =========================
            // Apply EF migrations at startup so a missing/stale database is
            // recreated (tables, seed data and vw_CameraFullData) before logging.
            var databaseReady = false;
            try
            {
                var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                using (var db = new AppDbContext(dbOptions))
                {
                    db.Database.Migrate();
                }
                databaseReady = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Startup] Database unavailable; continuing with file-only logging. {ex.Message}");
            }

            // =========================
            // 🔥 SERILOG CONFIG HERE
            // =========================

            var fileLevel = Enum.TryParse<LogEventLevel>(
                builder.Configuration["Serilog:FileLevel"], true, out var fLevel)
                ? fLevel : LogEventLevel.Information;

            var sqlLevel = Enum.TryParse<LogEventLevel>(
                builder.Configuration["Serilog:SqlLevel"], true, out var sLevel)
                ? sLevel : LogEventLevel.Error;

            var columnOptions = new ColumnOptions();
            columnOptions.Store.Add(StandardColumn.LogEvent);

            // 🔥 parse level safely
            var minLevel = Enum.TryParse<LogEventLevel>(
                builder.Configuration["Serilog:MinimumLevel"], true, out var level)
                ? level : LogEventLevel.Information;

            var logConfiguration = new LoggerConfiguration()

            // ✅ 👉 ADD IT HERE (FIRST THING)
            .MinimumLevel.Is(minLevel)

            // 🔥 reduce noise
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)

            // 🔥 file logs (all)
            .WriteTo.File("logs/log.txt",
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: fileLevel);

            // 🔥 SQL logs (only errors) - guarded so a dead database never crashes startup
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
            builder.Logging.AddSerilog(); // 🔥 IMPORTANT

            // =========================
            // 🔹 YOUR EXISTING SERVICES
            // =========================

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<SqlRepository>();

            builder.Services.AddSingleton<IPlateRecognitionEngine, SatpaRecognitionEngine>();
            builder.Services.AddSingleton<IPlateImageProcessor, PlateImageProcessor>();
            builder.Services.AddSingleton<IPlateRecognitionService, PlateRecognitionService>();

            builder.Services.AddSingleton<IGeoLocationService, WindowsGeoLocationService>();

            // 🔹 Bind classification config
            builder.Services.Configure<VehicleClassificationSettings>(
                builder.Configuration.GetSection("VehicleClassification"));

            // 🔹 Domain service: class-based limits + violation rules (config-driven)
            builder.Services.AddSingleton<IVehicleComplianceService>(sp =>
                new VehicleComplianceService(
                    sp.GetRequiredService<IOptions<VehicleClassificationSettings>>().Value));


            // 🔹 Bind config
            builder.Services.Configure<SerialPortSettings>(
                builder.Configuration.GetSection("SerialPort"));

            // 🔹 Register service
            builder.Services.AddSingleton<SerialPortService>();



            // 🔹 Bind config
            builder.Services.Configure<SnapshotCameraSettings>(
                builder.Configuration.GetSection("SnapshotCamera"));

            // 🔹 Register service (Singleton = correct for watchers)
            builder.Services.AddSingleton<CameraWatcherService>();



            builder.Services.AddTransient<MainForm>();
            builder.Services.AddTransient<VehicleReportForm>();


            var app = builder.Build();

            // =========================
            // 🔥 GLOBAL ERROR HANDLING
            // =========================

            ApplicationConfiguration.Initialize();

            Application.ThreadException += (s, e) =>
            {
                Log.Error(e.Exception, "UI Thread Exception");
                RestartAfterCrash("UI thread exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled Exception");
                RestartAfterCrash("Unhandled exception");
            };

            // حلقه‌ی کرش‌های پیاپی: بعد از چند بار سریع، دیگر ری‌استارت نشود
            _ = Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ =>
                Environment.SetEnvironmentVariable(CrashRestartCountEnv, "0"));

            try
            {
                using var scope = app.Services.CreateScope();
                var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();

                Application.Run(mainForm);

                // 🔥 ری‌استارت خودکار: بعد از خروج کامل (پاک‌سازی سرویس‌ها و flush لاگ‌ها)
                if (mainForm.RestartRequested)
                {
                    Log.Information("Relaunching application process...");

                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(Application.ExecutablePath)
                        {
                            UseShellExecute = true
                        });
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed");
                RestartAfterCrash("Application.Run crashed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private const string CrashRestartCountEnv = "VWMS_CRASH_RESTARTS";
        private const int MaxConsecutiveCrashRestarts = 3;
        private static int _crashRestartStarted;

        /// <summary>
        /// اجرای یک نمونه جدید برنامه بعد از کرش؛ با محافظت در برابر حلقه‌ی بی‌پایان.
        /// </summary>
        private static void RestartAfterCrash(string reason)
        {
            if (Interlocked.Exchange(ref _crashRestartStarted, 1) == 1)
                return;

            var count = int.TryParse(
                Environment.GetEnvironmentVariable(CrashRestartCountEnv), out var n)
                ? n : 0;

            if (count >= MaxConsecutiveCrashRestarts)
            {
                Log.Fatal(
                    "Crash restart limit ({Max}) reached — not restarting again. Reason: {Reason}",
                    MaxConsecutiveCrashRestarts, reason);
                return;
            }

            Log.Warning(
                "Application crashed ({Reason}) — restarting (attempt {Attempt})",
                reason, count + 1);

            Log.CloseAndFlush();

            // متغیر محیطی به پروسه‌ی فرزند هم منتقل می‌شود
            Environment.SetEnvironmentVariable(CrashRestartCountEnv, (count + 1).ToString());

            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(Application.ExecutablePath)
                    {
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to relaunch application after crash");
            }
        }
    }
}