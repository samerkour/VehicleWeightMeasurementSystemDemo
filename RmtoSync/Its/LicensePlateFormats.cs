namespace RmtoSync.Its;

/// <summary>Table 2-1 — LPF / SLPF country and plate type. ITS guide chapter 2-1-3.</summary>
public sealed record LicensePlateFormatEntry(long Lpf, long Slpf, string CountryName, string PlateTypeName);

public static class LicensePlateFormats
{
    public static LicensePlateFormatEntry OtherOther { get; } = new(0, 0, "سایر", "سایر");
    public static LicensePlateFormatEntry OtherIran { get; } = new(0, 1, "ایران", "سایر");
    public static LicensePlateFormatEntry NationalIran { get; } = new(1, 1, "ایران", "ملی");
    public static LicensePlateFormatEntry MotorcycleIran { get; } = new(1, 2, "ایران", "موتور سیکلت");
    public static LicensePlateFormatEntry IrPlateIran { get; } = new(1, 3, "ایران", "IR");
    public static LicensePlateFormatEntry TransitTemporaryIran { get; } = new(1, 4, "ایران", "گذر موقت");
    public static LicensePlateFormatEntry OldTransitIran { get; } = new(1, 5, "ایران", "ترانزیت قدیم");
    public static LicensePlateFormatEntry TransitIran { get; } = new(1, 6, "ایران", "ترانزیت");
    public static LicensePlateFormatEntry OldFreeZoneIran { get; } = new(1, 7, "ایران", "منطقه آزاد قدیم");
    public static LicensePlateFormatEntry FreeZoneIran { get; } = new(1, 8, "ایران", "منطقه آزاد");

    private static readonly IReadOnlyList<LicensePlateFormatEntry> AllEntries =
    [
        OtherOther,
        OtherIran,
        NationalIran,
        MotorcycleIran,
        IrPlateIran,
        TransitTemporaryIran,
        OldTransitIran,
        TransitIran,
        OldFreeZoneIran,
        FreeZoneIran,
        new(0, 2, "افغانستان", "سایر"),
        new(0, 3, "ارمنستان", "سایر"),
        new(0, 4, "آذربایجان", "سایر"),
        new(0, 5, "امارات متحده عربی", "سایر"),
        new(0, 6, "عراق", "سایر"),
        new(0, 7, "قزاقستان", "سایر"),
        new(0, 8, "پاکستان", "سایر"),
        new(0, 9, "روسیه", "سایر"),
        new(0, 10, "تاجیکستان", "سایر"),
        new(0, 11, "ترکیه", "سایر"),
        new(0, 12, "ترکمنستان", "سایر"),
        new(0, 13, "قطر", "سایر"),
        new(0, 14, "بحرین", "سایر"),
        new(0, 15, "کویت", "سایر"),
        new(0, 16, "عربستان", "سایر"),
        new(0, 17, "ازبکستان", "سایر"),
        new(0, 18, "گرجستان", "سایر"),
        new(0, 19, "عمان", "سایر"),
        new(0, 20, "قرقیزستان", "سایر"),
        new(0, 21, "اروپا", "سایر")
    ];

    public static IReadOnlyList<LicensePlateFormatEntry> All() => AllEntries;

    public static bool TryGet(long lpf, long slpf, out LicensePlateFormatEntry entry)
    {
        entry = AllEntries.FirstOrDefault(e => e.Lpf == lpf && e.Slpf == slpf)!;
        return entry != null;
    }
}
