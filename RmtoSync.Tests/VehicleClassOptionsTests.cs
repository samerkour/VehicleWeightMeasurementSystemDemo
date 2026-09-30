using Microsoft.Extensions.Configuration;
using RmtoSync.Configuration;
using System.Globalization;
using Xunit;

namespace RmtoSync.Tests;

public class VehicleClassOptionsTests
{
    [Fact]
    public void Bind_NumericJsonKeys_MapClassCodeToPersianLabel()
    {
        var config = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream("""{ "VehicleClassLabels": { "1": "سواری", "14": "نامشخص" } }"""u8.ToArray()))
            .Build();

        var options = new VehicleClassOptions();
        config.Bind(options); // mirrors HostBootstrap: parent of the section is bound

        Assert.Equal(2, options.VehicleClassLabels.Count);
        Assert.Equal("سواری", options.VehicleClassLabels["1"]);
        Assert.Equal("نامشخص", options.VehicleClassLabels["14"]);
    }

    [Fact]
    public void ShippedAppSettings_DefineLabelsForAllFourteenClasses()
    {
        var options = LoadShippedOptions();

        // محتوای برچسب‌ها پیکربندی است (نه قرارداد کد)؛ ساختار و پوشش کلاس‌ها تضمین می‌شود.
        for (int vehicleClass = 1; vehicleClass <= 14; vehicleClass++)
        {
            var key = vehicleClass.ToString(CultureInfo.InvariantCulture);
            Assert.True(
                options.VehicleClassLabels.TryGetValue(key, out var label),
                $"کلاس {vehicleClass} بدون برچسب در appsettings.json");
            Assert.False(string.IsNullOrWhiteSpace(label));
        }

        // هیچ کلاسی نباید نگاشت اضافی/کلید غیرعددی داشته باشد.
        Assert.Equal(14, options.VehicleClassLabels.Count);
    }

    [Fact]
    public void MissingSection_LeavesLabelsEmpty_SoLookupFallsBackToUnknownLabel()
    {
        var options = new VehicleClassOptions();

        Assert.Empty(options.VehicleClassLabels);
        Assert.False(options.VehicleClassLabels.TryGetValue("1", out _));
        Assert.Equal("نامشخص", VehicleClassOptions.UnknownLabel);
    }

    private static VehicleClassOptions LoadShippedOptions()
    {
        var path = FindAppSettings();
        var config = new ConfigurationBuilder()
            .AddJsonFile(path, optional: false)
            .Build();

        var options = new VehicleClassOptions();
        config.Bind(options);
        return options;
    }

    private static string FindAppSettings()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "RmtoSync", "appsettings.json");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        throw new FileNotFoundException("RmtoSync/appsettings.json not found above the test output folder.");
    }
}
