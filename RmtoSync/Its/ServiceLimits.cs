namespace RmtoSync.Its;

/// <summary>Operational limits from ITS guide chapters 1 and 3.</summary>
public static class ServiceLimits
{
    public const int MaxBatchRecords = 100;
    public const int ImageAttachHoursAfterPass = 24;
    public const int MaxDataAgeDays = 30;
}
