using RmtoSync.Its;

namespace RmtoSync.Services;

/// <summary>Latest Rahdari sync outcomes for UI and diagnostics.</summary>
public sealed class RahdariSyncStatus
{
    private readonly object _gate = new();
    private readonly List<RahdariSyncEvent> _recent = new();
    private const int MaxRecent = 20;

    public RahdariSyncEvent? LastEvent { get; private set; }

    public long SentCount { get; private set; }

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
            SentCount++;

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
        Record(new RahdariSyncEvent(
            Timestamp: DateTime.Now,
            IsSuccess: false,
            Explanation: RahdariResponseInterpreter.ExplainException(ex, operation) with { PhotoId = photoId }));
    }

    public void RecordFailure(string operation, RahdariResultExplanation explanation)
    {
        Record(new RahdariSyncEvent(
            Timestamp: DateTime.Now,
            IsSuccess: false,
            Explanation: explanation with { Operation = operation }));
    }

    private void Record(RahdariSyncEvent entry)
    {
        lock (_gate)
        {
            LastEvent = entry;
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
