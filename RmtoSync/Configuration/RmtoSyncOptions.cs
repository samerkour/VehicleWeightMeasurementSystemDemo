namespace RmtoSync.Configuration;

using RmtoSync.Its;

public sealed class RmtoSyncOptions
{
    public const string SectionName = "RmtoSync";

    public int PollIntervalMs { get; set; } = 2000;
    public int BatchSize { get; set; } = 50;
    public int MaxRecordsPerCycle { get; set; } = 200;
    public bool RequireOverviewImage { get; set; } = true;

    /// <summary>When SATPA plate crop is missing, derive plate JPEG from scene image (ITS min 1 KB).</summary>
    public bool UsePlateCropFallback { get; set; } = true;

    public int DefaultSpeed { get; set; }
    public string TempJpegPath { get; set; } = @"D:\app\temp.jpeg";

    /// <summary>When true, the GUI starts background work automatically on launch (recommended with Windows Startup).</summary>
    public bool AutoStartServiceOnLaunch { get; set; } = true;

    /// <summary>
    /// حداکثر تعداد تلاش ارسال برای هر رکورد. پس از رسیدن به این سقف، رکورد به‌عنوان
    /// «رهاشده» از صف خارج می‌شود تا یک رکوردِ سمی جلوی پردازش بقیه را نگیرد.
    /// </summary>
    public int MaxSendAttempts { get; set; } = 3;

    /// <summary>مهلت زمانی کل ارسال هر رکورد (ثانیه)؛ نقض آن فقط همان رکورد را ناموفق می‌کند.</summary>
    public int SendTimeoutSeconds { get; set; } = 60;

    /// <summary>تعداد رکوردهایی که هم‌زمان ارسال می‌شوند (محدودیت نرخ ارسال به سرویس راهداری).</summary>
    public int MaxConcurrentSends { get; set; } = 4;

    /// <summary>فاصله‌ی بین تلاش‌های مجدد یک رکورد (ثانیه) — جلوگیری از تلاش مجدد در هر چرخه‌ی ۲ ثانیه‌ای.</summary>
    public int RetryBackoffSeconds { get; set; } = 30;

    public int EffectiveBatchSize => Math.Clamp(BatchSize, 1, ServiceLimits.MaxBatchRecords);
    public int EffectiveMaxRecords => Math.Max(MaxRecordsPerCycle, 1);
    public int EffectivePollMs => Math.Max(PollIntervalMs, 500);
    public int EffectiveMaxSendAttempts => Math.Max(MaxSendAttempts, 1);
    public int EffectiveMaxConcurrentSends => Math.Clamp(MaxConcurrentSends, 1, 16);
    public TimeSpan SendTimeout => TimeSpan.FromSeconds(Math.Clamp(SendTimeoutSeconds, 5, 600));
    public TimeSpan RetryBackoff => TimeSpan.FromSeconds(Math.Max(RetryBackoffSeconds, 0));
}
