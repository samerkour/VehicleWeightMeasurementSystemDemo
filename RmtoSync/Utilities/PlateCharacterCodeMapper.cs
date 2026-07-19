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

    public static long BuildVehiclePlateCode(string? p1, string? p2Letter, string? p3, string? p4)
    {
        var persianLetter = PlateLetterMapper.Map(p2Letter);
        var code = $"{p1?.Trim()}{Map(persianLetter, p2Letter)}{p3?.Trim()}{p4?.Trim()}";
        return long.TryParse(code, out var value) ? value : 0;
    }

    public static IEnumerable<string> AllLetterCodes() => PlateLetterCodes.AllFormattedCodes();
}
