using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
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



            Log.Logger = new LoggerConfiguration()

                // ✅ 👉 ADD IT HERE (FIRST THING)
                .MinimumLevel.Is(minLevel)

                // 🔥 reduce noise
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)


                // 🔥 file logs (all)
                .WriteTo.File("logs/log.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: fileLevel)

                // 🔥 SQL logs (only errors)
                .WriteTo.MSSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions,
                    restrictedToMinimumLevel: sqlLevel
                )
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(); // 🔥 IMPORTANT

            // =========================
            // 🔹 YOUR EXISTING SERVICES
            // =========================

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<SqlRepository>();

            builder.Services.AddSingleton<IPlateRecognitionEngine, SatpaRecognitionEngine>();
            builder.Services.AddSingleton<PlateRecognitionService>();


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

            var app = builder.Build();

            // =========================
            // 🔥 GLOBAL ERROR HANDLING
            // =========================

            ApplicationConfiguration.Initialize();

            Application.ThreadException += (s, e) =>
            {
                Log.Error(e.Exception, "UI Thread Exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled Exception");
            };

            try
            {
                using var scope = app.Services.CreateScope();
                var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application crashed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}