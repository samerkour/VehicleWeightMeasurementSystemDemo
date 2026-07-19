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

    public int EffectiveBatchSize => Math.Clamp(BatchSize, 1, ServiceLimits.MaxBatchRecords);
    public int EffectiveMaxRecords => Math.Max(MaxRecordsPerCycle, 1);
    public int EffectivePollMs => Math.Max(PollIntervalMs, 500);
}
