using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace RmtoSync.Data;

public sealed class CameraPhotoQueueRepository
{
    private readonly string _connectionString;
    private readonly SqlQueryLoader _queries;

    public string ConnectionString => _connectionString;

    public CameraPhotoQueueRepository(IConfiguration configuration, SqlQueryLoader queries)
    {
        _connectionString = configuration.GetConnectionString("FarasooCamera")
            ?? throw new InvalidOperationException("Connection string 'FarasooCamera' is missing.");
        _queries = queries;
    }

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingTtoBatchAsync(int batchSize, CancellationToken ct) =>
        QueryBatchAsync("GetPendingBatch", batchSize, ct);

    public Task<IReadOnlyList<CameraPhotoRecord>> GetPendingImageBatchAsync(int batchSize, CancellationToken ct) =>
        QueryBatchAsync("GetPendingImageBatch", batchSize, ct);

    public async Task MarkTtoRegisteredAsync(
        long photoId,
        long passInfoId,
        long packId,
        DateTime passDateTime,
        CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load("MarkTtoRegistered");
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { PhotoId = photoId, PassInfoId = passInfoId, PackId = packId, PassDateTime = passDateTime },
                cancellationToken: ct));
    }

    public async Task<bool> TryMarkTerminalSentAsync(long photoId, DateTime sentAt, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load("MarkDataSent");
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { PhotoId = photoId, SentAt = sentAt }, cancellationToken: ct));
        return affected > 0;
    }

    public async Task RecordTerminalErrorAsync(long photoId, string errorMessage, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load("RecordTerminalError");
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { PhotoId = photoId, ErrorMessage = errorMessage }, cancellationToken: ct));
    }

    public async Task<int> MarkExpiredImageWindowsAsync(string errorMessage, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load("MarkExpiredImageWindow");
        return await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ErrorMessage = errorMessage }, cancellationToken: ct));
    }

    public async Task<bool> AbandonExpiredImageSendAsync(long photoId, string errorMessage, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load("AbandonExpiredImageSend");
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { PhotoId = photoId, ErrorMessage = errorMessage },
                cancellationToken: ct));
        return affected > 0;
    }

    private async Task<IReadOnlyList<CameraPhotoRecord>> QueryBatchAsync(string queryName, int batchSize, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = _queries.Load(queryName);
        var rows = await connection.QueryAsync<CameraPhotoRecord>(
            new CommandDefinition(sql, new { BatchSize = batchSize }, cancellationToken: ct));
        return rows.AsList();
    }
}
