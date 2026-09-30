namespace RmtoSync.Data;

public sealed class CameraPhotoRow
{
    public long Id { get; set; }
    public bool TerminalSent { get; set; }
    public DateTime? TerminalSentAt { get; set; }
    public string? TerminalLastError { get; set; }
    public bool? TerminalTtoRegistered { get; set; }
    public DateTime? TerminalTtoRegisteredAt { get; set; }
    public DateTime? TerminalImageDeadlineAt { get; set; }
    public bool? TerminalImageExpired { get; set; }
    public DateTime? TerminalImageExpiredAt { get; set; }
    public long? TerminalPassInfoId { get; set; }
    public long? TerminalPackId { get; set; }
    public long? PassInfoId { get; set; }
    public int TerminalSendAttempts { get; set; }
    public DateTime? TerminalLastAttemptAt { get; set; }
    public bool TerminalAbandoned { get; set; }
}
