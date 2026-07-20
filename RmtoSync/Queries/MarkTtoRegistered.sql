-- RmtoSync | TTO metadata accepted; store PassInfoId from InquiryInfo (ITS chapter 1-4)
UPDATE dbo.CameraPhotos
SET TerminalTtoRegistered = 1,
    TerminalTtoRegisteredAt = SYSUTCDATETIME(),
    TerminalImageDeadlineAt = DATEADD(HOUR, 24, @PassDateTime),
    TerminalPassInfoId = @PassInfoId,
    TerminalPackId = @PackId,
    PassInfoId = COALESCE(@PassInfoId, PassInfoId),
    TerminalLastError = NULL
WHERE Id = @PhotoId
  AND TerminalSent = 0;
