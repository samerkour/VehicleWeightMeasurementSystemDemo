/* ============================================================================================
   RecoverSpeedValidationAbandoned.sql
   --------------------------------------------------------------------------------------------
   هدف: بازگرداندن کنترل‌شده‌ی رکوردهایی که به‌دلیل «سرعت صفر» رها شده‌اند.
   سرور ITS مقدار صفر را برای سرعت، «ثبت‌نشده» می‌داند و با speedType = 1/2/3 نامعتبر
   اعلام می‌کند (خطاهای ۱۱۶ و ۱۲۴). کد قبلی سرعت صفرِ دیتابیس را همان‌طور می‌فرستاد
   (چون 0 با NULL فرق دارد) و رکورد پس از ۳ تلاش برای همیشه رها می‌شد؛ درحالی‌که هیچ
   درخواست موفقی برای آن ثبت نشده بود.

   اکنون سازنده‌ی payload هر سرعت صفر/NULL را با Rahdari:DefaultSpeed (پیش‌فرض ۶۰)
   جایگزین می‌کند. این اسکریپت همان رکوردهای رهاشده را — که با کد اصلاح‌شده ارسال
   می‌شوند — به صف برمی‌گرداند.

   ⚠ نکات مهم
   - هیچ لاگ یا رکوردی حذف نمی‌شود؛ فقط پرچم‌های صف بازنشانی می‌شوند.
   - به‌طور پیش‌فرض DRY-RUN است: فقط تعداد و تفکیک دلیل را نشان می‌دهد.
     برای اجرا @DryRun = 0 کنید.
   - فقط رکوردهای زیر ۳۰ روز (حداکثر عمر داده در ITS) و با پلاک قابل استفاده برمی‌گردند.
   - رکوردهایی که مرحله‌ی TTO آن‌ها قبلاً موفق بوده (TerminalTtoRegistered = 1) فقط وقتی
     برمی‌گردند که پنجره‌ی مجاز AddImage هنوز باز باشد؛ در غیر این صورت تصویر قابل
     افزودن نیست و رکورد دست‌نخورده می‌ماند.
   - رکوردهایی که به‌دلیل رد قطعی سرور (۱۰۸/۱۰۹/…)، نبود تصویر یا انقضای پنجره رها شده‌اند
     دست‌نخورده می‌مانند.
   - اسکریپت idempotent است: اجرای دوباره رکورد جدیدی برنمی‌گرداند (چون پرچم رهابرداری
     صفر می‌شود و شرط TerminalAbandoned = 1 دیگر برقرار نیست).
   ============================================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* گزینه‌های لازم برای به‌روزرسانی جداولی که ایندکس/ویو ایندکس‌شده دارند */
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

DECLARE @DryRun     BIT = 1;    -- 1 = فقط گزارش، 0 = اعمال
DECLARE @MaxAgeDays INT = 30;   -- حداکثر عمر مجاز داده در ITS (کد ۱۱۳)

/* ---------------------------------------------------------------------------------------------
   1) دلایلِ قابل بازگردانی — فقط خطاهای محلیِ اعتبارسنجی سرعت.
      الگوها باریک‌اند تا رد قطعی سرور بازگردانی نشود.
   --------------------------------------------------------------------------------------------- */
DECLARE @RecoverableReasons TABLE (Pattern NVARCHAR(200) NOT NULL);
INSERT INTO @RecoverableReasons (Pattern) VALUES
    (N'%Instant speed requires VehicleSpeed%'),          -- ITS 116
    (N'%SpeedType=0 requires%'),                         -- ITS 116
    (N'%average speed (ITS 124)%'),                      -- ITS 124
    (N'%Instant+average speed requires%'),               -- ITS 124
    (N'%ITS 116%'),
    (N'%ITS 124%');

/* ---------------------------------------------------------------------------------------------
   2) انتخاب کاندیدها — دو گروه مستقل
   --------------------------------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#TtoCandidates') IS NOT NULL DROP TABLE #TtoCandidates;
IF OBJECT_ID('tempdb..#ImageCandidates') IS NOT NULL DROP TABLE #ImageCandidates;

/* گروه الف: مرحله‌ی TTO هرگز موفق نشده ⇒ کل رکورد از ابتدا ارسال می‌شود. */
SELECT cp.Id AS PhotoId
INTO #TtoCandidates
FROM dbo.CameraPhotos cp
WHERE cp.TerminalSent = 1
  AND cp.TerminalAbandoned = 1
  AND ISNULL(cp.TerminalTtoRegistered, 0) = 0
  AND ISNULL(cp.CapturedAt, cp.ImportedAt) >= DATEADD(DAY, -@MaxAgeDays, GETDATE())
  AND EXISTS (
        SELECT 1
        FROM @RecoverableReasons r
        WHERE cp.TerminalLastError LIKE r.Pattern)
  /* پلاک قابل استفاده — همان شرطی که صف ارسال برای مرحله‌ی TTO اعمال می‌کند */
  AND (
        (LTRIM(RTRIM(ISNULL(cp.PlateP1, N''))) <> N''
         AND LTRIM(RTRIM(ISNULL(cp.PlateP2, N''))) <> N''
         AND LTRIM(RTRIM(ISNULL(cp.PlateP3, N''))) <> N''
         AND LTRIM(RTRIM(ISNULL(cp.PlateP4, N''))) <> N'')
        OR
        (LTRIM(RTRIM(ISNULL(cp.PlateP2, N''))) <> N''
         AND (LTRIM(RTRIM(ISNULL(cp.PlateP1, N''))) <> N''
              OR LTRIM(RTRIM(ISNULL(cp.PlateP3, N''))) <> N''
              OR LTRIM(RTRIM(ISNULL(cp.PlateP4, N''))) <> N''))
      );

/* گروه ب: TTO ثبت شده و فقط مرحله‌ی تصویر به‌دلیل سرعت شکست خورده ⇒
   تنها اگر پنجره‌ی AddImage هنوز باز باشد، فقط مرحله‌ی تصویر دوباره ارسال می‌شود. */
SELECT cp.Id AS PhotoId
INTO #ImageCandidates
FROM dbo.CameraPhotos cp
WHERE cp.TerminalSent = 1
  AND cp.TerminalAbandoned = 1
  AND cp.TerminalTtoRegistered = 1
  AND ISNULL(cp.TerminalImageExpired, 0) = 0
  AND (cp.TerminalImageDeadlineAt IS NULL OR cp.TerminalImageDeadlineAt > GETDATE())
  AND ISNULL(cp.CapturedAt, cp.ImportedAt) >= DATEADD(DAY, -@MaxAgeDays, GETDATE())
  AND EXISTS (
        SELECT 1
        FROM @RecoverableReasons r
        WHERE cp.TerminalLastError LIKE r.Pattern);

CREATE CLUSTERED INDEX IX_TtoCandidates ON #TtoCandidates (PhotoId);
CREATE CLUSTERED INDEX IX_ImageCandidates ON #ImageCandidates (PhotoId);

DECLARE @TtoCandidates   INT = (SELECT COUNT(*) FROM #TtoCandidates);
DECLARE @ImageCandidates INT = (SELECT COUNT(*) FROM #ImageCandidates);

PRINT '------------------------------------------------------------';
PRINT 'TTO-stage recoverable records (dry-run):   ' + CAST(@TtoCandidates AS VARCHAR(20));
PRINT 'Image-stage recoverable records (dry-run): ' + CAST(@ImageCandidates AS VARCHAR(20));
PRINT '------------------------------------------------------------';

IF @TtoCandidates + @ImageCandidates > 0
BEGIN
    PRINT 'Breakdown by original reason:';
    SELECT TOP (20)
           LEFT(cp.TerminalLastError, 110) AS Reason,
           SUM(CASE WHEN ISNULL(cp.TerminalTtoRegistered, 0) = 0 THEN 1 ELSE 0 END) AS TtoStage,
           SUM(CASE WHEN cp.TerminalTtoRegistered = 1 THEN 1 ELSE 0 END)             AS ImageStage
    FROM dbo.CameraPhotos cp
    WHERE cp.Id IN (SELECT PhotoId FROM #TtoCandidates
                    UNION ALL
                    SELECT PhotoId FROM #ImageCandidates)
    GROUP BY LEFT(cp.TerminalLastError, 110)
    ORDER BY 2 DESC, 3 DESC;
END

IF @DryRun = 1
BEGIN
    PRINT 'DRY-RUN: nothing was changed. Set @DryRun = 0 and re-run to re-queue these records.';
    DROP TABLE #TtoCandidates;
    DROP TABLE #ImageCandidates;
    RETURN;
END

/* ---------------------------------------------------------------------------------------------
   3) بازنشانی صف — فقط پرچم‌های صف، بدون تغییر داده‌ی اصلی یا لاگ‌ها
   --------------------------------------------------------------------------------------------- */
BEGIN TRANSACTION;

/* گروه الف: رکورد کامل (TTO + تصویر) از نو ارسال می‌شود. */
UPDATE cp
SET cp.TerminalSent            = 0,
    cp.TerminalSentAt          = NULL,
    cp.TerminalAbandoned       = 0,
    cp.TerminalSendAttempts    = 0,
    cp.TerminalLastAttemptAt   = NULL,
    cp.TerminalImageExpired    = 0,
    cp.TerminalImageExpiredAt  = NULL,
    cp.TerminalImageDeadlineAt = NULL,
    cp.TerminalLastError       = N''
FROM dbo.CameraPhotos cp
JOIN #TtoCandidates c ON c.PhotoId = cp.Id;

DECLARE @TtoUpdated INT = @@ROWCOUNT;

/* گروه ب: فقط مرحله‌ی تصویر؛ ثبت TTO و مهلت تصویر حفظ می‌شود. */
UPDATE cp
SET cp.TerminalSent           = 0,
    cp.TerminalSentAt         = NULL,
    cp.TerminalAbandoned      = 0,
    cp.TerminalSendAttempts   = 0,
    cp.TerminalLastAttemptAt  = NULL,
    cp.TerminalImageExpired   = 0,
    cp.TerminalImageExpiredAt = NULL,
    cp.TerminalLastError      = N''
FROM dbo.CameraPhotos cp
JOIN #ImageCandidates c ON c.PhotoId = cp.Id;

DECLARE @ImageUpdated INT = @@ROWCOUNT;

COMMIT TRANSACTION;

PRINT '------------------------------------------------------------';
PRINT 'Records re-queued (TTO stage):   ' + CAST(@TtoUpdated AS VARCHAR(20));
PRINT 'Records re-queued (image stage): ' + CAST(@ImageUpdated AS VARCHAR(20));
PRINT '------------------------------------------------------------';

DROP TABLE #TtoCandidates;
DROP TABLE #ImageCandidates;

/* ---------------------------------------------------------------------------------------------
   4) بررسی پس از اجرا
   --------------------------------------------------------------------------------------------- */
SELECT
    SUM(CASE WHEN TerminalSent = 0 AND TerminalAbandoned = 0 AND TerminalTtoRegistered = 0 THEN 1 ELSE 0 END)
        AS AwaitingTto,
    SUM(CASE WHEN TerminalSent = 0 AND TerminalAbandoned = 0 AND TerminalTtoRegistered = 1 THEN 1 ELSE 0 END)
        AS AwaitingImage,
    SUM(CASE WHEN TerminalSent = 1 AND TerminalAbandoned = 1 THEN 1 ELSE 0 END)
        AS StillAbandoned
FROM dbo.CameraPhotos;
