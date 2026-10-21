/* ============================================================================================
   RecoverAbandonedBatchSends.sql
   --------------------------------------------------------------------------------------------
   هدف: بازگرداندن کنترل‌شده‌ی رکوردهایی که به‌دلیل «نتیجه‌ی نامعلومِ» addTTOInfoBatch2 رها شده‌اند
   به صف ارسال. این رکوردها رد نشده‌اند؛ فقط پاسخ سرور برایشان InquiryInfo نداشته و کد قدیمی
   این حالت را «شکست قطعی» می‌شمرد و پس از ۳ تلاش برای همیشه رهایشان می‌کرد.

   ⚠ نکات مهم
   - این اسکریپت هیچ لاگ یا رکوردی را حذف نمی‌کند؛ فقط پرچم‌های صف را بازنشانی می‌کند.
   - به‌طور پیش‌فرض DRY-RUN است: فقط تعداد را نشان می‌دهد. برای اجرا @DryRun = 0 کنید.
   - فقط رکوردهای زیر ۳۰ روز (حداکثر عمر داده در ITS) و با پلاک قابل استفاده برمی‌گردند.
   - رکوردهایی که به‌دلیل رد قطعی سرور (کد ۱۰۸/۱۰۹/…)، نبود تصویر، یا انقضای پنجره‌ی تصویر
     رها شده‌اند دست‌نخورده می‌مانند.
   - اسکریپت idempotent است: اجرای دوباره رکورد جدیدی برنمی‌گرداند.
   ============================================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @DryRun  BIT = 1;          -- 1 = فقط گزارش، 0 = اعمال
DECLARE @MaxAgeDays INT = 30;      -- حداکثر عمر مجاز داده در ITS

/* ---------------------------------------------------------------------------------------------
   1) دلایلِ «نامعلوم/گذرا» که بازگردانی برایشان مجاز است.
      الگوها عمداً باریک‌اند تا رد قطعی سرور بازگردانی نشود.
   --------------------------------------------------------------------------------------------- */
DECLARE @RecoverableReasons TABLE (Pattern NVARCHAR(200) NOT NULL);
INSERT INTO @RecoverableReasons (Pattern) VALUES
    (N'%Batch response missing InquiryInfo%'),   -- نتیجه‌ی نامعلومِ batch (علت اصلی)
    (N'%The operation was canceled%'),           -- timeout/لغو گذرا
    (N'%connection attempt failed%'),            -- قطعی شبکه
    (N'%error occurred while sending the request%'),
    (N'%token expired%');

/* ---------------------------------------------------------------------------------------------
   2) انتخاب کاندیدها
   --------------------------------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#Candidates') IS NOT NULL DROP TABLE #Candidates;

SELECT cp.Id AS PhotoId
INTO #Candidates
FROM dbo.CameraPhotos cp
WHERE cp.TerminalSent = 1
  AND cp.TerminalAbandoned = 1
  AND cp.TerminalTtoRegistered = 0
  AND cp.TerminalSentAt >= DATEADD(DAY, -@MaxAgeDays, GETDATE())
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

CREATE CLUSTERED INDEX IX_Candidates ON #Candidates (PhotoId);

DECLARE @Candidates INT = (SELECT COUNT(*) FROM #Candidates);

PRINT '------------------------------------------------------------';
PRINT 'Recoverable abandoned records (dry-run candidate set): ' + CAST(@Candidates AS VARCHAR(20));
PRINT '------------------------------------------------------------';

IF @Candidates > 0
BEGIN
    PRINT 'Breakdown by original reason:';
    SELECT TOP (20)
           LEFT(cp.TerminalLastError, 110) AS Reason,
           COUNT(*)                        AS Records
    FROM #Candidates c
    JOIN dbo.CameraPhotos cp ON cp.Id = c.PhotoId
    GROUP BY LEFT(cp.TerminalLastError, 110)
    ORDER BY Records DESC;
END

IF @DryRun = 1
BEGIN
    PRINT 'DRY-RUN: nothing was changed. Set @DryRun = 0 and re-run to re-queue these records.';
    DROP TABLE #Candidates;
    RETURN;
END

/* ---------------------------------------------------------------------------------------------
   3) بازنشانی صف — فقط پرچم‌های صف، بدون تغییر داده‌ی اصلی یا لاگ‌ها
   --------------------------------------------------------------------------------------------- */
BEGIN TRANSACTION;

UPDATE cp
SET cp.TerminalSent              = 0,
    cp.TerminalSentAt            = NULL,
    cp.TerminalAbandoned         = 0,
    cp.TerminalSendAttempts      = 0,
    cp.TerminalLastAttemptAt     = NULL,
    cp.TerminalImageExpired      = 0,
    cp.TerminalImageExpiredAt    = NULL,
    cp.TerminalImageDeadlineAt   = NULL,
    cp.TerminalLastError         = N''
FROM dbo.CameraPhotos cp
JOIN #Candidates c ON c.PhotoId = cp.Id;

DECLARE @Updated INT = @@ROWCOUNT;

COMMIT TRANSACTION;

PRINT '------------------------------------------------------------';
PRINT 'Records re-queued: ' + CAST(@Updated AS VARCHAR(20));
PRINT '------------------------------------------------------------';

DROP TABLE #Candidates;

/* ---------------------------------------------------------------------------------------------
   4) بررسی پس از اجرا
   --------------------------------------------------------------------------------------------- */
SELECT
    SUM(CASE WHEN TerminalSent = 0 AND TerminalAbandoned = 0 AND TerminalTtoRegistered = 0 THEN 1 ELSE 0 END)
        AS AwaitingTto,
    SUM(CASE WHEN TerminalSent = 1 AND TerminalAbandoned = 1 THEN 1 ELSE 0 END)
        AS StillAbandoned
FROM dbo.CameraPhotos;
