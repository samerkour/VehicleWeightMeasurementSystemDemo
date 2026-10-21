using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
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
        var mode = ResolveMode(payload, options, photo);
        return new SendPlan(photo, payload, mode);
    }

    public static IReadOnlyList<SendPlan> PlanBatch(
        IReadOnlyList<CameraPhotoRecord> photos,
        RahdariOptions options)
    {
        return photos.Select(p => Plan(p, options)).ToList();
    }

    private static SendMode ResolveMode(TtoPayload payload, RahdariOptions options, CameraPhotoRecord photo)
    {
        if (payload.RequiresSingleSend)
            return SendMode.SingleWithImages;

        if (!options.UseBatchSend || !options.SendImagesSeparately)
            return SendMode.SingleWithImages;

        // اگر پنجره‌ی مجاز AddImage (از زمان عبور) گذشته باشد، مسیر «batch سپس تصویر»
        // هرگز نمی‌تواند تصویر را برساند و فقط TTO را ثبت می‌کند و بعد تصویر را رها می‌کند.
        // در این حالت تصویر همان‌جا و همراه addTTOInfo فرستاده می‌شود (نمونه‌ی ۲ راهنما).
        var windowHours = options.ImageAttachWindowHours > 0
            ? options.ImageAttachWindowHours
            : ServiceLimits.ImageAttachHoursAfterPass;

        if (photo.PassDatetime.AddHours(windowHours) <= DateTime.Now)
            return SendMode.SingleWithImages;

        return SendMode.BatchThenImage;
    }
}
