using RmtoSync.Data;
using RmtoSync.Models;

namespace RmtoSync.Utilities;

/// <summary>
/// Encodes ANPR plate parts into Rahdari vEHICLEPLATE / rFIDNUMBER / LPF / SLPF.
/// Classification (formatted / damaged / unformatted) is delegated to
/// <see cref="RmtoSync.Services.PlateValidationService"/> per ITS guide chapter 3-1.
/// </summary>
public static class PlatePayloadEncoder
{
    public static PlateTransmissionPayload Encode(CameraPhotoRecord photo) =>
        RmtoSync.Services.PlateValidationService.Validate(photo).ToPayload();
}