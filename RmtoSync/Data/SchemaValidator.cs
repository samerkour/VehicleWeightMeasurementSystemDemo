using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RmtoSync.Data;

/// <summary>نتیجه‌ی بررسی یک شیء پایگاه‌داده (جدول یا ویو) در برابر مدل EF.</summary>
/// <param name="ObjectName">نام کامل شیء، مثلاً <c>vw_CameraFullData</c>.</param>
/// <param name="EntityName">نام موجودیت EF که به این شیء نگاشت شده است.</param>
/// <param name="IsView">آیا شیء یک ویو است (در برابر جدول)؟</param>
/// <param name="Exists">آیا شیء در پایگاه‌داده وجود دارد؟</param>
/// <param name="ActualType">نوع واقعی شیء در <c>sys.objects</c>؛ <c>null</c> یعنی وجود ندارد.</param>
/// <param name="MissingColumns">ستون‌هایی که مدل لازم دارد ولی در پایگاه‌داده نیستند.</param>
public sealed record SchemaObjectReport(
    string ObjectName,
    string EntityName,
    bool IsView,
    bool Exists,
    string? ActualType,
    IReadOnlyList<string> MissingColumns)
{
    public bool IsMissingObject => !Exists;

    /// <summary>آیا این شیء با مدل EF ناسازگار است؟</summary>
    public bool IsIncompatible => IsMissingObject || MissingColumns.Count > 0;
}

/// <summary>گزارش کامل اعتبارسنجی اسکیما.</summary>
public sealed record SchemaValidationReport(IReadOnlyList<SchemaObjectReport> Objects)
{
    public bool IsCompatible => Objects.All(o => !o.IsIncompatible);

    public IEnumerable<SchemaObjectReport> Incompatible => Objects.Where(o => o.IsIncompatible);
}

/// <summary>
/// مقایسه‌ی مدل EF با اسکیمای واقعی پایگاه‌داده در زمان راه‌اندازی.
///
/// چرا این کلاس لازم است: خطای «Invalid column name 'X'» به‌صورت استثنای
/// <c>SqlException</c> در دل کوئری LINQ ظاهر می‌شود؛ یعنی به‌جای اینکه بگوید کدام
/// ستون‌ها کم هستند، فقط یک نام می‌دهد و آن هم فقط برای اولین ستونِ مفقود. این کلاس
/// همه‌ی ستون‌های مفقود را یک‌جا و قبل از شروع کار گزارش می‌کند.
///
/// توجه: <c>CameraPhotoRecord</c> با <c>ToView("vw_CameraFullData")</c> نگاشت شده است،
/// پس ستون‌هایی مثل TotalWeight / VehicleClass / AxleWeight1..9 / PlateReadStatus متعلق به
/// <em>ویو</em> هستند نه جدول CameraPhotos. افزودن آن‌ها به جدول، خطا را برطرف نمی‌کند.
/// </summary>
public sealed class SchemaValidator(
    IDbContextFactory<RmtoSyncDbContext> contextFactory,
    ILogger<SchemaValidator> logger)
{
    /// <summary>مسیر اسکیپت اصلاحی که باید روی پایگاه‌داده‌ی هدف اجرا شود.</summary>
    public const string RemediationScript = "Database/AlignCameraPhotoSchema.sql";

    /// <summary>
    /// ستون‌های مورد انتظار مدل که در پایگاه‌داده وجود ندارند.
    /// اگر شیء اصلاً وجود نداشته باشد، همه‌ی ستون‌ها مفقود شمرده می‌شوند.
    /// </summary>
    public static IReadOnlyList<string> FindMissingColumns(
        IReadOnlyList<string> expectedColumns,
        bool objectExists,
        IReadOnlySet<string> actualColumns)
    {
        if (!objectExists)
            return expectedColumns.OrderBy(c => c, StringComparer.Ordinal).ToList();

        return expectedColumns
            .Where(c => !actualColumns.Contains(c))
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<SchemaValidationReport> ValidateAsync(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var expected = context.Model.GetEntityTypes()
            .Select(MapEntity)
            .OfType<EntityExpectation>()
            .OrderBy(x => x.ObjectName, StringComparer.Ordinal)
            .ToList();

        await using var connection = new SqlConnection(context.Database.GetConnectionString());
        await connection.OpenAsync(ct);

        var reports = new List<SchemaObjectReport>(expected.Count);
        foreach (var expectation in expected)
        {
            var (exists, actualType, actualColumns) =
                await ReadObjectAsync(connection, expectation.ObjectName, ct);

            var missing = FindMissingColumns(expectation.Columns, exists, actualColumns);

            reports.Add(new SchemaObjectReport(
                expectation.ObjectName,
                expectation.EntityName,
                expectation.IsView,
                exists,
                actualType,
                missing));
        }

        var report = new SchemaValidationReport(reports);
        Log(report);
        return report;
    }

    /// <summary>انتظار مدل EF از یک شیء پایگاه‌داده (نام شیء + ستون‌های لازم).</summary>
    private sealed record EntityExpectation(
        string ObjectName,
        string EntityName,
        bool IsView,
        IReadOnlyList<string> Columns);

    /// <summary>
    /// نگاشت موجودیت EF به نام شیء و ستون‌های مورد انتظار.
    /// موجودیت‌هایی که به هیچ شیءی نگاشت نشده‌اند (keyless/owned) نادیده گرفته می‌شوند.
    /// </summary>
    private static EntityExpectation? MapEntity(IEntityType entityType)
    {
        var viewName = entityType.GetViewName();
        var tableName = entityType.GetTableName();
        var isView = !string.IsNullOrEmpty(viewName);
        var objectName = isView ? viewName! : tableName;

        // موجودیت‌هایی که به شیء نگاشت نشده‌اند (keyless / owned) برای ما مهم نیستند.
        if (string.IsNullOrEmpty(objectName))
            return null;

        // نام ستون در ویو/جدول. اگر مدل آن را override نکرده باشد، همان نام پراپرتی است
        // (ToView/ToTable بدون HasColumnName ⇒ نام ستون = نام پراپرتی).
        var columns = entityType.GetProperties()
            .Select(property =>
            {
                var column = property.GetColumnName();
                return string.IsNullOrEmpty(column) ? property.Name : column!;
            })
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new EntityExpectation(objectName!, entityType.ClrType.Name, isView, columns);
    }

    private static async Task<(bool Exists, string? Type, HashSet<string> Columns)> ReadObjectAsync(
        SqlConnection connection, string objectName, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT  o.type, c.name
FROM    sys.objects o
LEFT JOIN sys.columns c ON c.object_id = o.object_id
WHERE   o.name = @name
  AND   o.type IN ('U', 'V')";

        command.Parameters.AddWithValue("@name", objectName);

        var exists = false;
        string? type = null;
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            exists = true;
            type ??= reader.IsDBNull(0) ? null : reader.GetString(0);
            if (!reader.IsDBNull(1))
                columns.Add(reader.GetString(1));
        }

        return (exists, type, columns);
    }

    private void Log(SchemaValidationReport report)
    {
        if (report.IsCompatible)
        {
            logger.LogInformation(
                "Database schema is compatible with the EF model. Objects={Count} ({Objects})",
                report.Objects.Count,
                string.Join(", ", report.Objects.Select(o => o.ObjectName)));
            return;
        }

        foreach (var objectReport in report.Incompatible)
        {
            if (objectReport.IsMissingObject)
            {
                logger.LogCritical(
                    "SCHEMA MISMATCH: {Object} is missing from the database. EF maps {Entity} to it, " +
                    "so every queue query fails with 'Invalid object name'. Run {Script}. Expected {Expected} object. SQL: CREATE VIEW dbo.{Object} AS ...",
                    objectReport.ObjectName,
                    objectReport.EntityName,
                    RemediationScript,
                    objectReport.IsView ? "a VIEW" : "a TABLE",
                    objectReport.ObjectName);
                continue;
            }

            // نوع شیء با انتظار EF نمی‌خواند (مثلاً جدول به‌جای ویو) ⇒ نگاشت ToView شکست می‌خورد.
            var expectedType = objectReport.IsView ? "V" : "U";
            if (string.Equals(objectReport.ActualType, expectedType, StringComparison.Ordinal) == false)
            {
                logger.LogCritical(
                    "SCHEMA MISMATCH: {Object} exists but is a {Actual}, while EF maps {Entity} to it as a {Expected}. Run {Script}.",
                    objectReport.ObjectName,
                    DescribeObjectType(objectReport.ActualType),
                    objectReport.EntityName,
                    objectReport.IsView ? "VIEW" : "TABLE",
                    RemediationScript);
            }

            logger.LogCritical(
                "SCHEMA MISMATCH: {Object} is missing {MissingCount} column(s) required by EF ({Entity}): {Columns}. " +
                "Queue queries will fail with 'Invalid column name'. Run {Script}.",
                objectReport.ObjectName,
                objectReport.MissingColumns.Count,
                objectReport.EntityName,
                string.Join(", ", objectReport.MissingColumns),
                RemediationScript);
        }

        logger.LogCritical(
            "Database schema does not match the EF model ({Count} object(s)). " +
            "RmtoSync will keep running but cannot send records until {Script} has been applied to {Database}.",
            report.Incompatible.Count(),
            RemediationScript,
            "(the RmtoSync connection string)");
    }

    private static string DescribeObjectType(string? type) => type switch
    {
        "U" => "TABLE",
        "V" => "VIEW",
        null => "missing object",
        _ => $"type '{type}'",
    };
}
