-- RmtoSync | FarasooCamera | persist last Rahdari send failure (row stays TerminalSent = 0)
UPDATE dbo.CameraPhotos
SET TerminalLastError = @ErrorMessage
WHERE PhotoId = @PhotoId
  AND TerminalSent = 0;
