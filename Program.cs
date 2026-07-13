using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VehicleWeightMeasurementSystemDemo.Camera;
using VehicleWeightMeasurementSystemDemo.Data;
using VehicleWeightMeasurementSystemDemo.Services;

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

            // 🔹 Register EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 🔹 Register Repository
            builder.Services.AddScoped<SqlRepository>();

            // 🔹 Plate Recognition
            builder.Services.AddSingleton<IPlateRecognitionEngine, SatpaRecognitionEngine>();
            builder.Services.AddSingleton<PlateRecognitionService>();

            //// Serial Port
            //builder.Services.Configure<SerialPortSettings>(
            //    builder.Configuration.GetSection("SerialPort"));
            //builder.Services.AddSingleton<SerialPortService>();

            ////Camera Folder
            //builder.Services.Configure<CameraSettings>(
            //    builder.Configuration.GetSection("Camera"));
            //builder.Services.AddSingleton<CameraWatcherService>();

            // 🔹 WinForms
            builder.Services.AddTransient<MainForm>();

            var app = builder.Build();

            ApplicationConfiguration.Initialize();

            // 🔹 Run Form
            // 🔹 Run Form
            using var scope = app.Services.CreateScope();
            var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();

            Application.Run(mainForm);
        }
    }
}