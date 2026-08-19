using System.Globalization;
using System.Text.RegularExpressions;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;

namespace RmtoSync.Services;

/// <summary>Normalized plate components (ITS chapter 2-1 / Iranian plate layout).</summary>
public sealed record ParsedPlate(
    string P1,
    string P2Letter,
    string P3,
    string P4,
    string Raw)
{
    public bool HasAnyPart =>
        P1.Length > 0 || P2Letter.Length > 0 || P3.Length > 0 || P4.Length > 0;
}

/// <summary>Non-fatal issue found while classifying a plate (ITS error codes 108 / 109 / 114).</summary>
public sealed record PlateValidationIssue(long? ErrorCode, string Description)
{
    public bool HasErrorCode => ErrorCode.HasValue;
}

/// <summary>Outcome of plate validation — maps 1:1 to the Rahdari plate fields.</summary>
public sealed record PlateValidationResult(
    PlateType Type,
    long VehiclePlate,
    string? RfidNumber,
    long Lpf,
    long Slpf,
    IReadOnlyList<PlateValidationIssue> Issues)
{
    public PlateTransmissionPayload ToPayload() => new()
    {
        VehiclePlate = VehiclePlate,
        RfidNumber = RfidNumber,
        Lpf = Lpf,
        Slpf = Slpf
    };
}

/// <summary>
/// Iranian plate validation per ITS guide chapter 3-1 (v4):
/// 3-1-2 formatted plates, 3-1-3 damaged plates, 3-1-4 plates without a fixed format.
/// Encodes letter codes from official table 1-1 and LPF/SLPF from table 2-1.
/// </summary>
public static class PlateValidationService
{
    public const string UnreadableMarker = "#";
    public const string UnreadablePersianMarker = "ناخوانا";
    public const string TransitLetter = "گ";

    private static readonly Regex NationalLayoutPattern = new(
        @"^(\d{2})(\D)(\d{3})(\d{2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static readonly PlateValidationIssue InvalidLengthIssue =
        new(ItsErrorCodes.AddTto.InvalidPlateLength, "طول شماره پلاک صحیح نیست (ITS 108)");

    public static readonly PlateValidationIssue InvalidLetterIssue =
        new(ItsErrorCodes.AddTto.InvalidPlateLetter, "بخش حرفی پلاک اشتباه است (ITS 109)");

    public static readonly PlateValidationIssue ZeroNotAllowedIssue =
        new(ItsErrorCodes.AddTto.ZeroInPlateNotAllowed, "مقدار صفر در شماره پلاک مجاز نیست (ITS 114)");

    /// <summary>Validates a whole plate string such as «18س24411» (spaces and Persian digits allowed).</summary>
    public static PlateValidationResult Validate(string? plateNo)
    {
        if (IsUnreadableRaw(plateNo))
            return Damaged(Array.Empty<PlateValidationIssue>());

        return Evaluate(ParseRaw(plateNo));
    }

    /// <summary>Validates plate parts as stored by the ANPR pipeline (PlateP1 … PlateP4).</summary>
    public static PlateValidationResult Validate(CameraPhotoRecord photo) =>
        Evaluate(ParseParts(photo.PlateP1, photo.PlateP2, photo.PlateP3, photo.PlateP4));

    /// <summary>Normalizes and groups plate parts; ordering follows the ANPR engine output (P1 | letter | P3 | P4).</summary>
    public static ParsedPlate ParseParts(string? p1, string? p2, string? p3, string? p4)
    {
        var n1 = PersianPlateNormalizer.NormalizeDigits(p1).Trim();
        var letter = PersianPlateNormalizer.NormalizeLetters(p2).Trim();
        var n3 = PersianPlateNormalizer.NormalizeDigits(p3).Trim();
        var n4 = PersianPlateNormalizer.NormalizeDigits(p4).Trim();
        var raw = string.Concat(n1, letter, n3, n4);
        return new ParsedPlate(n1, letter, n3, n4, raw);
    }

    /// <summary>
    /// Robust parser for a raw plate string: compacts OCR noise (spaces, dots, Persian/Arabic digits)
    /// and splits the national layout 2 digits + letter + 3 digits + 2 digits.
    /// </summary>
    public static ParsedPlate ParseRaw(string? plateNo)
    {
        var normalized = PersianPlateNormalizer.Normalize(PersianPlateNormalizer.Compact(plateNo));
        if (normalized.Length == 0)
            return new ParsedPlate(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        var match = NationalLayoutPattern.Match(normalized);
        if (match.Success)
        {
            return new ParsedPlate(
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value,
                match.Groups[4].Value,
                normalized);
        }

        return new ParsedPlate(string.Empty, string.Empty, string.Empty, string.Empty, normalized);
    }

    public static PlateValidationResult Evaluate(ParsedPlate plate)
    {
        var issues = new List<PlateValidationIssue>();

        // ITS 3-1-3 — پلاک مخدوش: vehiclePlate=0 و بدون rFIDNUMBER.
        if (IsUnreadableMarker(plate.P2Letter))
            return Damaged(issues);

        if (!plate.HasAnyPart && plate.Raw.Length == 0)
            return Damaged(issues);

        var (isNational, isGLetter) = ClassifyLayout(plate);

        if (isNational)
        {
            // ITS 3-1-2 — پلاک فرمت‌بندی‌شده.
            var letterCode = MapLetterCode(plate.P2Letter);
            if (letterCode == PlateLetterCodes.UnknownCode)
            {
                // بخش حرفی قابل شناسایی نیست — نباید کد ساختگی ارسال شود (ITS 109).
                issues.Add(InvalidLetterIssue);
                return Damaged(issues);
            }

            // پلاک گذر موقت (گ) به‌دلیل داشتن تاریخ جزو پلاک‌های استاندارد نیست (ITS 3-1-2).
            if (isGLetter)
                return BuildUnformatted(plate, LicensePlateFormats.TransitTemporaryIran, issues);

            // صفر در شماره/سریال پلاک استاندارد مجاز نیست (ITS 114).
            if (ContainsZero(plate.P1) || ContainsZero(plate.P3) || ContainsZero(plate.P4))
            {
                issues.Add(ZeroNotAllowedIssue);
                return BuildUnformatted(plate, ResolveUnformattedFormat(plate, isGLetter: false), issues);
            }

            var vehiclePlate = ComposeVehiclePlate(plate.P1, letterCode, plate.P3, plate.P4);
            if (vehiclePlate == 0)
            {
                issues.Add(InvalidLengthIssue);
                return BuildUnformatted(plate, ResolveUnformattedFormat(plate, isGLetter: false), issues);
            }

            return new PlateValidationResult(
                PlateType.Formatted,
                vehiclePlate,
                null,
                LicensePlateFormats.NationalIran.Lpf,
                LicensePlateFormats.NationalIran.Slpf,
                issues);
        }

        // ITS 3-1-4 — پلاک بدون قالب مشخص: rFIDNUMBER رشته خام بدون فاصله/نویسه اضافی.
        if (plate.Raw.Length > 0 && ContainsPersianScript(plate.Raw))
            issues.Add(InvalidLengthIssue);

        return BuildUnformatted(plate, ResolveUnformattedFormat(plate, isGLetter), issues);
    }

    /// <summary>
    /// Guards a ready-to-send payload against ITS plate invariants (errors 108 / 114 / 127).
    /// </summary>
    public static void AssertValid(PlateTransmissionPayload plate, long photoId)
    {
        if (plate.VehiclePlate > 0)
        {
            if (plate.VehiclePlate is < 100_000_000L or > 999_999_999L)
            {
                throw new RahdariSendException(
                    photoId,
                    "vEHICLEPLATE must be a 9-digit number (ITS 108)",
                    ItsErrorCodes.AddTto.InvalidPlateLength);
            }

            // ITS 114 — صفر فقط در بخش‌های عددی پلاک (سری‌-‌سریال و شماره) مجاز نیست؛
            // کد دو رقمیِ حرف (ارقام ۳-۴) به‌طور مشروع می‌تواند صفر داشته باشد (الف..خ = 01..09، د=10، ظ=20، و=30).
            var digits = plate.VehiclePlate.ToString(CultureInfo.InvariantCulture);
            var hasZeroInPlateNumber =
                digits[0] == '0' || digits[1] == '0' ||   // P1 — شماره (سمت راست)
                digits[4] == '0' || digits[5] == '0' || digits[6] == '0' || // P3 — سریال
                digits[7] == '0' || digits[8] == '0';     // P4 — شماره (سمت چپ)

            if (hasZeroInPlateNumber)
            {
                throw new RahdariSendException(
                    photoId,
                    "Zero digit is not allowed in a formatted plate (ITS 114)",
                    ItsErrorCodes.AddTto.ZeroInPlateNotAllowed);
            }

            return;
        }

        if (plate.UsesRfidNumber)
        {
            var raw = plate.RfidNumber!;
            if (raw.Any(c => char.IsWhiteSpace(c) || !char.IsLetterOrDigit(c)))
            {
                throw new RahdariSendException(
                    photoId,
                    "rFIDNUMBER must be a raw plate string without spaces or symbols (ITS 127)",
                    ItsErrorCodes.AddTto.InvalidTransitPlateChars);
            }
        }

        // Damaged plate — vEHICLEPLATE=0 بدون rFIDNUMBER مجاز است (ITS 3-1-3).
    }

    public static bool IsUnreadableMarker(string? letter) =>
        letter == UnreadableMarker || letter == UnreadablePersianMarker;

    private static bool IsUnreadableRaw(string? plateNo) =>
        !string.IsNullOrWhiteSpace(plateNo) &&
        (plateNo.Contains(UnreadableMarker, StringComparison.Ordinal) ||
         plateNo.Contains(UnreadablePersianMarker, StringComparison.Ordinal));

    private static (bool IsNational, bool IsGLetter) ClassifyLayout(ParsedPlate plate)
    {
        var isG = plate.P2Letter == TransitLetter ||
                  string.Equals(plate.P2Letter, "g", StringComparison.OrdinalIgnoreCase);

        var isNational =
            plate.P1.Length == 2 &&
            plate.P3.Length == 3 &&
            plate.P4.Length == 2 &&
            IsDigits(plate.P1) && IsDigits(plate.P3) && IsDigits(plate.P4) &&
            plate.P2Letter.Length == 1 && char.IsLetter(plate.P2Letter[0]);

        return (isNational, isG);
    }

    private static string MapLetterCode(string letter)
    {
        var code = PlateLetterCodes.MapPersianLetter(letter);
        return code != PlateLetterCodes.UnknownCode
            ? code
            : PlateLetterCodes.MapLatinLetter(letter);
    }

    /// <summary>vEHICLEPLATE = P1(2) + code(2) + P3(3) + P4(2) — 9-digit number (ITS 2-1-2).</summary>
    private static long ComposeVehiclePlate(string p1, string letterCode, string p3, string p4)
    {
        var composed = $"{p1}{letterCode}{p3}{p4}";
        return composed.Length == 9 &&
               long.TryParse(composed, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static PlateValidationResult Damaged(IReadOnlyList<PlateValidationIssue> issues) =>
        new(
            PlateType.Damaged,
            VehiclePlate: 0,
            RfidNumber: null,
            LicensePlateFormats.NationalIran.Lpf,
            LicensePlateFormats.NationalIran.Slpf,
            issues);

    private static PlateValidationResult BuildUnformatted(
        ParsedPlate plate,
        LicensePlateFormatEntry format,
        List<PlateValidationIssue> issues)
    {
        var raw = PersianPlateNormalizer.Compact(plate.Raw);
        if (raw.Length == 0)
        {
            issues.Add(InvalidLengthIssue);
            return Damaged(issues);
        }

        return new PlateValidationResult(
            PlateType.Unformatted,
            VehiclePlate: 0,
            RfidNumber: raw,
            format.Lpf,
            format.Slpf,
            issues);
    }

    private static LicensePlateFormatEntry ResolveUnformattedFormat(ParsedPlate plate, bool isGLetter)
    {
        if (isGLetter)
            return LicensePlateFormats.TransitTemporaryIran;

        if (PersianPlateNormalizer.Normalize(plate.Raw).Contains("ARVAND", StringComparison.OrdinalIgnoreCase))
            return LicensePlateFormats.FreeZoneIran;

        return LicensePlateFormats.OtherIran;
    }

    private static bool IsDigits(string value) => value.Length > 0 && value.All(char.IsDigit);

    private static bool ContainsZero(string value) => value.Contains('0');

    private static bool ContainsPersianScript(string raw) =>
        raw.Any(c => char.IsLetter(c) && !char.IsAscii(c));
}