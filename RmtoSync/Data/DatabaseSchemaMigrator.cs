using Microsoft.Data.SqlClient;

namespace RmtoSync.Data;

/// <summary>Runtime schema patch for RmtoSync (full baseline: sql/Setup.sql).</summary>
public static class DatabaseSchemaMigrator
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static volatile bool _applied;

    private static readonly string[] Statements =
    [
        """
        IF COL_LENGTH('dbo.CameraPhotos', 'TerminalTtoRegisteredAt') IS NULL
            ALTER TABLE dbo.CameraPhotos ADD TerminalTtoRegisteredAt DATETIME2(3) NULL
        """,
        """
        IF COL_LENGTH('dbo.CameraPhotos', 'TerminalImageDeadlineAt') IS NULL
            ALTER TABLE dbo.CameraPhotos ADD TerminalImageDeadlineAt DATETIME2(3) NULL
        """,
        """
        IF COL_LENGTH('dbo.CameraPhotos', 'TerminalImageExpired') IS NULL
            ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpired BIT NOT NULL
                CONSTRAINT DF_CameraPhotos_TerminalImageExpired DEFAULT (0)
        """,
        """
        IF COL_LENGTH('dbo.CameraPhotos', 'TerminalImageExpiredAt') IS NULL
            ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpiredAt DATETIME2(3) NULL
        """,
        """
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_CameraPhotos_TerminalImagePending'
              AND object_id = OBJECT_ID(N'dbo.CameraPhotos')
        )
            CREATE NONCLUSTERED INDEX IX_CameraPhotos_TerminalImagePending
                ON dbo.CameraPhotos (TerminalImageDeadlineAt, Id)
                WHERE TerminalSent = 0 AND TerminalTtoRegistered = 1
        """,
        """
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_CameraPhotos_TerminalTtoPending'
              AND object_id = OBJECT_ID(N'dbo.CameraPhotos')
        )
            CREATE NONCLUSTERED INDEX IX_CameraPhotos_TerminalTtoPending
                ON dbo.CameraPhotos (PlateReadStatus, Id)
                WHERE TerminalSent = 0 AND TerminalTtoRegistered = 0
        """
    ];

    public static async Task EnsureRmtoSyncSchemaAsync(
        string connectionString,
        ILogger? logger,
        CancellationToken ct = default)
    {
        if (_applied)
            return;

        await Gate.WaitAsync(ct);
        try
        {
            if (_applied)
                return;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            foreach (var sql in Statements)
            {
                await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 60 };
                await cmd.ExecuteNonQueryAsync(ct);
            }

            _applied = true;
            logger?.LogInformation("RmtoSync database schema OK");
        }
        finally
        {
            Gate.Release();
        }
    }
}
