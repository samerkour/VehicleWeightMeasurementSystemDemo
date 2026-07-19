-- RmtoSync | FarasooCamera | mark sent atomically (WHERE TerminalSent = 0)
UPDATE dbo.CameraPhotos
SET TerminalSent = 1,
    TerminalSentAt = @SentAt,
    TerminalLastError = NULL
WHERE PhotoId = @PhotoId
  AND TerminalSent = 0;
