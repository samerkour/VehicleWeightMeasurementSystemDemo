/*
===============================================================================
 Recalculate Vehicles.CrimeCodes (and the class limits that feed them)
===============================================================================

 WHAT THIS SCRIPT DOES
   Rebuilds, for existing rows:
     - VehicleClass                 (from AxleCount + calibrated TotalWeight)
     - MaxAllowedWeightForClass     (per-class limit, same as appsettings)
     - MaxAllowedSpeedForClass      (light/heavy class)
     - TotalOverWeight              (TotalWeight - MaxAllowedWeightForClass)
     - CrimeCodes                   ('2056', '2020', '2056,2020', or NULL)
     - Allowed                      (PlateReadStatus = 1 AND no violation)

   It uses the weight ALREADY STORED in Vehicles.TotalWeight, i.e. the
   calibrated weight, exactly like the fixed application.

 WHAT IT DOES NOT DO
   - It never deletes rows.
   - It does not touch SpeedType / plate fields.
   - It does not run an update unless @DryRun = 0.
   - The default is @DryRun = 1 (report only).

 BEFORE RUNNING WITH @DryRun = 0
   1. Stop the weighing application (no new inserts while the script runs).
   2. Back up the database.
   3. Run once with @DryRun = 1 and review the counts and the sample rows.
   4. Decide whether historical VehicleClass should also be corrected:
        @RecalculateClass = 1  full repair (class + limits + codes)
        @RecalculateClass = 0  keep the existing class, but fix its stored
                               limits, overweight, codes and Allowed
   5. Keep the thresholds and crime codes below in sync with appsettings.json:
      VehicleClassification:SedanMaxWeight              = 1500
      VehicleClassification:PickupMaxWeight              = 3500
      VehicleClassification:LightTruckMaxWeight          = 6000
      VehicleClassification:Truck3MaxWeight              = 18000
      VehicleClassification:Truck4MaxWeight              = 24000
      VehicleClassification:LightVehicleSpeedViolation...= 110
      VehicleClassification:HeavyVehicleSpeedViolation...= 100
      VehicleClassification:CrimeCodeSpeedViolation      = 2056
      VehicleClassification:CrimeCodeWeightViolation     = 2020
      VehicleClassification:MaxAllowedWeightByClass      = table below

 NOTES
   - Rows with TotalWeight = 0/NULL are placeholders without a weight
     measurement and are intentionally skipped.
   - The script assumes TotalWeight already contains the calibrated value.
     If some rows were imported before calibration was introduced, use
     @FromId / @ToId to exclude them, or correct them separately.
   - Reclassifying historical rows is optional. Review the dry-run sample;
     if you only want to inspect the impact, stop here.

 USAGE
   sqlcmd -S . -d VehicleWeightDB -E -C -i RecalculateCrimeCodes.sql
   (edit @DryRun / @FromId / @ToId at the top)
===============================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DryRun BIT = 1;              -- 1 = report only (default), 0 = apply
DECLARE @RecalculateClass BIT = 1;    -- 1 = also fix historical VehicleClass; 0 = keep existing class
DECLARE @FromId INT = 0;
DECLARE @ToId   INT = 2147483647;

/* ---- thresholds and limits: keep in sync with appsettings.json ---- */
DECLARE @SedanMaxWeight      FLOAT = 1500;
DECLARE @PickupMaxWeight     FLOAT = 3500;
DECLARE @LightTruckMaxWeight FLOAT = 6000;
DECLARE @Truck3MaxWeight     FLOAT = 18000;
DECLARE @Truck4MaxWeight     FLOAT = 24000;

DECLARE @LightSpeed INT = 110;
DECLARE @HeavySpeed INT = 100;

DECLARE @SpeedCrime  BIGINT = 2056;
DECLARE @WeightCrime BIGINT = 2020;

DECLARE @FallbackWeight FLOAT = 44000;  -- WeightViolationThresholdKg

DECLARE @ClassLimit TABLE
(
    VehicleClass INT PRIMARY KEY,
    MaxWeight    FLOAT NOT NULL
);

INSERT INTO @ClassLimit (VehicleClass, MaxWeight) VALUES
    (1, 2200), (2, 3500), (3, 6000), (4, 10000), (5, 15000),
    (6, 23000), (7, 18000), (8, 23000), (9, 24000), (10, 27000),
    (11, 30000), (12, 32000), (13, 38000), (14, 40000);

IF OBJECT_ID('tempdb..#Evaluation') IS NOT NULL
    DROP TABLE #Evaluation;

;WITH Source AS
(
    SELECT
        v.Id,
        v.VehicleClass              AS OldClass,
        v.MaxAllowedWeightForClass  AS OldMaxWeight,
        v.MaxAllowedSpeedForClass   AS OldMaxSpeed,
        v.TotalOverWeight           AS OldOverWeight,
        v.CrimeCodes                AS OldCrimeCodes,
        v.Allowed                   AS OldAllowed,
        v.PlateReadStatus,
        v.TotalWeight,
        v.Speed,
        COALESCE(v.AxleCount, ax.AxleCount, 0) AS AxleCount
    FROM dbo.Vehicles v WITH (NOLOCK)
    LEFT JOIN
    (
        SELECT a.VehicleId, COUNT(*) AS AxleCount
        FROM dbo.Axles a WITH (NOLOCK)
        GROUP BY a.VehicleId
    ) ax ON ax.VehicleId = v.Id
    WHERE v.Id BETWEEN @FromId AND @ToId
      AND v.TotalWeight IS NOT NULL
      AND v.TotalWeight > 0
),
Classified AS
(
    SELECT
        s.*,
        CASE
            WHEN @RecalculateClass = 0 AND s.OldClass BETWEEN 1 AND 14
                THEN s.OldClass
            WHEN s.AxleCount = 1 THEN
                CASE WHEN s.TotalWeight <= @PickupMaxWeight THEN 1 ELSE 2 END
            WHEN s.AxleCount = 2 THEN
                CASE
                    WHEN s.TotalWeight <= @SedanMaxWeight      THEN 1
                    WHEN s.TotalWeight <= @PickupMaxWeight     THEN 2
                    WHEN s.TotalWeight <= @LightTruckMaxWeight THEN 3
                    ELSE 4
                END
            WHEN s.AxleCount = 3 THEN
                CASE WHEN s.TotalWeight <= @Truck3MaxWeight THEN 7 ELSE 8 END
            WHEN s.AxleCount = 4 THEN
                CASE WHEN s.TotalWeight <= @Truck4MaxWeight THEN 9 ELSE 10 END
            WHEN s.AxleCount = 5 THEN 12
            ELSE 14
        END AS NewClass
    FROM Source s
),
Limited AS
(
    SELECT
        c.*,
        COALESCE(cl.MaxWeight, @FallbackWeight) AS NewMaxWeight,
        CASE WHEN c.NewClass <= 3 THEN @LightSpeed ELSE @HeavySpeed END AS NewMaxSpeed
    FROM Classified c
    LEFT JOIN @ClassLimit cl ON cl.VehicleClass = c.NewClass
),
Evaluated AS
(
    SELECT
        l.*,
        CASE WHEN l.TotalWeight > l.NewMaxWeight
             THEN l.TotalWeight - l.NewMaxWeight
             ELSE 0
        END AS NewOverWeight,
        CASE WHEN ISNULL(l.Speed, 0) > l.NewMaxSpeed THEN 1 ELSE 0 END AS NewSpeedViolation,
        CASE WHEN l.TotalWeight > l.NewMaxWeight THEN 1 ELSE 0 END       AS NewWeightViolation
    FROM Limited l
)
SELECT
    e.Id,
    e.OldClass,
    e.NewClass,
    e.OldMaxWeight,
    e.NewMaxWeight,
    e.OldMaxSpeed,
    e.NewMaxSpeed,
    e.OldOverWeight,
    e.NewOverWeight,
    e.OldCrimeCodes,
    e.OldAllowed,
    e.PlateReadStatus,
    e.TotalWeight,
    e.Speed,
    CASE
        WHEN e.NewSpeedViolation = 1 AND e.NewWeightViolation = 1
            THEN CONCAT(@SpeedCrime, ',', @WeightCrime)
        WHEN e.NewSpeedViolation = 1
            THEN CONVERT(VARCHAR(20), @SpeedCrime)
        WHEN e.NewWeightViolation = 1
            THEN CONVERT(VARCHAR(20), @WeightCrime)
        ELSE NULL
    END AS NewCrimeCodes,
    CAST(
        CASE
            WHEN ISNULL(e.PlateReadStatus, 0) = 1
             AND e.NewSpeedViolation = 0
             AND e.NewWeightViolation = 0
                THEN 1
            ELSE 0
        END AS BIT
    ) AS NewAllowed,
    CASE
        WHEN ISNULL(e.OldClass, -1)      <> e.NewClass
          OR ISNULL(e.OldMaxWeight, -1)  <> e.NewMaxWeight
          OR ISNULL(e.OldMaxSpeed, -1)   <> e.NewMaxSpeed
          OR ISNULL(e.OldOverWeight, -1) <> e.NewOverWeight
          OR ISNULL(e.OldCrimeCodes, '') <> ISNULL(
                CASE
                    WHEN e.NewSpeedViolation = 1 AND e.NewWeightViolation = 1
                        THEN CONCAT(@SpeedCrime, ',', @WeightCrime)
                    WHEN e.NewSpeedViolation = 1
                        THEN CONVERT(VARCHAR(20), @SpeedCrime)
                    WHEN e.NewWeightViolation = 1
                        THEN CONVERT(VARCHAR(20), @WeightCrime)
                    ELSE NULL
                END, '')
          OR ISNULL(CAST(e.OldAllowed AS INT), -1) <> CAST(
                CASE
                    WHEN ISNULL(e.PlateReadStatus, 0) = 1
                     AND e.NewSpeedViolation = 0
                     AND e.NewWeightViolation = 0
                        THEN 1
                    ELSE 0
                END AS INT)
            THEN 1
        ELSE 0
    END AS Mismatch
INTO #Evaluation
FROM Evaluated e;

/* ---- dry-run report ------------------------------------------------------ */
SELECT
    COUNT(*)                      AS RowsEvaluated,
    SUM(Mismatch)                 AS RowsToChange,
    SUM(CASE WHEN ISNULL(OldClass, -1) <> NewClass THEN 1 ELSE 0 END)
                                  AS ClassChanges,
    SUM(CASE WHEN ISNULL(OldMaxWeight, -1) <> NewMaxWeight THEN 1 ELSE 0 END)
                                  AS MaxWeightChanges,
    SUM(CASE WHEN ISNULL(OldMaxSpeed, -1) <> NewMaxSpeed THEN 1 ELSE 0 END)
                                  AS MaxSpeedChanges,
    SUM(CASE WHEN ISNULL(OldOverWeight, -1) <> NewOverWeight THEN 1 ELSE 0 END)
                                  AS OverWeightChanges,
    SUM(CASE WHEN ISNULL(OldCrimeCodes, '') <> ISNULL(NewCrimeCodes, '') THEN 1 ELSE 0 END)
                                  AS CrimeCodeChanges,
    SUM(CASE WHEN ISNULL(CAST(OldAllowed AS INT), -1)
                  <> CAST(NewAllowed AS INT) THEN 1 ELSE 0 END)
                                  AS AllowedChanges
FROM #Evaluation;

SELECT TOP (50)
    Id,
    OldClass, NewClass,
    OldMaxWeight, NewMaxWeight,
    OldMaxSpeed, NewMaxSpeed,
    OldOverWeight, NewOverWeight,
    OldCrimeCodes, NewCrimeCodes,
    OldAllowed, NewAllowed,
    TotalWeight, Speed
FROM #Evaluation
WHERE Mismatch = 1
ORDER BY Id DESC;

/* ---- apply --------------------------------------------------------------- */
IF @DryRun = 0
BEGIN
    BEGIN TRANSACTION;

    UPDATE v
    SET
        VehicleClass             = e.NewClass,
        MaxAllowedWeightForClass = e.NewMaxWeight,
        MaxAllowedSpeedForClass  = e.NewMaxSpeed,
        TotalOverWeight          = e.NewOverWeight,
        CrimeCodes               = e.NewCrimeCodes,
        Allowed                  = e.NewAllowed
    FROM dbo.Vehicles v
    INNER JOIN #Evaluation e ON e.Id = v.Id
    WHERE e.Mismatch = 1;

    SELECT @@ROWCOUNT AS UpdatedRows;

    COMMIT TRANSACTION;

    /* Post-update sanity check. All counters should be 0. */
    SELECT
        SUM(CASE WHEN TotalWeight > MaxAllowedWeightForClass
                       AND ISNULL(CrimeCodes, '') NOT LIKE '%2020%' THEN 1 ELSE 0 END)
            AS RemainingMissingWeightCode,
        SUM(CASE WHEN TotalWeight <= MaxAllowedWeightForClass
                       AND ISNULL(CrimeCodes, '') LIKE '%2020%' THEN 1 ELSE 0 END)
            AS RemainingUnexpectedWeightCode,
        SUM(CASE WHEN ISNULL(PlateReadStatus, 0) = 1
                       AND IsNull(CrimeCodes, '') = ''
                       AND ISNULL(CAST(Allowed AS INT), 0) = 0 THEN 1 ELSE 0 END)
            AS RemainingWronglyDisallowed,
        SUM(CASE WHEN ISNULL(CrimeCodes, '') <> ''
                       AND ISNULL(CAST(Allowed AS INT), 0) = 1 THEN 1 ELSE 0 END)
            AS RemainingAllowedWithCrime
    FROM dbo.Vehicles WITH (NOLOCK)
    WHERE Id BETWEEN @FromId AND @ToId
      AND TotalWeight IS NOT NULL
      AND TotalWeight > 0;
END
ELSE
BEGIN
    PRINT 'DRY RUN: no rows were changed. Set @DryRun = 0 after reviewing the report.';
END

DROP TABLE #Evaluation;
