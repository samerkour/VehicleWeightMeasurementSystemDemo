-- =====================================================================================
--  AlignCameraPhotoSchema.sql
--  RmtoSync — camera/vehicle schema alignment (idempotent, production-safe)
-- =====================================================================================
--  PURPOSE
--    RmtoSync reads its send queue from the VIEW dbo.vw_CameraFullData (EF maps
--    CameraPhotoRecord to it via ToView) and writes retry state to the TABLE
--    dbo.CameraPhotos. When a database was not carried forward through the
--    VehicleWeightMeasurementSystemDemo migration chain, EF emits LINQ that
--    references columns the database does not have and SQL Server raises:
--
--        Invalid column name 'TerminalSent'.
--        Invalid column name 'TotalWeight'.
--        Invalid column name 'VehicleClass'.
--        Invalid column name 'AxleWeight1'.
--        ...
--
--  IMPORTANT — two different schema objects are involved:
--    * dbo.CameraPhotos  (table)  → the Terminal* retry/ack columns
--    * dbo.vw_CameraFullData (view) → TotalWeight, VehicleClass, AverageSpeed,
--                                       VehicleSpeed, PlateReadStatus, PlateConfidence,
--                                       Allowed, AxleWeight1..9, TotalAxles,
--                                       TotalOverWeight, Latitude, Longitude, ...
--    Adding "TotalWeight" to the CameraPhotos TABLE does NOT fix
--    "Invalid column name 'TotalWeight'": EF selects that name from the VIEW.
--    The view has to expose it. That is what Step 3 does.
--
--  SAFETY
--    * Every step is guarded by COL_LENGTH / OBJECT_ID → safe to re-run.
--    * Only ADDITIVE changes: no column is dropped, no data is modified.
--    * Newly added columns are nullable or carry a DEFAULT → existing rows survive.
--    * Requires SQL Server 2016 SP1+ (uses CREATE OR ALTER VIEW).
--    * Run against the SAME database RmtoSync points at
--      (ConnectionStrings:DefaultConnection).
-- =====================================================================================

SET NOCOUNT ON;
GO

-- -------------------------------------------------------------------------------------
-- Step 1 — dbo.CameraPhotos : columns the model and the view require
--            (all guarded, all nullable or defaulted → no data loss)
-- -------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.CameraPhotos', N'U') IS NULL
    THROW 51000, 'dbo.CameraPhotos does not exist in this database. RmtoSync cannot operate against it.', 1;
GO

-- Identity / join key used by the view
IF COL_LENGTH(N'dbo.CameraPhotos', N'VehicleId') IS NULL
    ALTER TABLE dbo.CameraPhotos ADD VehicleId int NULL;
GO

-- Plate
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP1') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP1 nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP2') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP2 nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP3') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP3 nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP4') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP4 nvarchar(10) NULL;
GO
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxLeft')   IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxLeft   int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxTop')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxTop    int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxWidth')  IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxWidth  int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxHeight') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxHeight int NULL;
GO

-- Image paths (added by 20260824081835_AddPlateImageFileFields)
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateFileName')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateFileName    nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateFullPath')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateFullPath    nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateRelativePath') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateRelativePath nvarchar(max) NULL;
GO

-- Axle-length measurements
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles12')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles12           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles23')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles23           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles34')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles34           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles45')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles45           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles56')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles56           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles67')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles67           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles78')           IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles78           int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxlesMoreThan8')    IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxlesMoreThan8    int NULL;
GO

-- Per-axle weight buckets
IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightA') IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightA int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightB') IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightB int NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightC') IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightC int NULL;
GO

-- Terminal acknowledgement / retry state (RmtoSync queue columns)
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSent')          IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalSent          bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalSent          DEFAULT (0);
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSentAt')        IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalSentAt        datetime2 NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalLastError')     IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalLastError     nvarchar(4000) NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalTtoRegistered') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalTtoRegistered bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalTtoRegistered DEFAULT (0);
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalTtoRegisteredAt') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalTtoRegisteredAt datetime2 NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageDeadlineAt') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageDeadlineAt datetime2 NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageExpired')  IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpired  bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalImageExpired  DEFAULT (0);
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageExpiredAt') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpiredAt datetime2 NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalPassInfoId')    IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalPassInfoId    bigint NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalPackId')        IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalPackId        bigint NULL;
IF COL_LENGTH(N'dbo.CameraPhotos', N'PassInfoId')            IS NULL ALTER TABLE dbo.CameraPhotos ADD PassInfoId            bigint NULL;
GO

-- PreviousDeviceCode is load-bearing: TtoPreSendValidator rejects speed-type payloads
-- when it is <= 0, so it must stay selectable even where it is no longer populated.
IF COL_LENGTH(N'dbo.CameraPhotos', N'PreviousDeviceCode') IS NULL ALTER TABLE dbo.CameraPhotos ADD PreviousDeviceCode bigint NULL;
GO

-- 🔥 Retry accounting — the columns that make a stuck image stop blocking the queue.
--    Without them a permanently failing record is re-selected every poll cycle forever.
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSendAttempts') IS NULL
    ALTER TABLE dbo.CameraPhotos ADD TerminalSendAttempts int NOT NULL CONSTRAINT DF_CameraPhotos_TerminalSendAttempts DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalLastAttemptAt') IS NULL
    ALTER TABLE dbo.CameraPhotos ADD TerminalLastAttemptAt datetime2 NULL;
GO
IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalAbandoned') IS NULL
    ALTER TABLE dbo.CameraPhotos ADD TerminalAbandoned bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalAbandoned DEFAULT (0);
GO

-- -------------------------------------------------------------------------------------
-- Step 2 — dbo.Vehicles / dbo.Axles : columns the view projects
--            (MaxAllowed* came from a later migration and is the usual gap)
-- -------------------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.Vehicles', N'MaxAllowedSpeedForClass') IS NULL
    ALTER TABLE dbo.Vehicles ADD MaxAllowedSpeedForClass int NOT NULL CONSTRAINT DF_Vehicles_MaxAllowedSpeedForClass DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.Vehicles', N'MaxAllowedWeightForClass') IS NULL
    ALTER TABLE dbo.Vehicles ADD MaxAllowedWeightForClass float NOT NULL CONSTRAINT DF_Vehicles_MaxAllowedWeightForClass DEFAULT (0);
GO
IF COL_LENGTH(N'dbo.Axles', N'AxleIndex') IS NULL ALTER TABLE dbo.Axles ADD AxleIndex int NULL;
IF COL_LENGTH(N'dbo.Axles', N'Weight')    IS NULL ALTER TABLE dbo.Axles ADD Weight float NULL;
GO

-- -------------------------------------------------------------------------------------
-- Step 3 — 🔥 dbo.vw_CameraFullData : THE fix for the TotalWeight / VehicleClass /
--            AxleWeight1..9 / PlateReadStatus / Allowed / … class of errors.
--
--            This view is the ONLY place those names are defined for EF, because
--            CameraPhotoRecord is mapped with ToView("vw_CameraFullData"). Recreating it
--            to the authoritative 81-column definition is what makes the EF LINQ valid.
-- -------------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Vehicles', N'U')     IS NULL THROW 51001, 'dbo.Vehicles is missing — cannot create vw_CameraFullData.', 1;
IF OBJECT_ID(N'dbo.Axles', N'U')        IS NULL THROW 51002, 'dbo.Axles is missing — cannot create vw_CameraFullData.', 1;
IF OBJECT_ID(N'dbo.CameraPhotos', N'U') IS NULL THROW 51003, 'dbo.CameraPhotos is missing — cannot create vw_CameraFullData.', 1;
GO

CREATE OR ALTER VIEW dbo.vw_CameraFullData
AS
SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                 v.PlateConfidence AS PlateConfidence, v.PlateReadStatus AS PlateReadStatus, v.PlateReadAt AS PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath,
                 cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt,
                 cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed AS VehicleSpeed, v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed,
                 v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, v.SpeedType, v.VehicleClass AS CarClass13, v.CrimeCodes, v.PlateConfidence AS OcrScore, v.HeadGap, v.Gap,
                 v.MaxAllowedSpeedForClass, v.MaxAllowedWeightForClass, v.VehicleLen AS FirstToLastAxlesLen, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                 cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC,
                 cp.TerminalSendAttempts, cp.TerminalLastAttemptAt, cp.TerminalAbandoned,
                 MAX(CASE WHEN a.AxleIndex = 1 THEN a.Weight END) AS AxleWeight1, MAX(CASE WHEN a.AxleIndex = 2 THEN a.Weight END) AS AxleWeight2, MAX(CASE WHEN a.AxleIndex = 3 THEN a.Weight END) AS AxleWeight3,
                 MAX(CASE WHEN a.AxleIndex = 4 THEN a.Weight END) AS AxleWeight4, MAX(CASE WHEN a.AxleIndex = 5 THEN a.Weight END) AS AxleWeight5, MAX(CASE WHEN a.AxleIndex = 6 THEN a.Weight END) AS AxleWeight6,
                 MAX(CASE WHEN a.AxleIndex = 7 THEN a.Weight END) AS AxleWeight7, MAX(CASE WHEN a.AxleIndex = 8 THEN a.Weight END) AS AxleWeight8, MAX(CASE WHEN a.AxleIndex = 9 THEN a.Weight END) AS AxleWeight9
FROM            dbo.Vehicles AS v LEFT OUTER JOIN
                 dbo.CameraPhotos AS cp ON cp.VehicleId = v.Id LEFT OUTER JOIN
                 dbo.Axles AS a ON a.VehicleId = v.Id
GROUP BY cp.Id, v.Id, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4, v.PlateConfidence, v.PlateReadStatus,
                 v.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath, cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError,
                 cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode,
                 v.Speed, v.AverageSpeed, v.TotalWeight, v.AxleCount, v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, v.SpeedType, v.CrimeCodes,
                 v.HeadGap, v.Gap, v.MaxAllowedSpeedForClass, v.MaxAllowedWeightForClass, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                 cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC, cp.TerminalSendAttempts, cp.TerminalLastAttemptAt, cp.TerminalAbandoned
GO

-- -------------------------------------------------------------------------------------
-- Step 4 — queue index: every poll cycle filters TerminalSent / TerminalTtoRegistered /
--            TerminalAbandoned / TerminalSendAttempts and orders by Id.
-- -------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CameraPhotos_SendQueue' AND object_id = OBJECT_ID(N'dbo.CameraPhotos'))
    CREATE NONCLUSTERED INDEX IX_CameraPhotos_SendQueue
        ON dbo.CameraPhotos (TerminalSent, TerminalTtoRegistered, TerminalAbandoned, TerminalSendAttempts)
        INCLUDE (TerminalLastAttemptAt, Id);
GO

PRINT 'AlignCameraPhotoSchema completed.';
GO

-- =====================================================================================
--  VERIFICATION (run after the script; every result must be 0)
-- =====================================================================================
--  SELECT c.name
--  FROM sys.columns c
--  WHERE c.object_id = OBJECT_ID(N'dbo.vw_CameraFullData')
--    AND c.name NOT IN ( /* the 81 model column names */ )
--  ORDER BY c.name;
--
--  Simpler smoke test — the RmtoSync queue query must return rows instead of raising
--  "Invalid column name":  SELECT TOP 1 * FROM dbo.vw_CameraFullData;
-- =====================================================================================
