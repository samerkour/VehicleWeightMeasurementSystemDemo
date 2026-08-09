using Microsoft.EntityFrameworkCore;

namespace RmtoSync.Data;

public sealed class CameraPhotoQueueRepository
{
    private readonly string _connectionString;
    private readonly IDbContextFactory<RmtoSyncDbContext> _contextFactory;

    public string ConnectionString => _connectionString;

    public CameraPhotoQueueRepository(IConfiguration configuration, IDbContextFactory<RmtoSyncDbContext> contextFactory)
    {
        _connectionString = configuration.GetConnectionString("FarasooCamera")
            ?? throw new InvalidOperationException("Connection string 'FarasooCamera' is missing.");
        _contextFactory = contextFactory;
    }

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingTtoBatchAsync(int batchSize, CancellationToken ct) =>
        QueryBatchAsync(batchSize, ttoRegistered: false, ct);

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingImageBatchAsync(int batchSize, CancellationToken ct) =>
        QueryBatchAsync(batchSize, ttoRegistered: true, ct);

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
                    .SetProperty(p => p.TerminalLastError, (string?)null),
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
                    .SetProperty(p => p.TerminalLastError, (string?)null),
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
        CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var query =
            from p in context.CameraPhotoQueue.AsNoTracking()
            join l in context.Lines on p.LineId equals l.Id
            where !p.TerminalSent
                && (p.TerminalTtoRegistered ?? false) == ttoRegistered
                && (p.TerminalImageExpired ?? false) == false
                && p.PlateReadStatus == 1
            orderby p.PhotoId
            select new { Photo = p, LineCode = l.LineCode };

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
