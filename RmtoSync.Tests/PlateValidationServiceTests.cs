using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using RmtoSync.Services;
using RmtoSync.Utilities;
using Xunit;

namespace RmtoSync.Tests;

public class PlateValidationServiceTests
{
    // ── پلاک استاندارد (ITS 3-1-2) ──────────────────────────────────────────────

    [Fact]
    public void NationalPlate_18س24411_EncodesNineDigitVehiclePlate()
    {
        var result = PlateValidationService.Validate("18س24411");

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(181524411L, result.VehiclePlate);
        Assert.Null(result.RfidNumber);
        Assert.Equal(1, result.Lpf);
        Assert.Equal(1, result.Slpf);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void NationalPlate_PersianDigits_EncodesSameAsAscii()
    {
        var result = PlateValidationService.Validate("۱۸س۲۴۴۱۱");

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(181524411L, result.VehiclePlate);
    }

    [Fact]
    public void NationalPlate_NoisyOcr_CompactsWhitespaceAndSymbols()
    {
        var result = PlateValidationService.Validate(" 18 - س _ 244 .. 11 ");

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(181524411L, result.VehiclePlate);
    }

    [Fact]
    public void NationalPlate_DbSeed_MatchesOfficialSample()
    {
        var photo = new CameraPhotoRecord
        {
            PlateP1 = "۱۸",
            PlateP2 = "س",
            PlateP3 = "۲۴۴",
            PlateP4 = "۱۱"
        };

        var result = PlateValidationService.Validate(photo);

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(181524411L, result.VehiclePlate);
    }

    [Theory]
    [InlineData("12B34567", 125234567L)]   // B = 52
    [InlineData("12b34567", 125234567L)]   // لاتین کوچک → همان کد
    [InlineData("12ي34567", 123234567L)]   // ي عربی → ی فارسی (= 32)
    [InlineData("12ك34567", 122534567L)]   // ك عربی → ک فارسی (= 25)
    public void LetterAliases_MapToOfficialCode(string plate, long expected)
    {
        var result = PlateValidationService.Validate(plate);

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(expected, result.VehiclePlate);
    }

    [Fact]
    public void AllDigitsPlate_IsNotFormatted()
    {
        var result = PlateValidationService.Validate("11222433");

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal(0, result.VehiclePlate);
    }

    [Fact]
    public void OfficialMappingDictionary_MatchesItsTable1_1()
    {
        // نمونه‌های صریح مستند
        Assert.Equal("01", PlateLetterCodes.PersianLetters["الف"]);
        Assert.Equal("04", PlateLetterCodes.PersianLetters["ت"]);
        Assert.Equal("14", PlateLetterCodes.PersianLetters["ژ"]);
        Assert.Equal("15", PlateLetterCodes.PersianLetters["س"]);
        Assert.Equal("26", PlateLetterCodes.PersianLetters["گ"]);
        Assert.Equal("32", PlateLetterCodes.PersianLetters["ی"]);
        Assert.Equal("51", PlateLetterCodes.LatinLetters["A"]);
        Assert.Equal("76", PlateLetterCodes.LatinLetters["Z"]);

        Assert.Equal(32, PlateLetterCodes.PersianLetters.Count);
    }

    // ── پلاک مخدوش (ITS 3-1-3) ──────────────────────────────────────────────────

    [Theory]
    [InlineData("#")]
    [InlineData("ناخوانا")]
    public void DamagedPlate_VehiclePlateZero_AndNoRfidNumber(string letter)
    {
        var result = PlateValidationService.Validate($"12{letter}34511");

        Assert.Equal(PlateType.Damaged, result.Type);
        Assert.Equal(0, result.VehiclePlate);
        Assert.Null(result.RfidNumber);
        Assert.False(result.ToPayload().UsesRfidNumber);
    }

    [Fact]
    public void DamagedPlate_UnknownLetter_FallsBackToDamagedWithError109()
    {
        var result = PlateValidationService.Validate("12ڠ34211");

        Assert.Equal(PlateType.Damaged, result.Type);
        Assert.Equal(0, result.VehiclePlate);
        Assert.Null(result.RfidNumber);
        Assert.Contains(result.Issues, i => i.ErrorCode == ItsErrorCodes.AddTto.InvalidPlateLetter);
    }

    // ── پلاک بدون قالب مشخص (ITS 3-1-4) ─────────────────────────────────────────

    [Fact]
    public void Unformatted_FreeZone_RawRfidAndFreeZoneFormat()
    {
        var result = PlateValidationService.Validate("ARVAND9273733");

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal(0, result.VehiclePlate);
        Assert.Equal("ARVAND9273733", result.RfidNumber);
        Assert.Equal(1, result.Lpf);
        Assert.Equal(8, result.Slpf);
    }

    [Fact]
    public void Unformatted_ForeignLatin_NoLengthIssue()
    {
        var result = PlateValidationService.Validate("FT538TF");

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal("FT538TF", result.RfidNumber);
        Assert.Equal(0, result.Lpf);
        Assert.Equal(1, result.Slpf);
        Assert.DoesNotContain(result.Issues, i => i.ErrorCode == ItsErrorCodes.AddTto.InvalidPlateLength);
    }

    [Fact]
    public void TransitTemporary_G_IsNeverFormatted()
    {
        var result = PlateValidationService.Validate("12گ34567");

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal(0, result.VehiclePlate);
        Assert.Equal("12گ34567", result.RfidNumber);
        Assert.Equal(1, result.Lpf);
        Assert.Equal(4, result.Slpf);
    }

    [Fact]
    public void Unformatted_OutOfLayoutPersianPlate_ReportsError108()
    {
        var result = PlateValidationService.Validate("122417ب");

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal("122417ب", result.RfidNumber);
        Assert.Contains(result.Issues, i => i.ErrorCode == ItsErrorCodes.AddTto.InvalidPlateLength);
    }

    // ── صفر در پلاک استاندارد (ITS 114) ─────────────────────────────────────────

    [Theory]
    [InlineData("05س24411")]  // صفر در شماره (سمت راست)
    [InlineData("12س04411")]  // صفر در سریال
    [InlineData("12س24410")]  // صفر در شماره (سمت چپ)
    public void ZeroDigit_NotAllowedInStructuredPlate_DowngradedToUnformatted(string plate)
    {
        var result = PlateValidationService.Validate(plate);

        Assert.Equal(PlateType.Unformatted, result.Type);
        Assert.Equal(0, result.VehiclePlate);
        Assert.Equal(plate, result.RfidNumber);
        Assert.Contains(result.Issues, i => i.ErrorCode == ItsErrorCodes.AddTto.ZeroInPlateNotAllowed);
    }

    // ── AssertValid (خطاهای 108 / 114 / 127 پیش از ارسال) ───────────────────────

    [Fact]
    public void AssertValid_FormattedPlate_WithWrongLength_Throws108()
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = 12345678, Lpf = 1, Slpf = 1 };

        var ex = Assert.Throws<RahdariSendException>(() =>
            PlateValidationService.AssertValid(payload, photoId: 1001));

        Assert.Equal(ItsErrorCodes.AddTto.InvalidPlateLength, ex.ItsErrorCode);
    }

    [Fact]
    public void AssertValid_FormattedPlate_WithZeroDigit_Throws114()
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = 120234510, Lpf = 1, Slpf = 1 };

        var ex = Assert.Throws<RahdariSendException>(() =>
            PlateValidationService.AssertValid(payload, photoId: 1001));

        Assert.Equal(ItsErrorCodes.AddTto.ZeroInPlateNotAllowed, ex.ItsErrorCode);
    }

    [Theory]
    [InlineData(120234567L)]   // کد حرف ب = 02 — صفر در کد حرف مجاز است
    [InlineData(121034511L)]   // کد حرف د = 10 — صفر در کد حرف مجاز است
    [InlineData(122034567L)]   // کد حرف ظ = 20 — صفر در کد حرف مجاز است
    [InlineData(123034511L)]   // کد حرف و = 30 — صفر در کد حرف مجاز است
    [InlineData(181524411L)]   // 18س24411
    public void AssertValid_LetterCodeMayContainZero(long vehiclePlate)
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = vehiclePlate, Lpf = 1, Slpf = 1 };

        PlateValidationService.AssertValid(payload, photoId: 1001); // no throw
    }

    [Theory]
    [InlineData(500234567L)]   // صفر در P1 (شماره سمت راست)
    [InlineData(120204567L)]   // صفر در P3 (سریال)
    [InlineData(120234506L)]   // صفر در P4 (شماره سمت چپ)
    public void AssertValid_ZeroInPlateNumber_Throws114(long vehiclePlate)
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = vehiclePlate, Lpf = 1, Slpf = 1 };

        var ex = Assert.Throws<RahdariSendException>(() =>
            PlateValidationService.AssertValid(payload, photoId: 1001));

        Assert.Equal(ItsErrorCodes.AddTto.ZeroInPlateNotAllowed, ex.ItsErrorCode);
    }

    [Theory]
    [InlineData("12ب34567", 120234567L)]   // ب = 02 — کد حرف صفر دارد ولی پلاک استاندارد است
    [InlineData("12د34567", 121034567L)]   // د = 10
    [InlineData("12ظ34511", 122034511L)]   // ظ = 20
    [InlineData("12و34511", 123034511L)]   // و = 30
    public void Validate_LetterCodeWithZero_IsStillFormatted(string plate, long expected)
    {
        var result = PlateValidationService.Validate(plate);

        Assert.Equal(PlateType.Formatted, result.Type);
        Assert.Equal(expected, result.VehiclePlate);
        Assert.DoesNotContain(result.Issues, i => i.ErrorCode == ItsErrorCodes.AddTto.ZeroInPlateNotAllowed);
    }

    [Fact]
    public void AssertValid_UnformattedPlate_WithSpace_Throws127()
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = 0, RfidNumber = "12 ب 345", Lpf = 0, Slpf = 1 };

        var ex = Assert.Throws<RahdariSendException>(() =>
            PlateValidationService.AssertValid(payload, photoId: 1001));

        Assert.Equal(ItsErrorCodes.AddTto.InvalidTransitPlateChars, ex.ItsErrorCode);
    }

    [Fact]
    public void AssertValid_DamagedPlate_IsAccepted()
    {
        var payload = new PlateTransmissionPayload { VehiclePlate = 0, RfidNumber = null, Lpf = 1, Slpf = 1 };

        PlateValidationService.AssertValid(payload, photoId: 1001); // no throw
    }

    // ── خط مونتاژ کامل (PlatePayloadEncoder) ─────────────────────────────────────

    [Fact]
    public void Encoder_DamagedParts_ProducesPayloadWithoutRfid()
    {
        var photo = new CameraPhotoRecord { PlateP1 = "12", PlateP2 = "#", PlateP3 = "345", PlateP4 = "11" };

        var payload = PlatePayloadEncoder.Encode(photo);

        Assert.Equal(0, payload.VehiclePlate);
        Assert.False(payload.UsesRfidNumber);
        Assert.Equal(1, payload.Lpf);
    }

    [Fact]
    public void Encoder_NationalParts_ProducesNineDigitPayload()
    {
        var photo = new CameraPhotoRecord { PlateP1 = "18", PlateP2 = "س", PlateP3 = "244", PlateP4 = "11" };

        var payload = PlatePayloadEncoder.Encode(photo);

        Assert.Equal(181524411L, payload.VehiclePlate);
        Assert.False(payload.UsesRfidNumber);
        Assert.Equal(1, payload.Lpf);
        Assert.Equal(1, payload.Slpf);
    }

    [Fact]
    public void Encoder_UnformattedParts_ProducesRawRfid()
    {
        var photo = new CameraPhotoRecord { PlateP1 = "ARVAND", PlateP2 = string.Empty, PlateP3 = "927", PlateP4 = "3733" };

        var payload = PlatePayloadEncoder.Encode(photo);

        Assert.Equal(0, payload.VehiclePlate);
        Assert.True(payload.UsesRfidNumber);
        Assert.Equal("ARVAND9273733", payload.RfidNumber);
    }
}