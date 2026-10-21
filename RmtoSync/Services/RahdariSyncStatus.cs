using RmtoSync.Its;

namespace RmtoSync.Services;

/// <summary>Latest Rahdari sync outcomes for UI and diagnostics.</summary>
public sealed class RahdariSyncStatus
{
    private readonly object _gate = new();
    private readonly List<RahdariSyncEvent> _recent = new();
    private const int MaxRecent = 50;

    private long _sentCount;
    private long _failedCount;
    private int _consecutiveFailures;
    private long _cycleCount;
    private int _lastCycleProcessed;
    private DateTime? _lastSuccessAt;
    private DateTime? _lastFailureAt;
    private DateTime? _lastCycleAt;
    private TimeSpan _lastCycleDuration;

    public DateTime StartedAt { get; } = DateTime.Now;

    public RahdariSyncEvent? LastEvent
    {
        get
        {
            lock (_gate)
                return _recent.Count > 0 ? _recent[0] : null;
        }
    }

    /// <summary>Records confirmed as sent to Rahdari during this session.</summary>
    public long SentCount
    {
        get
        {
            lock (_gate)
                return _sentCount;
        }
    }

    /// <summary>Failed send attempts during this session (one record may fail several times).</summary>
    public long FailedCount
    {
        get
        {
            lock (_gate)
                return _failedCount;
        }
    }

    public int ConsecutiveFailures
    {
        get
        {
            lock (_gate)
                return _consecutiveFailures;
        }
    }

    public DateTime? LastSuccessAt
    {
        get
        {
            lock (_gate)
                return _lastSuccessAt;
        }
    }

    public DateTime? LastFailureAt
    {
        get
        {
            lock (_gate)
                return _lastFailureAt;
        }
    }

    public DateTime? LastCycleAt
    {
        get
        {
            lock (_gate)
                return _lastCycleAt;
        }
    }

    public TimeSpan LastCycleDuration
    {
        get
        {
            lock (_gate)
                return _lastCycleDuration;
        }
    }

    public int LastCycleProcessed
    {
        get
        {
            lock (_gate)
                return _lastCycleProcessed;
        }
    }

    public long CycleCount
    {
        get
        {
            lock (_gate)
                return _cycleCount;
        }
    }

    public IReadOnlyList<RahdariSyncEvent> RecentEvents
    {
        get
        {
            lock (_gate)
                return _recent.ToList();
        }
    }

    public event Action? Changed;

    public void RecordSent()
    {
        lock (_gate)
        {
            _sentCount++;
            _lastSuccessAt = DateTime.Now;
            _consecutiveFailures = 0;
        }

        Changed?.Invoke();
    }

    public void RecordSuccess(string operation, string summary, long? photoId = null, long? validationCode = null)
    {
        var title = validationCode switch
        {
            ItsErrorCodes.AddTto.OkAwaitingImage => "پذیرش موقت — ارسال عکس (کد 99)",
            ItsErrorCodes.AddTto.Duplicate => "تکراری — قبلاً ثبت شده (کد 101)",
            ItsErrorCodes.AddTto.OkDamagedPlate => "پذیرفته شد — پلاک مخدوش (کد 1)",
            ItsErrorCodes.AddTto.OkTransitPlate => "پذیرفته شد — پلاک ترانزیت (کد 2)",
            ItsErrorCodes.AddTto.OkNationalPlate => "پذیرفته شد — پلاک ملی (کد 0)",
            _ => "پذیرفته شد"
        };

        lock (_gate)
        {
            _lastSuccessAt = DateTime.Now;
            _consecutiveFailures = 0;
        }

        Record(new RahdariSyncEvent(
            Timestamp: DateTime.Now,
            IsSuccess: true,
            Explanation: new RahdariResultExplanation(
                Title: title,
                Summary: summary,
                ErrorCode: validationCode,
                PhotoId: photoId,
                Operation: operation,
                Source: "ITS فصل 3-3 — InquiryInfo / addTTOInfo",
                IsSuccess: true)));
    }

    public void RecordFailure(string operation, Exception ex, long? photoId = null)
    {
        RecordFailure(operation, RahdariResponseInterpreter.ExplainException(ex, operation) with { PhotoId = photoId });
    }

    public void RecordFailure(string operation, RahdariResultExplanation explanation)
    {
        lock (_gate)
        {
            _failedCount++;
            _consecutiveFailures++;
            _lastFailureAt = DateTime.Now;
        }

        Record(new RahdariSyncEvent(
            Timestamp: DateTime.Now,
            IsSuccess: false,
            Explanation: explanation with { Operation = operation }));
    }

    /// <summary>Called by the worker after every poll so the UI can show liveness and throughput.</summary>
    public void RecordCycleCompleted(int processed, TimeSpan duration)
    {
        lock (_gate)
        {
            _lastCycleAt = DateTime.Now;
            _lastCycleDuration = duration;
            _lastCycleProcessed = processed;
            _cycleCount++;
        }
    }

    private void Record(RahdariSyncEvent entry)
    {
        lock (_gate)
        {
            _recent.Insert(0, entry);
            if (_recent.Count > MaxRecent)
                _recent.RemoveAt(_recent.Count - 1);
        }

        Changed?.Invoke();
    }
}

public sealed record RahdariSyncEvent(
    DateTime Timestamp,
    bool IsSuccess,
    RahdariResultExplanation Explanation);
