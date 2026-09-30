using Microsoft.EntityFrameworkCore;

namespace RmtoSync.Data;

public sealed class CameraPhotoQueueRepository
{
    private readonly string _connectionString;
    private readonly IDbContextFactory<RmtoSyncDbContext> _contextFactory;

    public string ConnectionString => _connectionString;

    public CameraPhotoQueueRepository(IConfiguration configuration, IDbContextFactory<RmtoSyncDbContext> contextFactory)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        _contextFactory = contextFactory;
    }

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingTtoBatchAsync(
        int batchSize, int maxAttempts, TimeSpan retryBackoff, CancellationToken ct) =>
        QueryBatchAsync(batchSize, ttoRegistered: false, maxAttempts, retryBackoff, ct);

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingImageBatchAsync(
        int batchSize, int maxAttempts, TimeSpan retryBackoff, CancellationToken ct) =>
        QueryBatchAsync(batchSize, ttoRegistered: true, maxAttempts, retryBackoff, ct);

    /// <summary>
    /// Atomically claims a record for one send attempt and returns the 1-based attempt number,
    /// or 0 when the record is no longer claimable (already sent, abandoned, or retry cap reached).
    /// The predicate is re-checked in SQL, so concurrent workers can never double-claim a record.
    /// </summary>
    public async Task<int> TryBeginAttemptAsync(long photoId, int maxAttempts, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;

        var affected = await context.CameraPhotos
            .Where(p => p.Id == photoId
                && !p.TerminalSent
                && !p.TerminalAbandoned
                && p.TerminalSendAttempts < maxAttempts)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalSendAttempts, p => p.TerminalSendAttempts + 1)
                    .SetProperty(p => p.TerminalLastAttemptAt, now),
                ct);

        if (affected == 0)
            return 0;

        return await context.CameraPhotos
            .Where(p => p.Id == photoId)
            .Select(p => p.TerminalSendAttempts)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Acknowledges a record that exhausted its retry budget: it leaves the pending queue
    /// (TerminalSent + TerminalAbandoned) so it can no longer block newer records.
    /// </summary>
    public async Task<bool> AbandonAfterMaxRetriesAsync(long photoId, string errorMessage, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;
        var affected = await context.CameraPhotos
            .Where(p => p.Id == photoId && !p.TerminalSent && !p.TerminalAbandoned)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalAbandoned, true)
                    .SetProperty(p => p.TerminalSent, true)
                    .SetProperty(p => p.TerminalSentAt, now)
                    .SetProperty(p => p.TerminalLastError, errorMessage),
                ct);
        return affected > 0;
    }

    public async Task MarkTtoRegisteredAsync(
        long photoId,
        long passInfoId,
        long packId,
        DateTime passDateTime,
        CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.CameraPhotos
            .Where(p => p.Id == photoId && !p.TerminalSent)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalTtoRegistered, true)
                    .SetProperty(p => p.TerminalTtoRegisteredAt, DateTime.UtcNow)
                    .SetProperty(p => p.TerminalImageDeadlineAt, passDateTime.AddHours(24))
                    .SetProperty(p => p.TerminalPassInfoId, passInfoId)
                    .SetProperty(p => p.TerminalPackId, packId)
                    .SetProperty(p => p.PassInfoId, passInfoId)
                    .SetProperty(p => p.TerminalLastError, string.Empty)
                    // مرحله‌ی TTO با موفقیت تمام شد؛ مرحله‌ی ارسال تصویر بودجه‌ی تلاش مستقل دارد.
                    .SetProperty(p => p.TerminalSendAttempts, 0)
                    .SetProperty(p => p.TerminalLastAttemptAt, (DateTime?)null),
                ct);
    }

    public async Task<bool> TryMarkTerminalSentAsync(long photoId, DateTime sentAt, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var affected = await context.CameraPhotos
            .Where(p => p.Id == photoId && !p.TerminalSent)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalSent, true)
                    .SetProperty(p => p.TerminalSentAt, sentAt)
                    .SetProperty(p => p.TerminalLastError, string.Empty),
                ct);
        return affected > 0;
    }

    public async Task RecordTerminalErrorAsync(long photoId, string errorMessage, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.CameraPhotos
            .Where(p => p.Id == photoId && !p.TerminalSent)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.TerminalLastError, errorMessage),
                ct);
    }

    public async Task<int> MarkExpiredImageWindowsAsync(string errorMessage, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;
        return await context.CameraPhotos
            .Where(p => !p.TerminalSent
                && (p.TerminalImageExpired ?? false) == false
                && p.TerminalTtoRegistered == true
                && p.TerminalImageDeadlineAt != null
                && p.TerminalImageDeadlineAt < now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalImageExpired, true)
                    .SetProperty(p => p.TerminalImageExpiredAt, now)
                    .SetProperty(p => p.TerminalLastError, errorMessage)
                    .SetProperty(p => p.TerminalSent, true)
                    .SetProperty(p => p.TerminalSentAt, now),
                ct);
    }

    public async Task<bool> AbandonExpiredImageSendAsync(long photoId, string errorMessage, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;
        var affected = await context.CameraPhotos
            .Where(p => p.Id == photoId
                && !p.TerminalSent
                && (p.TerminalImageExpired ?? false) == false)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.TerminalImageExpired, true)
                    .SetProperty(p => p.TerminalImageExpiredAt, now)
                    .SetProperty(p => p.TerminalLastError, errorMessage)
                    .SetProperty(p => p.TerminalSent, true)
                    .SetProperty(p => p.TerminalSentAt, now),
                ct);
        return affected > 0;
    }

    private async Task<IReadOnlyList<CameraPhotoRecord>> QueryBatchAsync(
        int batchSize,
        bool ttoRegistered,
        int maxAttempts,
        TimeSpan retryBackoff,
        CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;
        var backoffCutoff = now - retryBackoff;

        var query =
            from p in context.CameraPhotoQueue.AsNoTracking()
            join l in context.Lines on p.LineId equals l.Id
            where !p.TerminalSent
                && !p.TerminalAbandoned
                && p.TerminalSendAttempts < maxAttempts
                && (p.TerminalTtoRegistered ?? false) == ttoRegistered
                && (p.TerminalImageExpired ?? false) == false
                && p.PlateReadStatus == 1
                && p.TotalWeight != 0
                && p.AverageSpeed != 0
                && p.VehicleSpeed != 0
                && p.PlateConfidence != 0

            orderby p.PhotoId
            select new { Photo = p, LineCode = l.LineCode };

        // فاصله‌ی بین تلاش‌ها: رکوردی که همین حالا شکست خورده در این دور دوباره انتخاب نمی‌شود،
        // تا یک رکوردِ خراب کل پنجره‌ی batch را اشغال نکند و پردازش بقیه‌ی صف متوقف نشود.
        query = query.Where(r => r.Photo.TerminalLastAttemptAt == null || r.Photo.TerminalLastAttemptAt <= backoffCutoff);

        if (ttoRegistered)
        {
            query = query.Where(r =>
                r.Photo.TerminalImageDeadlineAt != null
                && r.Photo.TerminalImageDeadlineAt > DateTime.Now);
        }
        else
        {
            query = query.Where(r =>
                ((r.Photo.PlateP1 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP2 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP3 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP4 ?? string.Empty).Trim() != string.Empty)
                || ((r.Photo.PlateP2 ?? string.Empty).Trim() != string.Empty
                    && ((r.Photo.PlateP1 ?? string.Empty).Trim() != string.Empty
                        || (r.Photo.PlateP3 ?? string.Empty).Trim() != string.Empty
                        || (r.Photo.PlateP4 ?? string.Empty).Trim() != string.Empty)));
        }

        var rows = await query.Take(batchSize).ToListAsync(ct);

        var result = new List<CameraPhotoRecord>(rows.Count);
        foreach (var row in rows)
        {
            row.Photo.LineCode = row.LineCode ?? string.Empty;
            result.Add(row.Photo);
        }

        return result;
    }
}
