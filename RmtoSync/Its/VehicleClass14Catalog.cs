namespace RmtoSync.Its;

/// <summary>Table 4-1 — 14-class vehicle taxonomy (CARCLASS13 field). ITS guide chapter 2-3.</summary>
public enum VehicleClass14 : int
{
    Sedan = 1,
    Pickup = 2,
    TruckTwoAxleLight = 3,
    TruckTwoAxleHeavy = 4,
    BusTwoAxle = 5,
    BusThreeAxle = 6,
    TruckThreeAxle = 7,
    TrailerThreeAxle = 8,
    TruckFourAxle = 9,
    TrailerFourAxle12Wheels = 10,
    TrailerFourAxle14Wheels = 11,
    TrailerFiveAxle12Wheels = 12,
    TrailerFiveAxle18Wheels = 13,
    TrailerSixAxle = 14
}

public static class VehicleClass14Catalog
{
    /// <summary>Send 1 when vehicle type cannot be determined (PDF chapter 2-3).</summary>
    public const int DefaultWhenUnknown = (int)VehicleClass14.Sedan;

    public static bool IsValid(int code) => code is >= 1 and <= 14;

    private static readonly IReadOnlyDictionary<VehicleClass14, string> PersianNames = new Dictionary<VehicleClass14, string>
    {
        [VehicleClass14.Sedan] = "سواری",
        [VehicleClass14.Pickup] = "وانت",
        [VehicleClass14.TruckTwoAxleLight] = "کامیون 2 محور سبک",
        [VehicleClass14.TruckTwoAxleHeavy] = "کامیون 2 محور سنگین",
        [VehicleClass14.BusTwoAxle] = "اتوبوس دو محور",
        [VehicleClass14.BusThreeAxle] = "اتوبوس 3 محور",
        [VehicleClass14.TruckThreeAxle] = "کامیون 3 محور",
        [VehicleClass14.TrailerThreeAxle] = "تریلر 3 محور",
        [VehicleClass14.TruckFourAxle] = "کامیون 4 محور",
        [VehicleClass14.TrailerFourAxle12Wheels] = "تریلر 4 محور 12 چرخ",
        [VehicleClass14.TrailerFourAxle14Wheels] = "تریلر 4 محور 14 چرخ",
        [VehicleClass14.TrailerFiveAxle12Wheels] = "تریلر 5 محور 12 چرخ",
        [VehicleClass14.TrailerFiveAxle18Wheels] = "تریلر 5 محور 18 چرخ",
        [VehicleClass14.TrailerSixAxle] = "تریلر 6 محور"
    };

    public static string GetPersianName(int code) =>
        Enum.IsDefined(typeof(VehicleClass14), code)
            ? PersianNames[(VehicleClass14)code]
            : $"نامشخص ({code})";

    public static IReadOnlyList<(int Code, string Name)> All() =>
        PersianNames.Select(kv => ((int)kv.Key, kv.Value)).OrderBy(x => x.Item1).ToList();
}
