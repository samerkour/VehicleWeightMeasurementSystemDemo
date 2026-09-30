using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using RmtoSync.Configuration;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Models;
using Microsoft.Extensions.Options;

namespace RmtoSync.Services;

public sealed class RmtoSendService
{
    private const string DefaultImageRootFolder = @"C:\RahdariImages";
    private const string ImageSaveSubFolder = "RahdariImages";

    private readonly RmtoSyncOptions _options;
    private readonly RahdariOptions _rahdariOptions;
    private readonly CameraPhotoQueueRepository _queue;
    private readonly RahdariTtoClient _rahadri;
    private readonly TtoImageService _images;
    private readonly RahdariSyncStatus _syncStatus;
    private readonly RahdariHealthCheck _healthCheck;
    private readonly ILogger<RmtoSendService> _logger;
    private readonly ConcurrentDictionary<long, byte> _plateIssuesLogged = new();
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
        var startedAt = Stopwatch.GetTimestamp();

        processed += await ProcessPendingImagesAsync(maxRecords - processed, ct);
        if (processed >= maxRecords || ct.IsCancellationRequested)
            return LogCycle(processed, startedAt);

        processed += await ProcessPendingTtoAsync(maxRecords - processed, ct);
        return LogCycle(processed, startedAt);
    }

    private async Task<int> ProcessPendingTtoAsync(int maxRecords, CancellationToken ct)
    {
        if (maxRecords <= 0)
            return 0;

        var pending = await _queue.GetPendingTtoBatchAsync(
            Math.Min(_options.EffectiveBatchSize, maxRecords),
            _options.EffectiveMaxSendAttempts,
            _options.RetryBackoff,
            ct);
        if (pending.Count == 0)
            return 0;

        // لاگ مشکلات پلاک داخل ProcessOneAsync (تک‌بار برای هر رکورد) انجام می‌شود.

        var plans = TtoSendRouter.PlanBatch(pending, _rahdariOptions);
        var singles = plans.Where(p => p.Mode == TtoSendRouter.SendMode.SingleWithImages).ToList();
        var batches = plans.Where(p => p.Mode == TtoSendRouter.SendMode.BatchThenImage).ToList();

        var sent = 0;
        sent += await ProcessSinglePlansAsync(singles, ct);
        sent += await ProcessBatchPlansAsync(batches, ct);
        return sent;
    }

    /// <summary>
    /// هر تصویر مستقل پردازش می‌شود: خطای یک رکورد فقط همان رکورد را ناموفق می‌کند و
    /// پردازش بقیه‌ی صف ادامه می‌یابد.
    /// </summary>
    private async Task<int> ProcessPendingImagesAsync(int maxRecords, CancellationToken ct)
    {
        if (maxRecords <= 0 || !_rahdariOptions.SendImagesSeparately)
            return 0;

        var pending = await _queue.GetPendingImageBatchAsync(
            Math.Min(_options.EffectiveBatchSize, maxRecords),
            _options.EffectiveMaxSendAttempts,
            _options.RetryBackoff,
            ct);

        var sent = 0;
        var failed = 0;

        // ارسال‌ها موازی اما محدود: صف پرتر از پیش پیش می‌رود بدون فشار بیش از حد بر سرویس راهداری.
        await Parallel.ForEachAsync(
            pending,
            new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = _options.EffectiveMaxConcurrentSends,
            },
            async (photo, token) =>
            {
                var outcome = await ProcessOneAsync(
                    photo,
                    () => TtoPayloadFactory.Create(photo, _rahdariOptions),
                    TryCompleteWithImagesAsync,
                    SendStage.SendImage,
                    token);

                if (outcome == RecordOutcome.Sent)
                    Interlocked.Increment(ref sent);
                else if (outcome == RecordOutcome.Failed)
                    Interlocked.Increment(ref failed);
            });

        if (failed > 0)
        {
            _logger.LogWarning(
                "{Failed} image send(s) failed this cycle and will retry up to {MaxAttempts} attempt(s).",
                failed,
                _options.EffectiveMaxSendAttempts);
        }

        return sent;
    }

    private async Task<int> ProcessSinglePlansAsync(IReadOnlyList<TtoSendRouter.SendPlan> singles, CancellationToken ct)
    {
        var sent = 0;

        await Parallel.ForEachAsync(
            singles,
            new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = _options.EffectiveMaxConcurrentSends,
            },
            async (plan, token) =>
            {
                var outcome = await ProcessOneAsync(
                    plan.Photo,
                    () => plan.Payload,
                    SendSingleWithImagesAsync,
                    SendStage.AddTtoWithImages,
                    token);

                if (outcome == RecordOutcome.Sent)
                    Interlocked.Increment(ref sent);
            });

        return sent;
    }

    private async Task<bool> SendSingleWithImagesAsync(CameraPhotoRecord photo, TtoPayload payload, CancellationToken ct)
    {
        ValidatePhotoReady(photo);
        var plate = _images.BuildPlateImage(photo);
        var color = _images.BuildColorImage(photo, payload, plate);
        SaveRahdariImages(photo, color);
        TtoPreSendValidator.Validate(payload, color, plate, requireImages: true);

        var result = await _rahadri.SendSingleAsync(payload, color, plate, ct);
        RahdariTtoClient.EnsureSuccess(photo.PhotoId, result);
        await MarkSentAsync(photo, result.Reference, ct);
        return true;
    }

    /// <summary>
    /// اجرای امن یک رکورد: ادعای تلاش (attempt)، مهلت زمانی مستقل، ثبت مرحله و
    /// رهاسازی رکورد پس از سقف تلاش مجاز. تنها لغوِ خاموش‌سازی میزبان از این مسیر عبور می‌کند.
    /// </summary>
    private async Task<RecordOutcome> ProcessOneAsync(
        CameraPhotoRecord photo,
        Func<TtoPayload> buildPayload,
        Func<CameraPhotoRecord, TtoPayload, CancellationToken, Task<bool>> send,
        SendStage stage,
        CancellationToken ct)
    {
        var photoId = photo.PhotoId;
        LogPlateIssues(photo);

        var attempt = await _queue.TryBeginAttemptAsync(photoId, _options.EffectiveMaxSendAttempts, ct);
        if (attempt == 0)
        {
            _logger.LogDebug(
                "Skipped PhotoId={PhotoId} Stage={Stage} — already sent, abandoned, or retry cap reached",
                photoId, stage);
            return RecordOutcome.Skipped;
        }

        var retryCount = attempt - 1; // 0 برای اولین تلاش
        _logger.LogInformation(
            "Image send started PhotoId={PhotoId} Stage={Stage} RetryCount={RetryCount} Attempt={Attempt} of {MaxAttempts}",
            photoId, stage, retryCount, attempt, _options.EffectiveMaxSendAttempts);

        // مهلت مستقل هر رکورد: رکوردِ کند فقط خودش را از کار می‌اندازد، نه کل صف را.
        using var recordCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        recordCts.CancelAfter(_options.SendTimeout);

        try
        {
            var payload = buildPayload();
            var sent = await send(photo, payload, recordCts.Token);
            return sent ? RecordOutcome.Sent : RecordOutcome.Skipped;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Image send cancelled by host shutdown PhotoId={PhotoId} Stage={Stage} RetryCount={RetryCount}",
                photoId, stage, retryCount);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            await HandleSendFailureAsync(photo, stage, retryCount, _options.SendTimeout, ex, ct);
            return RecordOutcome.Failed;
        }
        catch (Exception ex)
        {
            await HandleSendFailureAsync(photo, stage, retryCount, null, ex, ct);
            return RecordOutcome.Failed;
        }
    }

    private async Task<int> ProcessBatchPlansAsync(IReadOnlyList<TtoSendRouter.SendPlan> batches, CancellationToken ct)
    {
        if (batches.Count == 0)
            return 0;

        var validBatches = new List<TtoSendRouter.SendPlan>(batches.Count);
        var attemptByPhotoId = new Dictionary<long, int>(batches.Count);

        foreach (var plan in batches)
        {
            ct.ThrowIfCancellationRequested();
            LogPlateIssues(plan.Photo);

            // ادعای تلاش برای مرحله‌ی TTO؛ رکوردِ رهاشده یا تمام‌شده دوباره انتخاب نمی‌شود.
            var attempt = await _queue.TryBeginAttemptAsync(
                plan.Photo.PhotoId, _options.EffectiveMaxSendAttempts, ct);
            if (attempt == 0)
                continue;

            attemptByPhotoId[plan.Photo.PhotoId] = attempt;

            try
            {
                ValidatePhotoReady(plan.Photo);
                TtoPreSendValidator.Validate(plan.Payload, null, null, requireImages: false);
                validBatches.Add(plan);
            }
            catch (Exception ex)
            {
                await HandleSendFailureAsync(plan.Photo, SendStage.ValidateTto, attempt - 1, null, ex, ct);
            }
        }

        if (validBatches.Count == 0)
            return 0;

        IReadOnlyList<InquiryInfoResult> inquiries;
        using (var batchCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            batchCts.CancelAfter(_options.SendTimeout);

            try
            {
                inquiries = await _rahadri.SendBatchAsync(validBatches.Select(b => b.Payload).ToList(), batchCts.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // شکست درخواست دسته‌ای: هر رکورد مستقل ناموفق ثبت می‌شود و صف متوقف نمی‌شود.
                foreach (var plan in validBatches)
                {
                    await HandleSendFailureAsync(
                        plan.Photo,
                        SendStage.AddTtoBatch,
                        attemptByPhotoId[plan.Photo.PhotoId] - 1,
                        _options.SendTimeout,
                        ex,
                        ct);
                }

                return 0;
            }
        }

        var inquiryByRef = inquiries
            .Where(i => i.ReferenceNo > 0)
            .GroupBy(i => i.ReferenceNo)
            .ToDictionary(g => g.Key, g => g.First());

        var sent = 0;
        foreach (var plan in validBatches)
        {
            var photoId = plan.Photo.PhotoId;
            var retryCount = attemptByPhotoId[photoId] - 1;

            if (!inquiryByRef.TryGetValue(photoId, out var inquiry))
            {
                await HandleSendFailureAsync(
                    plan.Photo,
                    SendStage.AddTtoBatch,
                    retryCount,
                    null,
                    new RahdariSendException(photoId, "Batch response missing InquiryInfo for record"),
                    ct);
                continue;
            }

            try
            {
                RahdariTtoClient.EnsureTtoSuccess(photoId, inquiry.ValidationCode);
                await _queue.MarkTtoRegisteredAsync(
                    photoId,
                    inquiry.PassInfoId,
                    inquiry.PackId,
                    plan.Photo.PassDatetime,
                    ct);

                _syncStatus.RecordSuccess(
                    "addTTOInfoBatch2",
                    $"Ref={photoId}: {ItsErrorCodes.Describe(inquiry.ValidationCode)}",
                    photoId,
                    inquiry.ValidationCode);

                if (inquiry.ValidationCode == ItsErrorCodes.AddTto.Duplicate)
                {
                    plan.Photo.TerminalTtoRegistered = true;
                    plan.Photo.TerminalPassInfoId = inquiry.PassInfoId > 0 ? inquiry.PassInfoId : plan.Photo.TerminalPassInfoId;
                }
            }
            catch (Exception ex)
            {
                await HandleSendFailureAsync(plan.Photo, SendStage.AddTtoBatch, retryCount, null, ex, ct);
                continue;
            }

            // مرحله‌ی ارسال تصویر بودجه‌ی تلاش مستقل دارد (MarkTtoRegisteredAsync شمارنده را صفر می‌کند).
            var payload = _rahdariOptions.SendImagesSeparately
                ? TtoPayloadFactory.Create(plan.Photo, _rahdariOptions)
                : plan.Payload;

            var outcome = await ProcessOneAsync(
                plan.Photo,
                () => payload,
                TryCompleteWithImagesAsync,
                SendStage.SendImage,
                ct);

            if (outcome == RecordOutcome.Sent)
                sent++;
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
        var plate = _images.BuildPlateImage(photo);
        var color = _images.BuildColorImage(photo, payload, plate);
        SaveRahdariImages(photo, color);
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

    private void SaveRahdariImages(CameraPhotoRecord photo, byte[] color)
    {
        if (!_rahdariOptions.SaveImagesToFolder)
            return;

        try
        {
            var watchRoot = _rahdariOptions.WatchRootPath;
            var dir = string.IsNullOrWhiteSpace(watchRoot)
                ? DefaultImageRootFolder
                : Path.Combine(watchRoot, ImageSaveSubFolder);
            Directory.CreateDirectory(dir);

            var name = BuildImageName(photo);
            if (color.Length > 0)
                File.WriteAllBytes(Path.Combine(dir, $"{name}_color.jpg"), color);
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
        if (!_plateIssuesLogged.TryAdd(photo.PhotoId, 0))
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

    /// <summary>
    /// ثبت ساخت‌یافته‌ی شکست یک رکورد. پس از رسیدن به سقف تلاش مجاز، رکورد «رها» می‌شود
    /// (از صف خارج و علامت‌گذاری می‌شود) تا دیگر جلوی پردازش تصاویر بعدی را نگیرد.
    /// </summary>
    private async Task HandleSendFailureAsync(
        CameraPhotoRecord photo,
        SendStage stage,
        int retryCount,
        TimeSpan? timeout,
        Exception ex,
        CancellationToken ct)
    {
        var photoId = ex is RahdariSendException rse ? rse.PhotoId : photo.PhotoId;
        var maxAttempts = _options.EffectiveMaxSendAttempts;
        var attempt = retryCount + 1;
        var isLastAttempt = attempt >= maxAttempts;

        if (ex is RahdariSendException { ItsErrorCode: ItsErrorCodes.AddImage.SendWindowExpired })
        {
            await AbandonExpiredImageSendAsync(photoId, ItsErrorCodes.AddImage.SendWindowExpired, ct);
            return;
        }

        var explained = RahdariResponseInterpreter.ExplainException(ex, "send");

        _logger.LogError(
            ex,
            "Image send failed PhotoId={PhotoId} Stage={Stage} RetryCount={RetryCount} Attempt={Attempt} of {MaxAttempts} Timeout={Timeout} LastAttempt={IsLastAttempt} — {Summary}",
            photoId,
            stage,
            retryCount,
            attempt,
            maxAttempts,
            timeout?.TotalSeconds.ToString("0") ?? "-",
            isLastAttempt,
            explained.Summary);

        _syncStatus.RecordFailure($"send/{stage}", ex, photoId);

        if (isLastAttempt)
        {
            var message =
                $"[{stage}] پس از {attempt} تلاش ناموفق رها شد — {explained.Summary}";

            if (await _queue.AbandonAfterMaxRetriesAsync(photoId, message, ct))
            {
                _logger.LogError(
                    "Record abandoned after {Attempts} attempt(s) — removed from the queue. PhotoId={PhotoId} Stage={Stage} Reason={Reason}",
                    attempt,
                    photoId,
                    stage,
                    explained.Summary);
            }

            return;
        }

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

    private int LogCycle(int processed, long startedAt)
    {
        if (processed > 0)
        {
            _logger.LogInformation(
                "Cycle finished. Sent {Count} photo(s) to Rahdari in {ElapsedMs}ms",
                processed,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds.ToString("0"));
        }

        return processed;
    }

    /// <summary>مرحله‌ی جاری پردازش هر رکورد — در لاگ‌های ساخت‌یافته گزارش می‌شود.</summary>
    public enum SendStage
    {
        ValidateTto,
        AddTtoBatch,
        AddTtoWithImages,
        SendImage,
    }

    private enum RecordOutcome
    {
        Sent,
        Failed,
        Skipped,
    }
}
