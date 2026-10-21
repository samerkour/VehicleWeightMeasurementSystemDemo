using System.Data;
using Microsoft.Data.SqlClient;

namespace RmtoSync.Data;

/// <summary>
/// Point-in-time queue counters for the operator window. <c>Ready*</c> applies the same
/// eligibility rules as the send queue (plate, retry budget, retry backoff, 30-day age and —
/// outside ANPR-only stations — sensor data), so it shows what the next cycles can actually send.
/// </summary>
public sealed record QueueStatusSnapshot(
    int PendingTto,
    int PendingImages,
    int ReadyTto,
    int ReadyImages,
    int PendingWithError,
    int Abandoned,
    int SentToday,
    DateTime? LastSentAt,
    DateTime? LastAttemptAt,
    DateTime CapturedAt);

/// <summary>
/// Reads send-queue counters directly from <c>dbo.CameraPhotos</c> so the UI can show them
/// even before the sync service (and its DI container) is started. Never throws: a missing
/// schema or an unreachable database yields <c>null</c> and the UI falls back to a notice.
/// The <c>Vehicles</c> join is used only when sensor data is required (non-ANPR station),
/// keeping the periodic UI query cheap on ANPR-only deployments.
/// </summary>
public static class QueueStatusReader
{
    public static async Task<QueueStatusSnapshot?> ReadAsync(
        string? connectionString,
        int maxAttempts,
        TimeSpan retryBackoff,
        bool requireSensorData,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        try
        {
            var now = DateTime.Now;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = BuildSql(requireSensorData);
            command.CommandTimeout = 10;
            command.Parameters.Add(new SqlParameter("@since", SqlDbType.DateTime2) { Value = DateTime.Today });
            command.Parameters.Add(new SqlParameter("@now", SqlDbType.DateTime2) { Value = now });
            command.Parameters.Add(new SqlParameter("@backoffCutoff", SqlDbType.DateTime2)
            {
                Value = now - retryBackoff
            });
            command.Parameters.Add(new SqlParameter("@ageCutoff", SqlDbType.DateTime2)
            {
                Value = now.AddDays(-RmtoSync.Its.ServiceLimits.MaxDataAgeDays)
            });
            command.Parameters.Add(new SqlParameter("@maxAttempts", SqlDbType.Int)
            {
                Value = Math.Max(maxAttempts, 1)
            });

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
            if (!await reader.ReadAsync(ct))
                return null;

            int Int(int index) => reader.IsDBNull(index) ? 0 : Convert.ToInt32(reader.GetValue(index));
            DateTime? Date(int index) => reader.IsDBNull(index) ? null : reader.GetDateTime(index);

            return new QueueStatusSnapshot(
                PendingTto: Int(0),
                PendingImages: Int(1),
                ReadyTto: Int(2),
                ReadyImages: Int(3),
                PendingWithError: Int(4),
                Abandoned: Int(5),
                SentToday: Int(6),
                LastSentAt: Date(7),
                LastAttemptAt: Date(8),
                CapturedAt: now);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or TimeoutException)
        {
            return null;
        }
    }

    private static string BuildSql(bool requireSensorData) =>
        SqlTemplate
            .Replace("/*JOIN_VEHICLES*/", requireSensorData
                ? "INNER JOIN dbo.Vehicles AS v WITH (NOLOCK) ON v.Id = cp.VehicleId"
                : string.Empty)
            .Replace("/*SENSOR_FILTER*/", requireSensorData
                ? """
                  AND (ISNULL(v.TotalWeight, 0) <> 0
                       AND ISNULL(v.AverageSpeed, 0) <> 0
                       AND ISNULL(v.Speed, 0) <> 0)
                  """
                : string.Empty);

    private const string SqlTemplate = """
        SELECT
            SUM(CASE WHEN cp.TerminalSent = 0
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND ISNULL(cp.TerminalTtoRegistered, 0) = 0
                      AND ISNULL(cp.TerminalImageExpired, 0) = 0
                     THEN 1 ELSE 0 END) AS PendingTto,
            SUM(CASE WHEN cp.TerminalSent = 0
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND ISNULL(cp.TerminalTtoRegistered, 0) = 1
                      AND ISNULL(cp.TerminalImageExpired, 0) = 0
                     THEN 1 ELSE 0 END) AS PendingImages,
            SUM(CASE WHEN cp.TerminalSent = 0
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND ISNULL(cp.TerminalTtoRegistered, 0) = 0
                      AND ISNULL(cp.TerminalImageExpired, 0) = 0
                      AND cp.TerminalSendAttempts < @maxAttempts
                      AND (cp.TerminalLastAttemptAt IS NULL OR cp.TerminalLastAttemptAt <= @backoffCutoff)
                      AND ISNULL(cp.CapturedAt, cp.ImportedAt) >= @ageCutoff
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
                          )
                      /*SENSOR_FILTER*/
                     THEN 1 ELSE 0 END) AS ReadyTto,
            SUM(CASE WHEN cp.TerminalSent = 0
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND ISNULL(cp.TerminalTtoRegistered, 0) = 1
                      AND ISNULL(cp.TerminalImageExpired, 0) = 0
                      AND cp.TerminalSendAttempts < @maxAttempts
                      AND (cp.TerminalLastAttemptAt IS NULL OR cp.TerminalLastAttemptAt <= @backoffCutoff)
                      AND cp.TerminalImageDeadlineAt IS NOT NULL
                      AND cp.TerminalImageDeadlineAt > @now
                     THEN 1 ELSE 0 END) AS ReadyImages,
            SUM(CASE WHEN cp.TerminalSent = 0
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND LTRIM(RTRIM(ISNULL(cp.TerminalLastError, N''))) <> N''
                     THEN 1 ELSE 0 END) AS PendingWithError,
            SUM(CASE WHEN ISNULL(cp.TerminalAbandoned, 0) = 1
                     THEN 1 ELSE 0 END) AS Abandoned,
            SUM(CASE WHEN cp.TerminalSent = 1
                      AND ISNULL(cp.TerminalAbandoned, 0) = 0
                      AND ISNULL(cp.TerminalImageExpired, 0) = 0
                      AND cp.TerminalSentAt >= @since
                     THEN 1 ELSE 0 END) AS SentToday,
            MAX(cp.TerminalSentAt) AS LastSentAt,
            MAX(cp.TerminalLastAttemptAt) AS LastAttemptAt
        FROM dbo.CameraPhotos AS cp WITH (NOLOCK)
        /*JOIN_VEHICLES*/;
        """;
}
