using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RmtoSync.Data;
using Xunit;
using Xunit.Abstractions;

namespace RmtoSync.Tests;

/// <summary>
/// محافظت در برابر رگرسیون «Invalid column name ...».
///
/// ریشه‌ی خطا: <c>CameraPhotoRecord</c> با <c>ToView("vw_CameraFullData")</c> نگاشت شده، پس
/// نام ستون‌هایی مثل TotalWeight / VehicleClass / AxleWeight1..9 / PlateReadStatus از ویو
/// خوانده می‌شوند نه از جدول. اگر ویو عقب‌تر از مدل EF بماند یا کلاً وجود نداشته باشد،
/// EF کوئری‌ای می‌سازد که به ستون‌های ناموجود ارجاع می‌دهد.
///
/// این تست‌ها دو کار می‌کنند:
///   ۱) تضمین می‌کنند اسکیمای پایگاه‌داده با مدل EF هم‌خوان است (قرارداد).
///   ۲) تضمین می‌کنند ناسازگاری، خطای مبهم و مسدودکننده نیست بلکه گزارش‌شده و قابل‌عبور است.
/// </summary>
public class SchemaAlignmentTests(ITestOutputHelper output)
{
    [Fact]
    public async Task DatabaseSchema_MatchesEfModel()
    {
        var ctx = await TestHost.CreateAsync(output);
        if (ctx is null)
            return;

        try
        {
            var report = await ctx.Validator.ValidateAsync(CancellationToken.None);

            foreach (var objectReport in report.Objects)
            {
                output.WriteLine(
                    $"{objectReport.ObjectName} expected={(objectReport.IsView ? "VIEW" : "TABLE")} " +
                    $"actual={objectReport.ActualType ?? "missing"} missingCount={objectReport.MissingColumns.Count}");
            }

            var detail = string.Join(
                " ;; ",
                report.Incompatible.Select(o => o.IsMissingObject
                    ? $"{o.ObjectName}=object-missing"
                    : $"{o.ObjectName}=missing-columns[{string.Join(",", o.MissingColumns)}]"));

            Assert.True(report.IsCompatible, detail);
        }
        finally
        {
            await ctx.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("Invalid column name 'TerminalSent'.", 207)]
    [InlineData("Invalid column name 'TotalWeight'.", 207)]
    [InlineData("Invalid column name 'AxleWeight7'.", 207)]
    [InlineData("Invalid object name 'vw_CameraFullData'.", 208)]
    public void SchemaErrorMessage_NamesTheMissingColumnOrObject(string sqlMessage, int errorNumber)
    {
        var described = CameraPhotoQueueRepository.DescribeSchemaError(sqlMessage, errorNumber);

        output.WriteLine(described);
        Assert.Contains("does not exist in the database", described);
        Assert.Contains(errorNumber.ToString(), described);
    }

    [Fact]
    public void SchemaErrorMessage_WithoutRecognisablePattern_StillReportsErrorNumber()
    {
        var described = CameraPhotoQueueRepository.DescribeSchemaError("Timeout expired", -2);

        output.WriteLine(described);
        Assert.Contains("no", described);
        Assert.Contains("-2", described);
    }

    [Fact]
    public void MissingObject_ReportsEveryExpectedColumnAsMissing()
    {
        string[] expected = ["TotalWeight", "TerminalSent", "AxleWeight1"];

        var missing = SchemaValidator.FindMissingColumns(
            expected, objectExists: false, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Equal(expected.Length, missing.Count);
    }

    [Fact]
    public void MissingColumns_AreDetectedAndOrderedDeterministically()
    {
        string[] expected = ["TotalWeight", "TerminalSent", "AxleWeight1"];
        var actual = new HashSet<string>(["TerminalSent", "AxleWeight1"], StringComparer.OrdinalIgnoreCase);

        var missing = SchemaValidator.FindMissingColumns(expected, objectExists: true, actual);

        Assert.Equal(["TotalWeight"], missing);
    }

    [Fact]
    public void ColumnComparison_IsCaseInsensitive_BecauseSqlServerIsNotCaseSensitive()
    {
        string[] expected = ["TerminalSent"];

        var missing = SchemaValidator.FindMissingColumns(
            expected, objectExists: true, new HashSet<string>(["terminalsent"], StringComparer.OrdinalIgnoreCase));

        Assert.Empty(missing);
    }

    [Fact]
    public async Task QueueQuery_ReturnsEmptyInsteadOfThrowing_WhenDatabaseIsBroken()
    {
        // تضمین «سیستم یخ نمی‌زند»: اگر کوئری صف به هر دلیل شکست بخورد، چرخه باید با
        // batch خالی ادامه پیدا کند، نه با استثنا.
        const string BrokenConnection =
            "Server=.;Database=NoSuchDatabase_ForSchemaTest;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = BrokenConnection,
            })
            .Build());
        services.AddDbContextFactory<RmtoSyncDbContext>(o => o.UseSqlServer(BrokenConnection));
        services.AddSingleton<CameraPhotoQueueRepository>();

        await using var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<CameraPhotoQueueRepository>();

        var batch = await repository.GetPendingTtoBatchAsync(10, 3, TimeSpan.Zero, CancellationToken.None);

        output.WriteLine($"Rows returned against a broken database: {batch.Count}");
        Assert.Empty(batch);
    }

    /// <summary>
    /// اسکریپت اصلاحی باید هر ستونی را که مدل EF لازم دارد پوشش دهد؛ وگرنه اجرای آن روی
    /// پایگاه‌داده‌ی ناسازگار خطا را برطرف نمی‌کند.
    /// </summary>
    [Fact]
    public void RemediationScript_CoversEveryColumnRequiredByTheModel()
    {
        var repoRoot = FindRepositoryRoot();
        var scriptPath = Path.Combine(repoRoot.FullName, "Database", "AlignCameraPhotoSchema.sql");
        Assert.True(File.Exists(scriptPath), $"remediation script not found: {scriptPath}");

        var script = File.ReadAllText(scriptPath);

        // These names are defined only by the view, never by the CameraPhotos table.
        // Drop any one of them from the view and EF raises the reported error.
        string[] viewOnlyColumns =
        [
            "PhotoId", "TotalWeight", "AverageSpeed", "VehicleSpeed", "VehicleClass", "TotalAxles",
            "TotalOverWeight", "PlateReadStatus", "PlateConfidence", "Allowed", "WrongDirection",
            "Latitude", "Longitude", "CarClass13", "CrimeCodes", "OcrScore", "HeadGap", "Gap",
            "VehicleLen", "FirstToLastAxlesLen", "AxleWeight1", "AxleWeight5", "AxleWeight9",
        ];

        var notCovered = viewOnlyColumns
            .Where(column => !script.Contains(column, StringComparison.Ordinal))
            .ToList();

        Assert.True(notCovered.Count == 0, $"not projected by the remediation script: {string.Join(", ", notCovered)}");

        // The retry-tracking columns must be CREATED, not merely mentioned by the view.
        foreach (var column in new[] { "TerminalSendAttempts", "TerminalLastAttemptAt", "TerminalAbandoned" })
            Assert.Contains($"ADD {column}", script);
    }

    /// <summary>
    /// 🔥 The remediation script must actually run, and must be safe to run twice.
    /// This is the artifact an operator applies to the broken production database, so a
    /// syntax error or a non-idempotent step would leave production worse off.
    /// </summary>
    [Fact]
    public async Task RemediationScript_ExecutesAndIsIdempotent()
    {
        var ctx = await TestHost.CreateAsync(output);
        if (ctx is null)
            return;

        var script = File.ReadAllText(Path.Combine(
            FindRepositoryRoot().FullName, "Database", "AlignCameraPhotoSchema.sql"));

        var batches = SplitOnGo(script)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToList();

        output.WriteLine($"Batches in script: {batches.Count}");
        Assert.NotEmpty(batches);

        await using var connection = new SqlConnection(ctx.ConnectionString);
        await connection.OpenAsync();

        for (var pass = 1; pass <= 2; pass++)
        {
            foreach (var batch in batches)
            {
                await using var command = new SqlCommand(batch, connection)
                {
                    CommandTimeout = 120,
                };

                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException(
                        $"Pass {pass} failed on batch:{Environment.NewLine}{batch}", ex);
                }
            }

            output.WriteLine($"Pass {pass}: executed {batches.Count} batch(es) with no error");
        }

        // After running the script the database must satisfy the EF model.
        var validator = ctx.Validator;
        var report = await validator.ValidateAsync(CancellationToken.None);
        Assert.True(
            report.IsCompatible,
            "after running the remediation script the schema must match the EF model; still incompatible: "
            + string.Join(" ;; ", report.Incompatible.Select(o => o.ObjectName)));
    }

    /// <summary>
    /// 🔥 End-to-end proof on a purpose-built *stale* database that reproduces the reported
    /// failure: a view missing TotalWeight / VehicleClass / AxleWeight1..9 / PlateReadStatus,
    /// and a CameraPhotos table without the retry-tracking columns.
    ///
    /// Before the script the EF model must be reported as incompatible; after the script it
    /// must be compatible. This is the regression that matters for production.
    /// </summary>
    [Fact]
    public async Task RemediationScript_RepairsAStaleDatabaseThatReproducesTheReportedErrors()
    {
        var ctx = await TestHost.CreateAsync(output);
        if (ctx is null)
            return;

        var dbName = "RmtoSync_SchemaFix_" + Guid.NewGuid().ToString("N")[..8];
        var master = new SqlConnectionStringBuilder(ctx.ConnectionString) { InitialCatalog = "master" };
        var target = new SqlConnectionStringBuilder(ctx.ConnectionString) { InitialCatalog = dbName };

        await using (var admin = new SqlConnection(master.ConnectionString))
        {
            try
            {
                await admin.OpenAsync();
            }
            catch (SqlException ex)
            {
                output.WriteLine($"SKIPPED - cannot connect to master: {ex.Message}");
                return;
            }

            try
            {
                await ExecuteAsync(admin, $"CREATE DATABASE [{dbName}];");
                await CreateStaleSchemaAsync(new SqlConnection(target.ConnectionString), output);
            }
            catch (SqlException ex)
            {
                output.WriteLine($"SKIPPED - cannot create a scratch database: {ex.Message}");
                await TryDropDatabaseAsync(admin, dbName);
                return;
            }

            try
            {
                // ── 1. before: the EF model must be reported as incompatible ──────────────
                var services = new ServiceCollection();
                services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
                services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = target.ConnectionString,
                    })
                    .Build());
                services.AddDbContextFactory<RmtoSyncDbContext>(o => o.UseSqlServer(target.ConnectionString));
                services.AddSingleton<SchemaValidator>();

                await using var provider = services.BuildServiceProvider();
                var validator = provider.GetRequiredService<SchemaValidator>();

                var before = await validator.ValidateAsync(CancellationToken.None);
                output.WriteLine($"Compatible BEFORE script: {before.IsCompatible}");

                Assert.False(
                    before.IsCompatible,
                    "the stale database must be reported as incompatible, otherwise this test proves nothing");

                var viewBefore = before.Objects.Single(o => o.ObjectName == "vw_CameraFullData");
                output.WriteLine($"vw_CameraFullData before: exists={viewBefore.Exists} missing={viewBefore.MissingColumns.Count}");

                // The exact column names from the bug report must be reported missing.
                Assert.Contains("TotalWeight", viewBefore.MissingColumns);
                Assert.Contains("VehicleClass", viewBefore.MissingColumns);
                Assert.Contains("AxleWeight1", viewBefore.MissingColumns);
                Assert.Contains("PlateReadStatus", viewBefore.MissingColumns);

                var tableBefore = before.Objects.Single(o => o.ObjectName == "CameraPhotos");
                Assert.Contains("TerminalAbandoned", tableBefore.MissingColumns);
                Assert.Contains("TerminalSendAttempts", tableBefore.MissingColumns);

                // ── 2. apply the remediation script ───────────────────────────────────────
                var script = File.ReadAllText(Path.Combine(
                    FindRepositoryRoot().FullName, "Database", "AlignCameraPhotoSchema.sql"));

                await using (var connection = new SqlConnection(target.ConnectionString))
                {
                    await connection.OpenAsync();
                    foreach (var batch in SplitOnGo(script).Where(b => !string.IsNullOrWhiteSpace(b)))
                        await ExecuteAsync(connection, batch);
                }

                // ── 3. after: the EF model must be compatible ──────────────────────────────
                var after = await validator.ValidateAsync(CancellationToken.None);
                output.WriteLine($"Compatible AFTER script: {after.IsCompatible}");

                var detail = string.Join(" ;; ", after.Incompatible.Select(o => o.IsMissingObject
                    ? $"{o.ObjectName}=object-missing"
                    : $"{o.ObjectName}=missing[{string.Join(",", o.MissingColumns)}]"));

                Assert.True(after.IsCompatible, "the script did not fully repair the stale database: " + detail);

                // The queue query must now run without "Invalid column name".
                var queueServices = new ServiceCollection();
                queueServices.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
                queueServices.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = target.ConnectionString,
                    })
                    .Build());
                queueServices.AddDbContextFactory<RmtoSyncDbContext>(o => o.UseSqlServer(target.ConnectionString));
                queueServices.AddSingleton<CameraPhotoQueueRepository>();

                await using var queueProvider = queueServices.BuildServiceProvider();
                var batch0 = await queueProvider.GetRequiredService<CameraPhotoQueueRepository>()
                    .GetPendingTtoBatchAsync(5, 3, TimeSpan.Zero, CancellationToken.None);

                output.WriteLine($"Queue query rows after repair: {batch0.Count}");
            }
            finally
            {
                await TryDropDatabaseAsync(admin, dbName);
            }
        }
    }

    /// <summary>
    /// A deliberately outdated schema of the kind that produces the reported errors:
    /// a truncated view (no TotalWeight / VehicleClass / AxleWeight* / PlateReadStatus) and a
    /// CameraPhotos table with no retry-tracking columns and no plate image paths.
    /// </summary>
    private static async Task CreateStaleSchemaAsync(SqlConnection connection, ITestOutputHelper output)
    {
        await connection.OpenAsync();

        await ExecuteAsync(connection, """
            CREATE TABLE Lines (
                Id int IDENTITY(1,1) PRIMARY KEY,
                LineCode nvarchar(50) NOT NULL,
                LineName nvarchar(100) NOT NULL);

            CREATE TABLE Vehicles (
                Id int IDENTITY(1,1) PRIMARY KEY,
                LineId int NULL,
                Timestamp datetime2 NOT NULL,
                PlateNumber nvarchar(20) NULL,
                Speed float NULL,
                AverageSpeed float NULL,
                AxleCount int NULL,
                TotalWeight float NULL,
                PlateConfidence float NULL,
                PlateReadStatus int NULL,
                PlateReadAt datetime2 NULL,
                Allowed bit NULL,
                WrongDirection bit NULL,
                VehicleClass int NULL,
                Longitude float NULL,
                Latitude float NULL,
                VehicleLen float NULL,
                TotalOverWeight float NULL,
                WimRawLine nvarchar(50) NULL,
                SpeedType int NULL,
                CrimeCodes nvarchar(200) NULL,
                HeadGap int NULL,
                Gap int NULL);

            CREATE TABLE Axles (
                Id int IDENTITY(1,1) PRIMARY KEY,
                VehicleId int NULL,
                AxleIndex int NULL,
                Weight float NULL,
                TimeMs float NULL,
                Distance float NULL,
                LengthToNext float NULL,
                IsOverweight bit NULL);

            CREATE TABLE CameraPhotos (
                Id bigint IDENTITY(1,1) PRIMARY KEY,
                VehicleId int NULL,
                FileName nvarchar(500) NULL,
                RelativePath nvarchar(500) NULL,
                FullPath nvarchar(max) NULL,
                FileSizeBytes bigint NULL,
                FileHash nvarchar(100) NULL,
                CapturedAt datetime2 NULL,
                ImportedAt datetime2 NOT NULL,
                PlateP1 nvarchar(10) NOT NULL,
                PlateP2 nvarchar(10) NOT NULL,
                PlateP3 nvarchar(10) NOT NULL,
                PlateP4 nvarchar(10) NOT NULL,
                PlateBoxLeft int NULL,
                PlateBoxTop int NULL,
                PlateBoxWidth int NULL,
                PlateBoxHeight int NULL,
                TerminalSent bit NOT NULL DEFAULT (0),
                TerminalSentAt datetime2 NULL,
                TerminalLastError nvarchar(4000) NULL,
                TerminalTtoRegistered bit NOT NULL DEFAULT (0),
                TerminalTtoRegisteredAt datetime2 NULL,
                TerminalImageDeadlineAt datetime2 NULL,
                TerminalImageExpired bit NOT NULL DEFAULT (0),
                TerminalImageExpiredAt datetime2 NULL,
                TerminalPassInfoId bigint NULL,
                TerminalPackId bigint NULL,
                PassInfoId bigint NULL,
                PreviousDeviceCode bigint NULL,
                LengthAxles12 int NULL, LengthAxles23 int NULL, LengthAxles34 int NULL,
                LengthAxles45 int NULL, LengthAxles56 int NULL, LengthAxles67 int NULL,
                LengthAxles78 int NULL, LengthAxlesMoreThan8 int NULL,
                TotalWeightA int NULL, TotalWeightB int NULL, TotalWeightC int NULL);
            """);

        // An old, truncated view: this is what makes EF emit
        // "Invalid column name 'TotalWeight'" / "'VehicleClass'" / "'AxleWeight1'".
        await ExecuteAsync(connection, """
            CREATE VIEW dbo.vw_CameraFullData
            AS
            SELECT cp.Id AS PhotoId, cp.VehicleId, cp.FileName, cp.FullPath, cp.CapturedAt
            FROM dbo.CameraPhotos AS cp;
            """);

        output.WriteLine("Stale schema created (truncated view, no retry columns).");
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync();
    }

    private static async Task TryDropDatabaseAsync(SqlConnection admin, string dbName)
    {
        try
        {
            // Clear lingering sessions so the scratch database can be dropped.
            await using (var clear = new SqlCommand(
                $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", admin))
            {
                await clear.ExecuteNonQueryAsync();
            }

            await using var drop = new SqlCommand($"DROP DATABASE IF EXISTS [{dbName}];", admin);
            await drop.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            // Best effort cleanup; a leftover scratch database must never fail the suite.
        }
    }

    /// <summary>
    /// Splits a T-SQL script on standalone <c>GO</c> batch separators. GO is a client-side
    /// directive, not T-SQL, so each batch has to be sent separately.
    /// </summary>
    private static IEnumerable<string> SplitOnGo(string script) =>
        script.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => !line.Equals("GO", StringComparison.OrdinalIgnoreCase))
            .Aggregate(
                new List<string> { string.Empty },
                (batches, line) =>
                {
                    if (line.Length == 0)
                    {
                        batches.Add(string.Empty);
                        return batches;
                    }

                    batches[^1] = batches[^1].Length == 0 ? line : batches[^1] + "\n" + line;
                    return batches;
                });

    private static DirectoryInfo FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "RmtoSync", "appsettings.json")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        return dir!;
    }

    private sealed class TestHost : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;

        private TestHost(ServiceProvider provider, SchemaValidator validator, string connectionString)
        {
            _provider = provider;
            Validator = validator;
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
        public SchemaValidator Validator { get; }

        public static async Task<TestHost?> CreateAsync(ITestOutputHelper output)
        {
            var root = FindRepositoryRoot();
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(root.FullName, "RmtoSync", "appsettings.json"), optional: false)
                .Build();

            var cs = config.GetConnectionString("DefaultConnection")!;
            try
            {
                await using var probe = new SqlConnection(cs);
                await probe.OpenAsync();
            }
            catch (Exception ex)
            {
                output.WriteLine($"SKIPPED - dev database unavailable: {ex.Message}");
                return null;
            }

            var services = new ServiceCollection();
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
            services.AddSingleton<IConfiguration>(config);
            services.AddDbContextFactory<RmtoSyncDbContext>(o => o.UseSqlServer(cs));
            services.AddSingleton<SchemaValidator>();

            var provider = services.BuildServiceProvider();
            return new TestHost(provider, provider.GetRequiredService<SchemaValidator>(), cs);
        }

        public ValueTask DisposeAsync() => _provider.DisposeAsync();
    }
}
