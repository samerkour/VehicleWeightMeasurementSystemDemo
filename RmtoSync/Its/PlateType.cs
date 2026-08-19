namespace RmtoSync.Its;

/// <summary>Plate classification per ITS guide chapter 3-1-1.</summary>
public enum PlateType
{
    /// <summary>پلاک فرمت‌بندی‌شده (ایران) — vEHICLEPLATE عدد ۹ رقمی + LPF + SLPF.</summary>
    Formatted = 0,

    /// <summary>پلاک مخدوش — vEHICLEPLATE برابر صفر و بدون فیلد rFIDNUMBER (ITS 3-1-3).</summary>
    Damaged = 1,

    /// <summary>پلاک بدون قالب مشخص — vEHICLEPLATE برابر صفر و rFIDNUMBER رشته خام (ITS 3-1-4).</summary>
    Unformatted = 2
}