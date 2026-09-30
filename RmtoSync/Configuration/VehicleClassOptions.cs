namespace RmtoSync.Configuration;

/// <summary>
/// نام‌های خوانای کلاس‌های خودرو (کلاس ۱ تا ۱۴)؛ کلید = کد کلاس، مقدار = برچسب فارسی.
/// منبع نمایش در اورلی تصویر؛ نگاشت در کد سخت‌کد نمی‌شود.
/// </summary>
public sealed class VehicleClassOptions
{
    public const string SectionName = "VehicleClassLabels";

    /// <summary>برچسب نمایشی برای کلاس نامشخص/نال/خارج از محدوده.</summary>
    public const string UnknownLabel = "نامشخص";

    /// <summary>
    /// کلید = کد کلاس به‌صورت متن (نگاشت عددی در <c>ConfigurationBinder</c> پشتیبانی نمی‌شود)،
    /// مقدار = برچسب فارسی. نمونه: <c>"1": "سواری"</c>.
    /// </summary>
    public Dictionary<string, string> VehicleClassLabels { get; set; } = new(StringComparer.Ordinal);
}
