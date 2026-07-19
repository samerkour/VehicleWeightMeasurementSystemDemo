namespace RmtoSync.Its;

/// <summary>Table 1-1 — plate letter codes for formatted Iranian plates. ITS guide chapter 2-1-2.</summary>
public static class PlateLetterCodes
{
    private static readonly IReadOnlyDictionary<string, string> PersianToCode = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["الف"] = "01",
        ["ب"] = "02",
        ["پ"] = "03",
        ["ت"] = "04",
        ["ث"] = "05",
        ["ج"] = "06",
        ["چ"] = "07",
        ["ح"] = "08",
        ["خ"] = "09",
        ["د"] = "10",
        ["ذ"] = "11",
        ["ر"] = "12",
        ["ز"] = "13",
        ["ژ"] = "14",
        ["س"] = "15",
        ["ش"] = "16",
        ["ص"] = "17",
        ["ض"] = "18",
        ["ط"] = "19",
        ["ظ"] = "20",
        ["ع"] = "21",
        ["غ"] = "22",
        ["ف"] = "23",
        ["ق"] = "24",
        ["ک"] = "25",
        ["گ"] = "26",
        ["ل"] = "27",
        ["م"] = "28",
        ["ن"] = "29",
        ["و"] = "30",
        ["ه"] = "31",
        ["ی"] = "32"
    };

    private static readonly IReadOnlyDictionary<string, string> LatinToCode =
        Enumerable.Range(0, 26)
            .ToDictionary(i => ((char)('A' + i)).ToString(), i => (51 + i).ToString("00"), StringComparer.OrdinalIgnoreCase);

    public const string UnknownCode = "00";

    public static string MapPersianLetter(string? letter)
    {
        if (string.IsNullOrWhiteSpace(letter))
            return UnknownCode;

        return PersianToCode.TryGetValue(letter.Trim(), out var code) ? code : UnknownCode;
    }

    public static string MapLatinLetter(string? letter)
    {
        if (string.IsNullOrWhiteSpace(letter))
            return UnknownCode;

        return LatinToCode.TryGetValue(letter.Trim(), out var code) ? code : UnknownCode;
    }

    public static bool IsFormattedLetterCode(string code) =>
        code != UnknownCode &&
        (PersianToCode.Values.Contains(code) || LatinToCode.Values.Contains(code));

    public static IEnumerable<string> AllFormattedCodes()
    {
        yield return UnknownCode;
        for (var i = 1; i <= 32; i++)
            yield return i.ToString("00");
        for (var i = 51; i <= 76; i++)
            yield return i.ToString("00");
    }

    public static IReadOnlyList<(string Letter, string Code)> AllPersianEntries() =>
        PersianToCode.Select(kv => (kv.Key, kv.Value)).OrderBy(x => x.Value).ToList();
}
