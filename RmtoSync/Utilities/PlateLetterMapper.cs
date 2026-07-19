namespace RmtoSync.Utilities;

public static class PlateLetterMapper
{
    public static string Map(string? part2) => part2 switch
    {
        "$" => "ش",
        "a" => "ع",
        "g" => "گ",
        "s" => "ص",
        "t" => "ط",
        "A" => "الف",
        "B" => "ب",
        "C" => "ث",
        "D" => "د",
        "F" => "ف",
        "G" => "ق",
        "H" => "ه",
        "I" => "ی",
        "J" => "ج",
        "L" => "ل",
        "M" => "م",
        "N" => "ن",
        "P" => "پ",
        "S" => "س",
        "T" => "ت",
        "V" => "و",
        "X" => "ویلچر",
        "Z" => "ز",
        "#" => "ناخوانا",
        _ => part2 ?? string.Empty
    };

    public static void ParsePlateParts(string? plateNo, out string p1, out string p2, out string p3, out string p4)
    {
        p1 = p2 = p3 = p4 = string.Empty;
        var raw = (plateNo ?? string.Empty).Trim();
        if (raw.Length < 8)
            return;

        p1 = raw[..2];
        p2 = raw[2..3];
        p3 = raw[3..6];
        p4 = raw[6..8];
    }
}
