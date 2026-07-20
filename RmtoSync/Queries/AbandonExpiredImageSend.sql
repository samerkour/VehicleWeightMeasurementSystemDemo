-- RmtoSync | ITS AddImage 1009 or local 24h deadline — close permanently, no further retries
UPDATE dbo.CameraPhotos
SET TerminalImageExpired = 1,
    TerminalImageExpiredAt = SYSDATETIME(),
    TerminalLastError = @ErrorMessage,
    TerminalSent = 1,
    TerminalSentAt = SYSDATETIME()
WHERE Id = @PhotoId
  AND TerminalSent = 0
  AND ISNULL(TerminalImageExpired, 0) = 0;
