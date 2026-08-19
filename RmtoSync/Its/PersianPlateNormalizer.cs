using System.Text;

namespace RmtoSync.Its;

/// <summary>
/// Normalizes OCR / DB plate text before parsing and encoding:
/// Persian &amp; Arabic-Indic digits to ASCII, Arabic letter forms to Persian canonical forms,
/// and strips whitespace / symbols so rFIDNUMBER stays a raw string (ITS 3-1-4).
/// </summary>
public static class PersianPlateNormalizer
{
    private static readonly IReadOnlyDictionary<char, char> DigitMap = new Dictionary<char, char>
    {
        ['۰'] = '0', ['۱'] = '1', ['۲'] = '2', ['۳'] = '3', ['۴'] = '4',
        ['۵'] = '5', ['۶'] = '6', ['۷'] = '7', ['۸'] = '8', ['۹'] = '9',
        ['٠'] = '0', ['١'] = '1', ['٢'] = '2', ['٣'] = '3', ['٤'] = '4',
        ['٥'] = '5', ['٦'] = '6', ['٧'] = '7', ['٨'] = '8', ['٩'] = '9'
    };

    private static readonly IReadOnlyDictionary<char, char> LetterMap = new Dictionary<char, char>
    {
        ['ي'] = 'ی',
        ['ى'] = 'ی',
        ['ك'] = 'ک',
        ['ھ'] = 'ه',
        ['ہ'] = 'ه'
    };

    public static string NormalizeDigits(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var result = new char[value.Length];
        for (var i = 0; i < value.Length; i++)
            result[i] = DigitMap.TryGetValue(value[i], out var ascii) ? ascii : value[i];
        return new string(result);
    }

    public static string NormalizeLetters(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var result = new char[value.Length];
        for (var i = 0; i < value.Length; i++)
            result[i] = LetterMap.TryGetValue(value[i], out var canonical) ? canonical : value[i];
        return new string(result);
    }

    public static string Normalize(string? value) => NormalizeLetters(NormalizeDigits(value));

    /// <summary>Keeps only letters and digits — removes spaces, dots and any OCR noise.</summary>
    public static string Compact(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
                builder.Append(c);
        }

        return builder.ToString();
    }
}