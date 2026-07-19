using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;

namespace RmtoSync.Utilities;

/// <summary>Encodes ANPR plate parts into Rahdari vEHICLEPLATE / rFIDNUMBER / LPF / SLPF.</summary>
public static class PlatePayloadEncoder
{
    private static readonly HashSet<string> FormattedLetterCodes = PlateLetterCodes
        .AllFormattedCodes()
        .Where(c => c != PlateLetterCodes.UnknownCode)
        .ToHashSet(StringComparer.Ordinal);

    public static PlateTransmissionPayload Encode(CameraPhotoRecord photo)
    {
        var p1 = photo.PlateP1?.Trim() ?? string.Empty;
        var p2Raw = photo.PlateP2?.Trim() ?? string.Empty;
        var p3 = photo.PlateP3?.Trim() ?? string.Empty;
        var p4 = photo.PlateP4?.Trim() ?? string.Empty;
        var persianLetter = PlateLetterMapper.Map(p2Raw);

        if (IsTransitTemporaryUnformatted(p1, p3, p4, persianLetter, p2Raw))
            return BuildTransitTemporaryPayload(p1, p3, p4, persianLetter);

        var letterCode = PlateCharacterCodeMapper.Map(persianLetter, p2Raw);
        if (IsStandardFormattedPlate(p1, p3, p4, letterCode))
        {
            var vehiclePlate = PlateCharacterCodeMapper.BuildVehiclePlateCode(p1, p2Raw, p3, p4);
            if (vehiclePlate > 0)
            {
                var format = LicensePlateFormats.NationalIran;
                return new PlateTransmissionPayload
                {
                    VehiclePlate = vehiclePlate,
                    Lpf = format.Lpf,
                    Slpf = format.Slpf
                };
            }
        }

        return BuildUnformattedPayload(p1, p3, p4, persianLetter, p2Raw);
    }

    /// <summary>
    /// گذر موقت (گ) without standard 2+letter+3+2 layout — ITS: send as unformatted (vehiclePlate=0, RFID).
    /// National plates with letter گ use numeric vehiclePlate (code 26).
    /// </summary>
    private static bool IsTransitTemporaryUnformatted(
        string p1, string p3, string p4, string persianLetter, string p2Raw)
    {
        var isG = persianLetter is "گ" ||
                  string.Equals(p2Raw, "g", StringComparison.OrdinalIgnoreCase);
        if (!isG)
            return false;

        return !HasNationalPlateLayout(p1, p3, p4);
    }

    private static bool HasNationalPlateLayout(string p1, string p3, string p4) =>
        p1.Length == 2 && p3.Length == 3 && p4.Length == 2 &&
        p1.All(char.IsDigit) && p3.All(char.IsDigit) && p4.All(char.IsDigit);

    private static bool IsStandardFormattedPlate(string p1, string p3, string p4, string letterCode) =>
        HasNationalPlateLayout(p1, p3, p4) && FormattedLetterCodes.Contains(letterCode);

    private static PlateTransmissionPayload BuildTransitTemporaryPayload(
        string p1, string p3, string p4, string persianLetter)
    {
        var format = LicensePlateFormats.TransitTemporaryIran;
        return new PlateTransmissionPayload
        {
            VehiclePlate = 0,
            RfidNumber = $"{p4}{persianLetter}{p3}{p1}",
            Lpf = format.Lpf,
            Slpf = format.Slpf
        };
    }

    private static PlateTransmissionPayload BuildUnformattedPayload(
        string p1, string p3, string p4, string persianLetter, string p2Raw)
    {
        var letter = string.IsNullOrWhiteSpace(persianLetter) ? p2Raw : persianLetter;
        var raw = $"{p4}{letter}{p3}{p1}".Replace(" ", string.Empty, StringComparison.Ordinal);
        var format = ResolveUnformattedPlateFormat(raw);
        return new PlateTransmissionPayload
        {
            VehiclePlate = 0,
            RfidNumber = raw,
            Lpf = format.Lpf,
            Slpf = format.Slpf
        };
    }

    private static LicensePlateFormatEntry ResolveUnformattedPlateFormat(string rfidUpper)
    {
        if (rfidUpper.Contains("ARVAND", StringComparison.OrdinalIgnoreCase))
            return LicensePlateFormats.FreeZoneIran;

        return LicensePlateFormats.OtherIran;
    }
}
