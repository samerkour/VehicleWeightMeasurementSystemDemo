-- RmtoSync | proactive close of overdue image windows (no HTTP retry)
UPDATE dbo.CameraPhotos
SET TerminalImageExpired = 1,
    TerminalImageExpiredAt = SYSDATETIME(),
    TerminalLastError = @ErrorMessage,
    TerminalSent = 1,
    TerminalSentAt = SYSDATETIME()
WHERE TerminalSent = 0
  AND ISNULL(TerminalImageExpired, 0) = 0
  AND TerminalTtoRegistered = 1
  AND TerminalImageDeadlineAt IS NOT NULL
  AND TerminalImageDeadlineAt < SYSDATETIME();
