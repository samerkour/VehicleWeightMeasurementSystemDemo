namespace RmtoSync.Configuration;

using RmtoSync.Its;

public sealed class RahdariOptions
{
    public const string SectionName = "Rahdari";

    public string ServiceUrl { get; set; } = "http://10.30.197.140:8080/";
    public string UserName { get; set; } = "tabatozin";
    public string Password { get; set; } = "tab@#Tozin#123";
    public long DeviceCode { get; set; } = 16953126;
    public long SystemCode { get; set; } = 202;
    public long CompanyCode { get; set; } = 207;
    public string Reserved7 { get; set; } = "607057";
    public string StationLabel { get; set; } = "دیر - کنگان";

    public bool UseBatchSend { get; set; } = true;
    public bool SendImagesSeparately { get; set; } = true;

    public int DefaultCarClass13 { get; set; } = VehicleClass14Catalog.DefaultWhenUnknown;
    public long DefaultVehicleClass { get; set; } = TtoFieldValues.VehicleClass.Light;

    public int SpeedViolationThresholdKmh { get; set; } = 105;
    public int WeightViolationThresholdKg { get; set; } = 44000;
    public long SpeedViolationCrimeCode { get; set; } = ItsCrimeCodes.SpeedViolation;
    public long WeightViolationCrimeCode { get; set; } = ItsCrimeCodes.WeightViolation;
    public bool EnableAutoViolationDetection { get; set; }

    /// <summary>
    /// Camera-only station (no speed / WIM sensors yet). Sends stub speed/weight values with
    /// <see cref="FakeInstantSpeedKmh"/> until real sensors populate SQL columns.
    /// Set to <c>false</c> after speed/weight sensors are connected.
    /// </summary>
    public bool AnprOnlyStation { get; set; } = true;

    /// <summary>
    /// Stub instant speed (km/h) when <see cref="AnprOnlyStation"/> is true — no radar yet.
    /// </summary>
    public int FakeInstantSpeedKmh { get; set; } = 60;

    public decimal? StationLongitude { get; set; }
    public decimal? StationLatitude { get; set; }

    /// <summary>
    /// When true, probes Rahdari service reachability (and optionally general internet via
    /// <see cref="InternetCheckUrl"/>) before each send cycle and skips sending while unhealthy.
    /// </summary>
    public bool EnableHealthCheck { get; set; } = true;

    /// <summary>
    /// Optional URL used to verify general internet connectivity before sending.
    /// Empty or whitespace skips the internet probe (service reachability is still checked).
    /// </summary>
    public string InternetCheckUrl { get; set; } = string.Empty;

    /// <summary>Timeout (ms) for health probes.</summary>
    public int HealthCheckTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// When true, every Rahdari color/plate JPEG built for sending is also written to
    /// <see cref="ImageSaveFolderPath"/> (created on demand) for inspection/debugging.
    /// </summary>
    public bool SaveImagesToFolder { get; set; }

    /// <summary>Target folder for saved send images when <see cref="SaveImagesToFolder"/> is true.</summary>
    public string ImageSaveFolderPath { get; set; } = @"C:\Temp\RahdariImages";
}
