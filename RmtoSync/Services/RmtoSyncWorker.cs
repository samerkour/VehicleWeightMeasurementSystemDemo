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
            "RmtoSync worker started. Poll={PollMs}ms Batch={Batch} MaxPerCycle={Max} MaxAttempts={MaxAttempts} Timeout={Timeout}s Concurrency={Concurrency}",
            _options.EffectivePollMs,
            _options.EffectiveBatchSize,
            _options.EffectiveMaxRecords,
            _options.EffectiveMaxSendAttempts,
            _options.SendTimeout.TotalSeconds,
            _options.EffectiveMaxConcurrentSends);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.EffectivePollMs));
        var consecutiveFailures = 0;

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
            {
                _logger.LogWarning(
                    "Previous sync cycle is still running — skipping this tick (Poll={PollMs}ms). A slow or hanging record is holding the cycle.",
                    _options.EffectivePollMs);
                continue;
            }

            try
            {
                await _sendService.RunCycleAsync(stoppingToken);
                consecutiveFailures = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sync loop cancelled — host is shutting down.");
                break;
            }
            catch (Exception ex)
            {
                // هیچ خطایی (حتی خطای غیرمنتظره) نباید worker را متوقف کند؛ چرخه‌ی بعدی ادامه می‌یابد.
                var explained = RahdariResponseInterpreter.ExplainException(ex, "sync cycle");
                _logger.LogError(
                    ex,
                    "Sync cycle error (cycle #{Cycle}) — {Summary}. Continuing with the next cycle.",
                    ++consecutiveFailures,
                    explained.Summary);
                _syncStatus.RecordFailure("sync cycle", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _syncRunning, 0);
            }
        }

        _logger.LogInformation("RmtoSync worker stopped.");
    }
}
