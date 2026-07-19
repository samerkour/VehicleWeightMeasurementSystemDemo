using RmtoSync.Configuration;
using RmtoSync.Its;
using Microsoft.Extensions.Options;

namespace RmtoSync.Services;

public sealed class RmtoSyncWorker : BackgroundService
{
    private readonly RmtoSendService _sendService;
    private readonly RahdariSyncStatus _syncStatus;
    private readonly RmtoSyncOptions _options;
    private readonly ILogger<RmtoSyncWorker> _logger;
    private int _syncRunning;

    public RmtoSyncWorker(
        RmtoSendService sendService,
        RahdariSyncStatus syncStatus,
        IOptions<RmtoSyncOptions> options,
        ILogger<RmtoSyncWorker> logger)
    {
        _sendService = sendService;
        _syncStatus = syncStatus;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "RmtoSync worker started. Poll={PollMs}ms Batch={Batch} MaxPerCycle={Max}",
            _options.EffectivePollMs,
            _options.EffectiveBatchSize,
            _options.EffectiveMaxRecords);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.EffectivePollMs));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (Interlocked.CompareExchange(ref _syncRunning, 1, 0) != 0)
                continue;

            try
            {
                await _sendService.RunCycleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var explained = RahdariResponseInterpreter.ExplainException(ex, "sync cycle");
                _logger.LogError(ex, "Sync cycle error — {Summary}", explained.Summary);
                _syncStatus.RecordFailure("sync cycle", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _syncRunning, 0);
            }
        }
    }
}
