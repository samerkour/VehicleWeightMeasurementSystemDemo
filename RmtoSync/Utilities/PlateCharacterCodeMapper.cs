using System.Globalization;
using RmtoSync.Its;

namespace RmtoSync.Utilities;

/// <summary>Persian plate letter → numeric code for Rahdari vEHICLEPLATE (ITS table 1-1).</summary>
public static class PlateCharacterCodeMapper
{
    public static string Map(string? persianLetter, string? rawToken = null)
    {
        var code = PlateLetterCodes.MapPersianLetter(persianLetter);
        if (code != PlateLetterCodes.UnknownCode)
            return code;

        return PlateLetterCodes.MapLatinLetter(rawToken ?? persianLetter);
    }

    /// <summary>vEHICLEPLATE = P1(2) + code(2) + P3(3) + P4(2) — exactly 9 digits (ITS 2-1-2).</summary>
    public static long BuildVehiclePlateCode(string? p1, string? p2Letter, string? p3, string? p4)
    {
        var n1 = PersianPlateNormalizer.NormalizeDigits(p1).Trim();
        var n3 = PersianPlateNormalizer.NormalizeDigits(p3).Trim();
        var n4 = PersianPlateNormalizer.NormalizeDigits(p4).Trim();
        var code = Map(p2Letter, p2Letter);
        var combined = $"{n1}{code}{n3}{n4}";

        return combined.Length == 9 &&
               long.TryParse(combined, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    public static IEnumerable<string> AllLetterCodes() => PlateLetterCodes.AllFormattedCodes();
}