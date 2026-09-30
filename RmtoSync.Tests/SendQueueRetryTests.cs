using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RmtoSync.Configuration;
using RmtoSync.Data;
using Xunit;
using Xunit.Abstractions;

namespace RmtoSync.Tests;

/// <summary>
/// Regression tests for the "stuck on one image" defect:
/// a record that always fails must not stay in the pending window forever and block newer records.
/// These run against the configured dev database and clean up after themselves.
/// </summary>
public class SendQueueRetryTests(ITestOutputHelper output)
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan NoBackoff = TimeSpan.Zero;

    [Fact]
    public async Task PoisonRecord_IsAbandonedAfterMaxAttempts_AndStopsBlockingQueue()
    {
        var ctx = await TestData.CreateAsync(output);
        if (ctx is null)
            return;

        try
        {
            var queue = ctx.Queue;

            // هر دو رکورد ابتدا در صف انتخاب می‌شوند.
            var first = await queue.GetPendingTtoBatchAsync(50, MaxAttempts, NoBackoff, default);
            Assert.Contains(first, p => p.PhotoId == ctx.PoisonPhotoId);
            Assert.Contains(first, p => p.PhotoId == ctx.HealthyPhotoId);

            // رکوردِ خراب تا سقف تلاش مجاز، هر بار ادعا و ناموفق می‌شود.
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                var claimed = await queue.TryBeginAttemptAsync(ctx.PoisonPhotoId, MaxAttempts, default);
                Assert.Equal(attempt, claimed);

                await queue.RecordTerminalErrorAsync(ctx.PoisonPhotoId, $"simulated failure #{attempt}", default);

                var isLastAttempt = claimed >= MaxAttempts;
                if (isLastAttempt)
                {
                    Assert.True(await queue.AbandonAfterMaxRetriesAsync(
                        ctx.PoisonPhotoId, "abandoned after 3 attempts", default));
                }
            }

            // سقف تلاش: دیگر قابل ادعا نیست، حتی اگر کسی مستقیم درخواست کند.
            Assert.Equal(0, await queue.TryBeginAttemptAsync(ctx.PoisonPhotoId, MaxAttempts, default));

            // رکوردِ رهاشده از صف خارج شده ولی رکورد سالم هنوز در صف است ⇒ صف جلو نمی‌افتد.
            var after = await queue.GetPendingTtoBatchAsync(50, MaxAttempts, NoBackoff, default);
            Assert.DoesNotContain(after, p => p.PhotoId == ctx.PoisonPhotoId);
            Assert.Contains(after, p => p.PhotoId == ctx.HealthyPhotoId);

            var row = await TestData.ReadStateAsync(ctx.ConnectionString, ctx.PoisonPhotoId);
            Assert.True(row.Abandoned, "TerminalAbandoned must be set");
            Assert.True(row.Sent, "abandoned record must leave the pending queue (TerminalSent)");
            Assert.Equal(MaxAttempts, row.Attempts);
            Assert.Equal("abandoned after 3 attempts", row.LastError);
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    [Fact]
    public async Task FailedRecord_IsNotRetriedImmediately_WhenBackoffIsConfigured()
    {
        var ctx = await TestData.CreateAsync(output);
        if (ctx is null)
            return;

        try
        {
            var queue = ctx.Queue;
            Assert.Equal(1, await queue.TryBeginAttemptAsync(ctx.PoisonPhotoId, MaxAttempts, default));
            await queue.RecordTerminalErrorAsync(ctx.PoisonPhotoId, "boom", default);

            // با backoff یک‌ساعته، رکوردی که همین حالا شکست خورده نباید در همین چرخه دوباره انتخاب شود.
            var withBackoff = await queue.GetPendingTtoBatchAsync(
                50, MaxAttempts, TimeSpan.FromHours(1), default);
            Assert.DoesNotContain(withBackoff, p => p.PhotoId == ctx.PoisonPhotoId);
            Assert.Contains(withBackoff, p => p.PhotoId == ctx.HealthyPhotoId);

            // بدون backoff، دوباره در دسترس است (تلاش مجدد).
            var withoutBackoff = await queue.GetPendingTtoBatchAsync(50, MaxAttempts, NoBackoff, default);
            Assert.Contains(withoutBackoff, p => p.PhotoId == ctx.PoisonPhotoId);
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    [Fact]
    public async Task TtoSuccess_ResetsAttempts_SoImagePhaseGetsFreshBudget()
    {
        var ctx = await TestData.CreateAsync(output);
        if (ctx is null)
            return;

        try
        {
            var queue = ctx.Queue;
            Assert.Equal(1, await queue.TryBeginAttemptAsync(ctx.PoisonPhotoId, MaxAttempts, default));
            await queue.TryBeginAttemptAsync(ctx.PoisonPhotoId, MaxAttempts, default);

            await queue.MarkTtoRegisteredAsync(
                ctx.PoisonPhotoId, passInfoId: 555, packId: 66, ctx.Timestamp, default);

            var row = await TestData.ReadStateAsync(ctx.ConnectionString, ctx.PoisonPhotoId);
            Assert.Equal(0, row.Attempts);
            Assert.Null(row.LastAttemptAt);
            Assert.True(row.TtoRegistered);
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    private sealed class TestData : IAsyncDisposable
    {
        private readonly string _lineCode;
        private readonly int _lineId;
        private readonly int _vehicleId;

        private TestData(
            string connectionString,
            CameraPhotoQueueRepository queue,
            long poisonPhotoId,
            long healthyPhotoId,
            string lineCode,
            int lineId,
            int vehicleId,
            DateTime timestamp)
        {
            ConnectionString = connectionString;
            Queue = queue;
            PoisonPhotoId = poisonPhotoId;
            HealthyPhotoId = healthyPhotoId;
            _lineCode = lineCode;
            _lineId = lineId;
            _vehicleId = vehicleId;
            Timestamp = timestamp;
        }

        public string ConnectionString { get; }
        public CameraPhotoQueueRepository Queue { get; }
        public long PoisonPhotoId { get; }
        public long HealthyPhotoId { get; }
        public DateTime Timestamp { get; }

        public static async Task<TestData?> CreateAsync(ITestOutputHelper output)
        {
            var repoRoot = new DirectoryInfo(AppContext.BaseDirectory);
            while (repoRoot is not null && !File.Exists(Path.Combine(repoRoot.FullName, "RmtoSync", "appsettings.json")))
                repoRoot = repoRoot.Parent;

            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(repoRoot!.FullName, "RmtoSync", "appsettings.json"), optional: false)
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")!;
            try
            {
                await using var probe = new SqlConnection(cs);
                await probe.OpenAsync();
            }
            catch (Exception ex)
            {
                output.WriteLine($"SKIPPED — dev database unavailable: {ex.Message}");
                return null;
            }

            var services = new ServiceCollection();
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
            services.AddSingleton<IConfiguration>(config);
            services.AddDbContextFactory<RmtoSyncDbContext>(o => o.UseSqlServer(cs));
            services.AddSingleton<CameraPhotoQueueRepository>();
            var provider = services.BuildServiceProvider();

            var lineCode = "ZTEST" + Guid.NewGuid().ToString("N")[..6];
            var marker = "ZZSENDTEST" + Guid.NewGuid().ToString("N")[..6];
            var now = DateTime.Now;

            await using (var conn = new SqlConnection(cs))
            {
                await conn.OpenAsync();

                int lineId;
                int vehicleId;

                await using (var seed = new SqlCommand(
                    """
                    INSERT INTO Lines (LineCode, LineName) VALUES (@lineCode, N'RmtoSync send-queue test');
                    DECLARE @lineId int = SCOPE_IDENTITY();
                    INSERT INTO Vehicles (LineId, Timestamp) VALUES (@lineId, @ts);
                    SELECT @lineId AS LineId, CAST(SCOPE_IDENTITY() AS int) AS VehicleId;
                    """, conn))
                {
                    seed.Parameters.AddWithValue("@lineCode", lineCode);
                    seed.Parameters.AddWithValue("@ts", now);
                    await using var seedReader = await seed.ExecuteReaderAsync();
                    await seedReader.ReadAsync();
                    lineId = seedReader.GetInt32(0);
                    vehicleId = seedReader.GetInt32(1);
                }

                async Task<long> InsertPhotoAsync(string name)
                {
                    await using var cmd = new SqlCommand(
                        """
                        INSERT INTO CameraPhotos (VehicleId, FileName, FullPath, CapturedAt, ImportedAt, PlateP1, PlateP2, PlateP3, PlateP4, TerminalSent, TerminalTtoRegistered, TerminalImageExpired)
                        OUTPUT INSERTED.Id
                        VALUES (@vehicleId, @marker + '_' + @name + '.jpg', @fullPath, @ts, @ts, N'11', N'ب', N'123', N'12', 0, 0, 0);
                        """, conn);
                    cmd.Parameters.AddWithValue("@vehicleId", vehicleId);
                    cmd.Parameters.AddWithValue("@marker", marker);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@fullPath", $@"C:\nonexistent\{name}.jpg");
                    cmd.Parameters.AddWithValue("@ts", now);

                    var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                    return id;
                }

                var poisonPhotoId = await InsertPhotoAsync("poison");
                var healthyPhotoId = await InsertPhotoAsync("healthy");

                // مقادیر لازم برای عبور از فیلتر صف (وزن/سرعت/اطمینان پلاک و وضعیت پایه)
                await using (var setup = new SqlCommand(
                    $"""
                    UPDATE v SET PlateReadStatus = 1, PlateConfidence = 0.95, Speed = 50, AverageSpeed = 48,
                                  TotalWeight = 12000, VehicleClass = 1
                    FROM Vehicles v WHERE v.Id = @vehicleId;
                    """, conn))
                {
                    setup.Parameters.AddWithValue("@vehicleId", vehicleId);
                    await setup.ExecuteNonQueryAsync();
                }

                return new TestData(
                    cs,
                    provider.GetRequiredService<CameraPhotoQueueRepository>(),
                    poisonPhotoId,
                    healthyPhotoId,
                    lineCode,
                    lineId,
                    vehicleId,
                    now);
            }
        }

        public static async Task<(bool Abandoned, bool Sent, bool TtoRegistered, int Attempts, DateTime? LastAttemptAt, string? LastError)>
            ReadStateAsync(string connectionString, long photoId)
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                """
                SELECT ISNULL(TerminalAbandoned, 0), ISNULL(TerminalSent, 0), ISNULL(TerminalTtoRegistered, 0),
                       ISNULL(TerminalSendAttempts, 0), TerminalLastAttemptAt, TerminalLastError
                FROM CameraPhotos WHERE Id = @id;
                """, conn);
            cmd.Parameters.AddWithValue("@id", photoId);

            await using var r = await cmd.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync(), "test record disappeared");
            return (
                r.GetBoolean(0),
                r.GetBoolean(1),
                r.GetBoolean(2),
                r.GetInt32(3),
                r.IsDBNull(4) ? null : r.GetDateTime(4),
                r.IsDBNull(5) ? null : r.GetString(5));
        }

        public async ValueTask DisposeAsync()
        {
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                $"""
                DELETE FROM CameraPhotos WHERE VehicleId = @vehicleId;
                DELETE FROM Vehicles WHERE Id = @vehicleId;
                DELETE FROM Lines WHERE Id = @lineId;
                """, conn);
            cmd.Parameters.AddWithValue("@vehicleId", _vehicleId);
            cmd.Parameters.AddWithValue("@lineId", _lineId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
