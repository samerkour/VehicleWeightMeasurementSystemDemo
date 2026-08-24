using System.Globalization;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using Microsoft.Extensions.Options;

namespace RmtoSync.Services;

public sealed class RmtoSendService
{
    private readonly RmtoSyncOptions _options;
    private readonly RahdariOptions _rahdariOptions;
    private readonly CameraPhotoQueueRepository _queue;
    private readonly RahdariTtoClient _rahadri;
    private readonly TtoImageService _images;
    private readonly RahdariSyncStatus _syncStatus;
    private readonly RahdariHealthCheck _healthCheck;
    private readonly ILogger<RmtoSendService> _logger;
    private readonly HashSet<long> _plateIssuesLogged = new();
    private bool _lastHealthOk = true;
    private DateTime _lastHealthWarnUtc = DateTime.MinValue;

    public RmtoSendService(
        IOptions<RmtoSyncOptions> options,
        IOptions<RahdariOptions> rahdariOptions,
        CameraPhotoQueueRepository queue,
        RahdariTtoClient rahdari,
        TtoImageService images,
        RahdariSyncStatus syncStatus,
        RahdariHealthCheck healthCheck,
        ILogger<RmtoSendService> logger)
    {
        _options = options.Value;
        _rahdariOptions = rahdariOptions.Value;
        _queue = queue;
        _rahadri = rahdari;
        _images = images;
        _syncStatus = syncStatus;
        _healthCheck = healthCheck;
        _logger = logger;
    }

    public async Task<int> RunCycleAsync(CancellationToken ct)
    {
        if (!await EnsureServiceHealthyAsync(ct))
            return 0;

        await ExpireOverdueImageWindowsAsync(ct);

        var processed = 0;
        var maxRecords = _options.EffectiveMaxRecords;

        processed += await ProcessPendingImagesAsync(maxRecords - processed, ct);
        if (processed >= maxRecords || ct.IsCancellationRequested)
            return LogCycle(processed);

        processed += await ProcessPendingTtoAsync(maxRecords - processed, ct);
        return LogCycle(processed);
    }

    private async Task<int> ProcessPendingTtoAsync(int maxRecords, CancellationToken ct)
    {
        if (maxRecords <= 0)
            return 0;

        var pending = await _queue.GetPendingTtoBatchAsync(Math.Min(_options.EffectiveBatchSize, maxRecords), ct);
        if (pending.Count == 0)
            return 0;

        foreach (var photo in pending)
            LogPlateIssues(photo);

        var plans = TtoSendRouter.PlanBatch(pending, _rahdariOptions);
        var singles = plans.Where(p => p.Mode == TtoSendRouter.SendMode.SingleWithImages).ToList();
        var batches = plans.Where(p => p.Mode == TtoSendRouter.SendMode.BatchThenImage).ToList();

        var sent = 0;
        sent += await ProcessSinglePlansAsync(singles, ct);
        sent += await ProcessBatchPlansAsync(batches, ct);
        return sent;
    }

    private async Task<int> ProcessPendingImagesAsync(int maxRecords, CancellationToken ct)
    {
        if (maxRecords <= 0 || !_rahdariOptions.SendImagesSeparately)
            return 0;

        var pending = await _queue.GetPendingImageBatchAsync(Math.Min(_options.EffectiveBatchSize, maxRecords), ct);
        var sent = 0;

        foreach (var photo in pending)
        {
            if (ct.IsCancellationRequested)
                break;

            LogPlateIssues(photo);

            try
            {
                var payload = TtoPayloadFactory.Create(photo, _rahdariOptions);
                if (await TryCompleteWithImagesAsync(photo, payload, ct))
                    sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleSendFailureAsync(photo, ex, ct);
            }
        }

        return sent;
    }

    private async Task<int> ProcessSinglePlansAsync(IReadOnlyList<TtoSendRouter.SendPlan> singles, CancellationToken ct)
    {
        var sent = 0;
        foreach (var plan in singles)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            {
                ValidatePhotoReady(plan.Photo);
                var color = _images.BuildColorImage(plan.Photo, plan.Payload);
                var plate = _images.BuildPlateImage(plan.Photo);
                TtoPreSendValidator.Validate(plan.Payload, color, plate, requireImages: true);

                var result = await _rahadri.SendSingleAsync(plan.Payload, color, plate, ct);
                RahdariTtoClient.EnsureSuccess(plan.Photo.PhotoId, result);
                await MarkSentAsync(plan.Photo, result.Reference, ct);
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleSendFailureAsync(plan.Photo, ex, ct);
            }
        }

        return sent;
    }

    private async Task<int> ProcessBatchPlansAsync(IReadOnlyList<TtoSendRouter.SendPlan> batches, CancellationToken ct)
    {
        if (batches.Count == 0)
            return 0;

        var validBatches = new List<TtoSendRouter.SendPlan>(batches.Count);
        foreach (var plan in batches)
        {
            try
            {
                ValidatePhotoReady(plan.Photo);
                TtoPreSendValidator.Validate(plan.Payload, null, null, requireImages: false);
                validBatches.Add(plan);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleSendFailureAsync(plan.Photo, ex, ct);
            }
        }

        if (validBatches.Count == 0)
            return 0;

        IReadOnlyList<InquiryInfoResult> inquiries;
        try
        {
            inquiries = await _rahadri.SendBatchAsync(validBatches.Select(b => b.Payload).ToList(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var summary = RahdariResponseInterpreter.ExplainException(ex, "addTTOInfoBatch2").Summary;
            foreach (var plan in validBatches)
                await RecordErrorAsync(plan.Photo.PhotoId, summary, ct);
            return 0;
        }

        var inquiryByRef = inquiries
            .Where(i => i.ReferenceNo > 0)
            .GroupBy(i => i.ReferenceNo)
            .ToDictionary(g => g.Key, g => g.First());

        var sent = 0;
        foreach (var plan in validBatches)
        {
            if (!inquiryByRef.TryGetValue(plan.Photo.PhotoId, out var inquiry))
            {
                await RecordErrorAsync(plan.Photo.PhotoId, "Batch response missing InquiryInfo for record", ct);
                continue;
            }

            try
            {
                RahdariTtoClient.EnsureTtoSuccess(plan.Photo.PhotoId, inquiry.ValidationCode);
                await _queue.MarkTtoRegisteredAsync(
                    plan.Photo.PhotoId,
                    inquiry.PassInfoId,
                    inquiry.PackId,
                    plan.Photo.PassDatetime,
                    ct);

                _syncStatus.RecordSuccess(
                    "addTTOInfoBatch2",
                    $"Ref={plan.Photo.PhotoId}: {ItsErrorCodes.Describe(inquiry.ValidationCode)}",
                    plan.Photo.PhotoId,
                    inquiry.ValidationCode);

                if (inquiry.ValidationCode == ItsErrorCodes.AddTto.Duplicate)
                {
                    plan.Photo.TerminalTtoRegistered = true;
                    plan.Photo.TerminalPassInfoId = inquiry.PassInfoId > 0 ? inquiry.PassInfoId : plan.Photo.TerminalPassInfoId;
                }

                if (_rahdariOptions.SendImagesSeparately)
                {
                    var refreshed = TtoPayloadFactory.Create(plan.Photo, _rahdariOptions);
                    if (await TryCompleteWithImagesAsync(plan.Photo, refreshed, ct))
                        sent++;
                }
                else if (await TryCompleteWithImagesAsync(plan.Photo, plan.Payload, ct))
                {
                    sent++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await HandleSendFailureAsync(plan.Photo, ex, ct);
            }
        }

        return sent;
    }

    private async Task<bool> EnsureServiceHealthyAsync(CancellationToken ct)
    {
        if (!_rahdariOptions.EnableHealthCheck)
            return true;

        var health = await _healthCheck.CheckAsync(ct);
        if (health.IsHealthy)
        {
            if (!_lastHealthOk)
            {
                _logger.LogInformation(
                    "Rahdari health check recovered — resuming sends. {Detail}",
                    DescribeHealth(health));
                _syncStatus.RecordSuccess(
                    "health check",
                    $"سرویس راهداری دوباره در دسترس است — {DescribeHealth(health)}");
            }

            _lastHealthOk = true;
            return true;
        }

        var wasOk = _lastHealthOk;
        _lastHealthOk = false;
        var now = DateTime.UtcNow;

        if (wasOk || now - _lastHealthWarnUtc > TimeSpan.FromMinutes(1))
        {
            _lastHealthWarnUtc = now;
            _logger.LogWarning(
                "Rahdari health check failed — skipping send cycle. {Detail}",
                DescribeHealth(health));
            _syncStatus.RecordFailure("health check", new RahdariResultExplanation(
                Title: "سرویس راهداری در دسترس نیست",
                Summary: $"ارسال در این دور متوقف شد — {DescribeHealth(health)}",
                Operation: "health check",
                Source: "Rahdari HealthCheck"));
        }

        return false;
    }

    private static string DescribeHealth(RahdariHealthResult health)
    {
        var parts = new List<string>();
        if (health.InternetCheckConfigured)
            parts.Add($"Internet: {health.InternetMessage}");
        parts.Add($"Service: {health.ServiceMessage}");
        return string.Join(" | ", parts);
    }

    private async Task<bool> TryCompleteWithImagesAsync(CameraPhotoRecord photo, TtoPayload payload, CancellationToken ct)
    {
        if (photo.TerminalImageExpired == true)
            return false;

        if (IsImageWindowExpired(photo))
        {
            await AbandonExpiredImageSendAsync(photo.PhotoId, ItsErrorCodes.AddImage.SendWindowExpired, ct);
            return false;
        }

            ValidatePhotoReady(photo);
            var color = _images.BuildColorImage(photo, payload);
            var plate = _images.BuildPlateImage(photo);
            SaveRahdariImages(photo, color, plate);
            TtoPreSendValidator.Validate(payload, color, plate, requireImages: true);

            var imageResult = await _rahadri.SendImageAsync(payload, color, plate, ct);
        if (!RahdariTtoClient.IsImageSuccess(imageResult))
        {
            if (imageResult.ErrorCode == ItsErrorCodes.AddImage.SendWindowExpired)
            {
                await AbandonExpiredImageSendAsync(photo.PhotoId, imageResult.ErrorCode, ct);
                return false;
            }

            RahdariTtoClient.EnsureImageSuccess(photo.PhotoId, imageResult);
        }

        await MarkSentAsync(photo, photo.TerminalPassInfoId ?? photo.PhotoId, ct);
        return true;
    }

    private void SaveRahdariImages(CameraPhotoRecord photo, byte[] color, byte[] plate)
    {
        if (!_rahdariOptions.SaveImagesToFolder)
            return;

        try
        {
            var dir = string.IsNullOrWhiteSpace(_rahdariOptions.ImageSaveFolderPath)
                ? @"C:\RahdariImages"
                : _rahdariOptions.ImageSaveFolderPath;
            Directory.CreateDirectory(dir);

            var name = BuildImageName(photo);
            if (color.Length > 0)
                File.WriteAllBytes(Path.Combine(dir, $"{name}_color.jpg"), color);

            if (plate.Length > 0)
                File.WriteAllBytes(Path.Combine(dir, $"{name}_plate.jpg"), plate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save Rahdari debug images PhotoId={PhotoId}", photo.PhotoId);
        }
    }

    private static string BuildImageName(CameraPhotoRecord photo)
    {
        var plate = photo.PlateNoCompact;
        if (string.IsNullOrWhiteSpace(plate))
            return photo.PhotoId.ToString(CultureInfo.InvariantCulture);

        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(plate.Trim().Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
    }

    private void ValidatePhotoReady(CameraPhotoRecord photo)
    {
        if (_options.RequireOverviewImage && !File.Exists(photo.FullPath))
            throw new RahdariSendException(photo.PhotoId, $"Overview image missing: {photo.FullPath}");
    }

    private void LogPlateIssues(CameraPhotoRecord photo)
    {
        if (!_plateIssuesLogged.Add(photo.PhotoId))
            return;

        try
        {
            var result = PlateValidationService.Validate(photo);
            foreach (var issue in result.Issues)
            {
                _logger.LogWarning(
                    "Plate classified as {PlateType} PhotoId={PhotoId} Plate={Plate} Code={Code} — {Description}",
                    result.Type,
                    photo.PhotoId,
                    photo.PlateNoCompact,
                    issue.ErrorCode?.ToString(CultureInfo.InvariantCulture) ?? "-",
                    issue.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to evaluate plate issues PhotoId={PhotoId}", photo.PhotoId);
        }
    }

    private async Task MarkSentAsync(CameraPhotoRecord photo, long reference, CancellationToken ct)
    {
        if (!await _queue.TryMarkTerminalSentAsync(photo.PhotoId, DateTime.Now, ct))
            throw new RahdariSendException(photo.PhotoId, "Record was already marked sent by another process");

        _logger.LogInformation(
            "Sent to Rahdari terminal PhotoId={PhotoId} Plate={Plate} Ref={Ref} CarClass13={Class}",
            photo.PhotoId, photo.PlateNoCompact, reference, photo.CarClass13 ?? _rahdariOptions.DefaultCarClass13);

        _syncStatus.RecordSent();
    }

    private async Task HandleSendFailureAsync(CameraPhotoRecord photo, Exception ex, CancellationToken ct)
    {
        var photoId = ex is RahdariSendException rse ? rse.PhotoId : photo.PhotoId;

        if (ex is RahdariSendException { ItsErrorCode: ItsErrorCodes.AddImage.SendWindowExpired })
        {
            await AbandonExpiredImageSendAsync(photoId, ItsErrorCodes.AddImage.SendWindowExpired, ct);
            return;
        }

        var explained = RahdariResponseInterpreter.ExplainException(ex, "send");
        _logger.LogError(ex, "Rahdari send failed PhotoId={PhotoId} — {Summary}", photoId, explained.Summary);
        _syncStatus.RecordFailure("send", ex, photoId);
        await RecordErrorAsync(photoId, explained.Summary, ct);
    }

    private static bool IsImageWindowExpired(CameraPhotoRecord photo) =>
        photo.TerminalImageDeadlineAt is { } deadline && deadline <= DateTime.Now;

    private async Task AbandonExpiredImageSendAsync(long photoId, long errorCode, CancellationToken ct)
    {
        var message = ItsErrorCodes.Describe(errorCode);
        try
        {
            if (await _queue.AbandonExpiredImageSendAsync(photoId, message, ct))
            {
                _logger.LogWarning(
                    "PhotoId={PhotoId} image window expired — abandoned (no retry). {Message}",
                    photoId,
                    message);
                _syncStatus.RecordFailure("AddImage", new RahdariSendException(photoId, message, errorCode), photoId);
            }
        }
        catch (Exception abandonEx)
        {
            _logger.LogWarning(abandonEx, "Failed to abandon expired image send PhotoId={PhotoId}", photoId);
        }
    }

    private async Task RecordErrorAsync(long photoId, string message, CancellationToken ct)
    {
        try
        {
            await _queue.RecordTerminalErrorAsync(photoId, message, ct);
        }
        catch (Exception recordEx)
        {
            _logger.LogWarning(recordEx, "Failed to record terminal error PhotoId={PhotoId}", photoId);
        }
    }

    private async Task ExpireOverdueImageWindowsAsync(CancellationToken ct)
    {
        if (!_rahdariOptions.SendImagesSeparately)
            return;

        var message =
            $"مهلت {ServiceLimits.ImageAttachHoursAfterPass} ساعته ارسال تصویر منقضی شده (ITS کد {ItsErrorCodes.AddImage.SendWindowExpired})";
        var count = await _queue.MarkExpiredImageWindowsAsync(message, ct);
        if (count > 0)
            _logger.LogWarning(
                "Abandoned {Count} record(s) — Rahdari image upload window expired (no retry)",
                count);
    }

    private int LogCycle(int processed)
    {
        if (processed > 0)
            _logger.LogInformation("Cycle finished. Sent {Count} photo(s) to Rahdari.", processed);
        return processed;
    }
}
