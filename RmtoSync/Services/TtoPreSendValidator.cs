using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;

namespace RmtoSync.Services;

public static class TtoPreSendValidator
{
    public static void Validate(TtoPayload payload, byte[]? colorImage, byte[]? plateImage, bool requireImages)
    {
        if (payload.ReferenceNo <= 0)
            throw new RahdariSendException(payload.ReferenceNo, "ReferenceNo is required and must be greater than zero");

        PlateValidationService.AssertValid(payload.Plate, payload.ReferenceNo);

        var now = DateTime.Now;
        if (payload.PassDateTime > now.AddMinutes(1))
            throw new RahdariSendException(payload.ReferenceNo, "PassDateTime cannot be in the future");

        if (payload.ReceiveDateTime > now.AddMinutes(1))
            throw new RahdariSendException(payload.ReferenceNo, "ReceiveDateTime cannot be in the future");

        if (payload.PassDateTime > payload.ReceiveDateTime.AddMinutes(1))
            throw new RahdariSendException(payload.ReferenceNo, "PassDateTime cannot be after ReceiveDateTime");

        if (payload.PassDateTime < now.AddDays(-ServiceLimits.MaxDataAgeDays))
            throw new RahdariSendException(payload.ReferenceNo, $"PassDateTime is older than {ServiceLimits.MaxDataAgeDays} days (ITS 113)");

        if (payload.ReceiveDateTime < now.AddDays(-ServiceLimits.MaxDataAgeDays))
            throw new RahdariSendException(payload.ReferenceNo, $"ReceiveDateTime is older than {ServiceLimits.MaxDataAgeDays} days (ITS 113)");

        if (!VehicleClass14Catalog.IsValid(payload.CarClass13))
            throw new RahdariSendException(payload.ReferenceNo, $"Invalid CARCLASS13 value: {payload.CarClass13}");

        ValidateSpeedFields(payload);

        if (payload.Allowed == TtoFieldValues.Allowed.Violation && payload.CrimeCodes.Count == 0)
            throw new RahdariSendException(payload.ReferenceNo, "Violation traffic requires at least one crime code");

        if (payload.Allowed == TtoFieldValues.Allowed.Permitted && payload.CrimeCodes.Count > 0)
            throw new RahdariSendException(payload.ReferenceNo, "Permitted traffic cannot include crime codes");

        if (payload.IsHeavy || payload.IsViolation)
        {
            if (!requireImages)
                throw new RahdariSendException(payload.ReferenceNo, "Heavy/violation traffic must include images in addTTOInfo");

            ValidateImages(payload, colorImage, plateImage, payload.IsHeavy);
        }
        else if (requireImages)
        {
            ValidateImages(payload, colorImage, plateImage, isHeavy: false);
        }
    }

    /// <summary>ITS v4 table SpeedType + errors 116 / 124.</summary>
    private static void ValidateSpeedFields(TtoPayload payload)
    {
        var photoId = payload.ReferenceNo;

        switch (payload.SpeedType)
        {
            case TtoFieldValues.SpeedType.None:
                if (payload.VehicleSpeed > 0 || payload.AverageSpeed > 0)
                    throw new RahdariSendException(photoId, "SpeedType=0 requires VehicleSpeed and average speed to be 0 (ITS 116)");
                break;

            case TtoFieldValues.SpeedType.Instant:
                if (payload.VehicleSpeed <= 0)
                    throw new RahdariSendException(photoId, "Instant speed requires VehicleSpeed > 0 (ITS 116)");
                break;

            case TtoFieldValues.SpeedType.Average:
                if (payload.AverageSpeed <= 0)
                    throw new RahdariSendException(photoId, "Average speed type requires reserved10 / average speed (ITS 124)");
                if (payload.PreviousDeviceCode <= 0)
                    throw new RahdariSendException(photoId, "Average speed type requires PreviousDeviceCode (ITS v4 §5-1)");
                break;

            case TtoFieldValues.SpeedType.InstantAndAverage:
                if (payload.VehicleSpeed <= 0 || payload.AverageSpeed <= 0)
                    throw new RahdariSendException(photoId, "Instant+average speed requires VehicleSpeed and average speed (ITS 124)");
                if (payload.PreviousDeviceCode <= 0)
                    throw new RahdariSendException(photoId, "Instant+average speed requires PreviousDeviceCode (ITS v4 §5-1)");
                break;

            default:
                throw new RahdariSendException(photoId, $"Invalid speedType value: {payload.SpeedType} (ITS 116)");
        }
    }

    private static ItsImageKind ResolveColorImageKind(bool isHeavy, TtoPayload payload)
    {
        if (isHeavy && payload.TotalWeight > 0)
            return ItsImageKind.MainWim;

        if (payload.SpeedType is TtoFieldValues.SpeedType.Average or TtoFieldValues.SpeedType.InstantAndAverage)
            return ItsImageKind.MainAverageSpeed;

        return ItsImageKind.Main;
    }

    private static void ValidateImages(TtoPayload payload, byte[]? colorImage, byte[]? plateImage, bool isHeavy)
    {
        var photoId = payload.ReferenceNo;

        if (colorImage is not { Length: > 0 })
            throw new RahdariSendException(photoId, "Color image is required");

        if (plateImage is not { Length: > 0 })
            throw new RahdariSendException(photoId, "Plate image is required");

        ValidateSize(photoId, "color", colorImage.Length, ImageSizeLimits.Get(ResolveColorImageKind(isHeavy, payload)));
        ValidateSize(photoId, "plate", plateImage.Length, ImageSizeLimits.Get(ItsImageKind.Plate));
    }

    private static void ValidateSize(long photoId, string label, int bytes, ImageSizeLimits.Limit limit)
    {
        if (limit.MinKb.HasValue && bytes < limit.MinKb.Value * 1024)
            throw new RahdariSendException(photoId, $"{label} image is smaller than {limit.MinKb} KB");

        if (bytes > limit.MaxKb * 1024)
            throw new RahdariSendException(photoId, $"{label} image exceeds {limit.MaxKb} KB");
    }
}
