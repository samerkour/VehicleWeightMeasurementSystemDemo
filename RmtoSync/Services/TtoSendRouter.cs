using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Models;

namespace RmtoSync.Services;

public static class TtoSendRouter
{
    public sealed record SendPlan(
        CameraPhotoRecord Photo,
        TtoPayload Payload,
        SendMode Mode);

    public enum SendMode
    {
        BatchThenImage,
        SingleWithImages
    }

    public static SendPlan Plan(CameraPhotoRecord photo, RahdariOptions options)
    {
        var payload = TtoPayloadFactory.Create(photo, options);
        var mode = ResolveMode(payload, options);
        return new SendPlan(photo, payload, mode);
    }

    public static IReadOnlyList<SendPlan> PlanBatch(
        IReadOnlyList<CameraPhotoRecord> photos,
        RahdariOptions options)
    {
        return photos.Select(p => Plan(p, options)).ToList();
    }

    private static SendMode ResolveMode(TtoPayload payload, RahdariOptions options)
    {
        if (payload.RequiresSingleSend)
            return SendMode.SingleWithImages;

        if (!options.UseBatchSend || !options.SendImagesSeparately)
            return SendMode.SingleWithImages;

        return SendMode.BatchThenImage;
    }
}
