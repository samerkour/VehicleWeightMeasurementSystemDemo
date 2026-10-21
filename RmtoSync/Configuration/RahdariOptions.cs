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

    public int LightVehicleSpeedViolationThresholdKmh { get; set; } = 110;
    public int HeavyVehicleSpeedViolationThresholdKmh { get; set; } = 110; 
    public int WeightViolationThresholdKg { get; set; } = 44000;

    /// <summary>
    /// Camera-only station (no speed / WIM sensors yet). Sends stub speed/weight values with
    /// <see cref="FakeInstantSpeedKmh"/> until real sensors populate SQL columns.
    /// Set to <c>false</c> after speed/weight sensors are connected.
    /// </summary>
    public bool AnprOnlyStation { get; set; } = true;

    /// <summary>
    /// سرعت جایگزین (km/h) وقتی سرعت لحظه‌ای یا میانگین ثبت نشده است (NULL یا صفر).
    /// بر اساس ITS مقدار صفر یعنی «سرعت ثبت نشده» و برای speedType = 1/2/3 نامعتبر است
    /// (خطاهای ۱۱۶ و ۱۲۴)، بنابراین هیچ‌وقت صفر فرستاده نمی‌شود.
    /// </summary>
    public int DefaultSpeed { get; set; } = 60;

    /// <summary>
    /// نام قدیمیِ <see cref="DefaultSpeed"/>؛ فقط زمانی استفاده می‌شود که
    /// <see cref="DefaultSpeed"/> صفر یا منفی باشد (سازگاری با فایل‌های پیکربندی قدیمی).
    /// </summary>
    public int FakeInstantSpeedKmh { get; set; } = 60;

    /// <summary>سرعت پیش‌فرض مؤثر برای فیلدهای سرعتِ خالی/صفر.</summary>
    public int EffectiveDefaultSpeed =>
        DefaultSpeed > 0 ? DefaultSpeed : (FakeInstantSpeedKmh > 0 ? FakeInstantSpeedKmh : 60);

    /// <summary>
    /// مهلت مجاز ارسال تصویر پس از زمان عبور خودرو (ITS). پیش‌فرض ۲۴ ساعت است.
    /// رکوردهایی که این مهلت را از دست داده‌اند با addTTOInfo همراه تصویر (نمونه‌ی ۲ راهنما)
    /// فرستاده می‌شوند، چون مسیر AddImage دیگر پذیرفته نمی‌شود.
    /// </summary>
    public int ImageAttachWindowHours { get; set; } = ServiceLimits.ImageAttachHoursAfterPass;

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
    /// When true, every Rahdari color JPEG built for sending is also written under the
    /// <see cref="WatchRootPath"/> image folder (created on demand) for inspection/debugging.
    /// </summary>
    public bool SaveImagesToFolder { get; set; }

    /// <summary>
    /// Root folder of camera snapshot files. Saved send images go to
    /// <c>{WatchRootPath}\RahdariImages</c>; when empty, <c>C:\RahdariImages</c> is used.
    /// </summary>
    public string WatchRootPath { get; set; } = string.Empty;
}
