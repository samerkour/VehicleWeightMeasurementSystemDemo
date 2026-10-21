using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;

namespace RmtoSync.Data;

public sealed class CameraPhotoQueueRepository
{
    private readonly string _connectionString;
    private readonly IDbContextFactory<RmtoSyncDbContext> _contextFactory;
    private readonly ILogger<CameraPhotoQueueRepository> _logger;

    /// <summary>
    /// آیا ایستگاه سنسور سرعت/وزن دارد؟ در ایستگاه فقط-دوربین (ANPR) این ستون‌ها صفر می‌مانند،
    /// بنابراین الزام «وزن/سرعت غیرصفر» در صف معادل حذف کل ترافیک عادی است.
    /// </summary>
    private readonly bool _requireSensorData;

    /// <summary>مهلت مجاز AddImage (ساعت) از زمان عبور — از تنظیمات.</summary>
    private readonly int _imageAttachWindowHours;

    public string ConnectionString => _connectionString;

    public CameraPhotoQueueRepository(
        IConfiguration configuration,
        IDbContextFactory<RmtoSyncDbContext> contextFactory,
        IOptions<RahdariOptions> rahdariOptions,
        ILogger<CameraPhotoQueueRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        _contextFactory = contextFactory;
        _requireSensorData = !rahdariOptions.Value.AnprOnlyStation;
        _imageAttachWindowHours = rahdariOptions.Value.ImageAttachWindowHours > 0
            ? rahdariOptions.Value.ImageAttachWindowHours
            : RmtoSync.Its.ServiceLimits.ImageAttachHoursAfterPass;
        _logger = logger;
    }

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingTtoBatchAsync(
        int batchSize, int maxAttempts, TimeSpan retryBackoff, CancellationToken ct, int? lineId = null) =>
        QueryBatchAsync(batchSize, ttoRegistered: false, maxAttempts, retryBackoff, ct, lineId);

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingImageBatchAsync(
        int batchSize, int maxAttempts, TimeSpan retryBackoff, CancellationToken ct, int? lineId = null) =>
        QueryBatchAsync(batchSize, ttoRegistered: true, maxAttempts, retryBackoff, ct, lineId);

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
                    .SetProperty(p => p.TerminalImageDeadlineAt, passDateTime.AddHours(_imageAttachWindowHours))
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

    /// <summary>
    /// رکوردهایی را که ITS به‌دلیل عمر بیش از ۳۰ روز نمی‌پذیرد (کد ۱۱۳) به‌صورت صریح و با
    /// دلیل روشن از صف خارج می‌کند. این کار «نادیده‌گرفتن» نیست: تعداد و دلیل لاگ می‌شود و
    /// برای هر رکورد <c>TerminalLastError</c> ثبت می‌گردد.
    /// </summary>
    public async Task<int> AbandonBeyondMaxDataAgeAsync(int maxAgeDays, string errorMessage, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var cutoff = DateTime.Now.AddDays(-maxAgeDays);
        var now = DateTime.Now;

        return await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE cp
            SET cp.TerminalAbandoned   = 1,
                cp.TerminalSent        = 1,
                cp.TerminalSentAt      = {now},
                cp.TerminalLastError   = {errorMessage}
            FROM dbo.CameraPhotos AS cp
            WHERE cp.TerminalSent = 0
              AND ISNULL(cp.TerminalAbandoned, 0) = 0
              AND ISNULL(cp.TerminalTtoRegistered, 0) = 0
              AND ISNULL(cp.CapturedAt, cp.ImportedAt) < {cutoff}
              AND (
                    (LTRIM(RTRIM(ISNULL(cp.PlateP1, N''))) <> N''
                     AND LTRIM(RTRIM(ISNULL(cp.PlateP2, N''))) <> N''
                     AND LTRIM(RTRIM(ISNULL(cp.PlateP3, N''))) <> N''
                     AND LTRIM(RTRIM(ISNULL(cp.PlateP4, N''))) <> N'')
                    OR
                    (LTRIM(RTRIM(ISNULL(cp.PlateP2, N''))) <> N''
                     AND (LTRIM(RTRIM(ISNULL(cp.PlateP1, N''))) <> N''
                          OR LTRIM(RTRIM(ISNULL(cp.PlateP3, N''))) <> N''
                          OR LTRIM(RTRIM(ISNULL(cp.PlateP4, N''))) <> N''))
                  );
            """, ct);
    }

    public async Task<int> MarkExpiredImageWindowsAsync(string errorMessage, CancellationToken ct)    {
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
        CancellationToken ct,
        int? lineId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var now = DateTime.Now;
        var backoffCutoff = now - retryBackoff;

        var query =
            from p in context.CameraPhotoQueue.AsNoTracking()
            join l in context.Lines on p.LineId equals l.Id into lineJoin
            from l in lineJoin.DefaultIfEmpty()
            where !p.TerminalSent
                && !p.TerminalAbandoned
                && p.TerminalSendAttempts < maxAttempts
                && (p.TerminalTtoRegistered ?? false) == ttoRegistered
                && (p.TerminalImageExpired ?? false) == false

            orderby p.PhotoId
            select new QueueRow { Photo = p, LineCode = l.LineCode };

        // فاصله‌ی بین تلاش‌ها: رکوردی که همین حالا شکست خورده در این دور دوباره انتخاب نمی‌شود،
        // تا یک رکوردِ خراب کل پنجره‌ی batch را اشغال نکند و پردازش بقیه‌ی صف متوقف نشود.
        query = query.Where(r => r.Photo.TerminalLastAttemptAt == null || r.Photo.TerminalLastAttemptAt <= backoffCutoff);

        // محدودکردن به یک خط، برای تست‌های قطعی و برای ایستگاه‌هایی که هر خط را جدا همگام می‌کنند.
        if (lineId is not null)
            query = query.Where(r => r.Photo.LineId == lineId.Value);

        if (ttoRegistered)
        {
            // مرحله‌ی تصویر: رکورد قبلاً در ITS ثبت شده و تصویر آن باید برسد.
            // فیلتر پلاک/سنسور این‌جا فقط باعث می‌شود تصویرِ ثبت‌شده ارسال نشود.
            query = query.Where(r =>
                r.Photo.TerminalImageDeadlineAt != null
                && r.Photo.TerminalImageDeadlineAt > DateTime.Now);
        }
        else
        {
            // پلاک قابل استفاده (حروف + حداقل یک رقم) شرط واقعی ارسال است.
            // PlateReadStatus/PlateConfidence شرط ITS نیستند: رکوردهایی با پلاک کامل ولی
            // confidence=0 (۲۵٪ رکوردها) قبلاً بی‌صدا از صف حذف می‌شدند.
            query = query.Where(r =>
                ((r.Photo.PlateP1 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP2 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP3 ?? string.Empty).Trim() != string.Empty
                    && (r.Photo.PlateP4 ?? string.Empty).Trim() != string.Empty)
                || ((r.Photo.PlateP2 ?? string.Empty).Trim() != string.Empty
                    && ((r.Photo.PlateP1 ?? string.Empty).Trim() != string.Empty
                        || (r.Photo.PlateP3 ?? string.Empty).Trim() != string.Empty
                        || (r.Photo.PlateP4 ?? string.Empty).Trim() != string.Empty)));

            // ITS داده‌ی قدیمی‌تر از ۳۰ روز را نمی‌پذیرد (کد ۱۱۳). انتخاب چنین رکوردی فقط
            // سه تماس بی‌نتیجه و سپس رهاسازی می‌سازد؛ اصلاً وارد صف نمی‌شود.
            var maxDataAgeCutoff = now.AddDays(-Its.ServiceLimits.MaxDataAgeDays);
            query = query.Where(r =>
                (r.Photo.CapturedAt ?? r.Photo.ImportedAt) >= maxDataAgeCutoff);

            // فقط وقتی ایستگاه واقعاً سنسور دارد، نبودِ داده‌ی سنسور دلیل حذف است.
            // در ایستگاه ANPR این شرط‌ها ۳٬۹۶۱ رکورد با پلاک کامل و بدون تخلف را حذف می‌کرد.
            if (_requireSensorData)
            {
                query = query.Where(r =>
                    r.Photo.TotalWeight != 0
                    && r.Photo.AverageSpeed != 0
                    && r.Photo.VehicleSpeed != 0);
            }
        }

        List<QueueRow> rows;
        try
        {
            rows = await query.Take(batchSize).ToListAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (IsDatabaseFailure(ex))
        {
            // 🔥 ناسازگاری اسکیما (مثل «Invalid column name 'TerminalSent'») یا هر خطای
            // دسترسی به پایگاه‌داده نباید چرخه را متوقف کند: batch خالی برگردانده می‌شود تا
            // پردازش در چرخه‌ی بعدی ادامه پیدا کند. خطا پنهان نمی‌شود — با جزئیات کامل
            // ثبت می‌شود تا علت ریشه‌ای («کدام ستون/ویو کم است») قابل رفع باشد.
            //
            // نکته: EF Core خطاهای گذرا (نظیر «Cannot open database») را در
            // InvalidOperationException می‌پیچد و SqlException درون آن قرار می‌دهد،
            // بنابراین گرفتن مستقیم SqlException کافی نیست.
            _logger.LogError(
                ex,
                "Queue query failed for {Phase} — skipping this batch and continuing with the next cycle. " +
                "BatchSize={BatchSize} SchemaError={SchemaError}. " +
                "If this is 'Invalid column name' the database does not match the EF model: run {Remediation}.",
                ttoRegistered ? "TTO" : "image",
                batchSize,
                TryDescribeSchemaError(ex),
                SchemaValidator.RemediationScript);

            return Array.Empty<CameraPhotoRecord>();
        }

        var result = new List<CameraPhotoRecord>(rows.Count);
        foreach (var row in rows)
        {
            row.Photo.LineCode = row.LineCode ?? string.Empty;
            result.Add(row.Photo);
        }

        return result;
    }

    /// <summary>
    /// نام مفقود را از پیام «Invalid column name 'X'» / «Invalid object name 'X'» استخراج می‌کند
    /// تا لاگ علت ریشه‌ای را نشان دهد. SQL Server نام شیءِ والد را در پیام نمی‌آورد،
    /// بنابراین فقط نام ستون یا شیءِ مفقود گزارش می‌شود.
    /// </summary>
    public static string DescribeSchemaError(string sqlErrorMessage, int errorNumber)
    {
        var match = InvalidNameRegex.Match(sqlErrorMessage);
        if (!match.Success)
            return $"no (error {errorNumber})";

        return $"{match.Groups["kind"].Value} '{match.Groups["name"].Value}' " +
               $"does not exist in the database (error {errorNumber})";
    }

    private static string TryDescribeSchemaError(Exception ex)
    {
        // نام ستون/شیءِ مفقود ممکن است در لایه‌ی بالاتر (پیام پیچیده‌ی EF) یا در استثنای
        // درونی باشد؛ هر دو بررسی می‌شوند.
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException)
                return DescribeSchemaError(sqlException.Message, sqlException.Number);

            var described = DescribeSchemaError(current.Message, 0);
            if (described != "no (error 0)")
                return described;
        }

        return DescribeSchemaError(ex.Message, 0);
    }

    /// <summary>
    /// آیا این استثنا ریشه در دسترسی به پایگاه‌داده دارد؟
    /// EF Core هم <c>SqlException</c> را مستقیم پرتاب می‌کند و هم آن را داخل
    /// <c>InvalidOperationException</c> (خطاهای گذرا) می‌پیچد؛ هر دو باید گرفته شوند،
    /// ولی استثناهای برنامه‌نویسی نباید نادیده گرفته شوند.
    /// </summary>
    public static bool IsDatabaseFailure(Exception? ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is DbException or SqlException)
                return true;
        }

        return false;
    }

    private static readonly Regex InvalidNameRegex = new(
        @"Invalid\s+(?<kind>column|object)\s+name\s+'(?<name>[^']+)'",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// DTO پروژکشن صف. (ساختار tuple در درختِ عبارت EF قابل استفاده نیست، بنابراین
    /// به‌جای آن یک نوع نام‌دار به کار می‌رود.)
    /// </summary>
    private sealed class QueueRow
    {
        public CameraPhotoRecord Photo { get; set; } = new();
        public string? LineCode { get; set; }
    }
}
