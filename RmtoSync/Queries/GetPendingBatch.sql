-- RmtoSync | pending TTO registration (metadata not yet accepted by ITS)
SELECT TOP (@BatchSize)
    p.PhotoId,
    p.LineId,
    cl.LineCode,
    p.FullPath,
    p.FileName,
    p.CapturedAt,
    p.ImportedAt,
    p.PlateP1,
    p.PlateP2,
    p.PlateP3,
    p.PlateP4,
    p.PlateConfidence,
    p.PlateBoxLeft,
    p.PlateBoxTop,
    p.PlateBoxWidth,
    p.PlateBoxHeight,
    p.PlateReadStatus,
    ISNULL(p.TerminalTtoRegistered, 0) AS TerminalTtoRegistered,
    p.TerminalPassInfoId,
    p.TerminalPackId,
    p.VehicleSpeed,
    p.AverageSpeed,
    p.TotalWeight,
    p.TotalAxles,
    p.AxleWeight1,
    p.AxleWeight2,
    p.AxleWeight3,
    p.AxleWeight4,
    p.AxleWeight5,
    p.AxleWeight6,
    p.AxleWeight7,
    p.AxleWeight8,
    p.AxleWeight9,
    p.CarClass13,
    p.Allowed,
    p.WrongDirection,
    p.SpeedType,
    p.VehicleClass,
    p.CrimeCodes,
    p.OcrScore,
    p.Longitude,
    p.Latitude,
    p.VehicleLen,
    p.HeadGap,
    p.Gap,
    p.FirstToLastAxlesLen,
    p.LengthAxles12,
    p.LengthAxles23,
    p.LengthAxles34,
    p.LengthAxles45,
    p.LengthAxles56,
    p.LengthAxles67,
    p.LengthAxles78,
    p.LengthAxlesMoreThan8,
    p.TotalWeightA,
    p.TotalWeightB,
    p.TotalWeightC,
    p.TotalOverWeight,
    p.PassInfoId,
    p.PreviousDeviceCode,
    p.TerminalTtoRegisteredAt,
    p.TerminalImageDeadlineAt
FROM dbo.vw_CameraFullData p
INNER JOIN dbo.Lines cl ON cl.Id = p.LineId
WHERE p.TerminalSent = 0
  AND ISNULL(p.TerminalTtoRegistered, 0) = 0
  AND ISNULL(p.TerminalImageExpired, 0) = 0
  AND p.PlateReadStatus = 1
  AND (
        (
            LTRIM(RTRIM(ISNULL(p.PlateP1, N''))) <> N''
            AND LTRIM(RTRIM(ISNULL(p.PlateP2, N''))) <> N''
            AND LTRIM(RTRIM(ISNULL(p.PlateP3, N''))) <> N''
            AND LTRIM(RTRIM(ISNULL(p.PlateP4, N''))) <> N''
        )
        OR (
            LTRIM(RTRIM(ISNULL(p.PlateP2, N''))) <> N''
            AND (
                LTRIM(RTRIM(ISNULL(p.PlateP1, N''))) <> N''
                OR LTRIM(RTRIM(ISNULL(p.PlateP3, N''))) <> N''
                OR LTRIM(RTRIM(ISNULL(p.PlateP4, N''))) <> N''
            )
        )
      )
ORDER BY p.PhotoId ASC;
