using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <summary>
    /// هماهنگ‌سازی اسکیمای دوربین/خودرو برای RmtoSync.
    ///
    /// RmtoSync صف ارسال را از ویوی <c>dbo.vw_CameraFullData</c> می‌خواند
    /// (<c>CameraPhotoRecord</c> با <c>ToView</c> نگاشت شده) و وضعیت تلاش/تأیید را روی جدول
    /// <c>dbo.CameraPhotos</c> می‌نویسد. اگر پایگاه‌داده‌ای از زنجیره‌ی مهاجرت‌ها عبور نکرده باشد،
    /// EF کوئری‌ای تولید می‌کند که به ستون‌های موجود اشاره می‌کند و SQL Server خطا می‌دهد:
    ///
    ///     Invalid column name 'TerminalSent'.
    ///     Invalid column name 'TotalWeight'.
    ///     Invalid column name 'VehicleClass'.
    ///
    /// نکته‌ی کلیدی: دو شیء متفاوت درگیرند.
    ///   • جدول  <c>dbo.CameraPhotos</c>      → ستون‌های خانواده‌ی Terminal*
    ///   • ویوی <c>dbo.vw_CameraFullData</c>   → TotalWeight, VehicleClass, AverageSpeed,
    ///                                            VehicleSpeed, PlateReadStatus, PlateConfidence,
    ///                                            Allowed, AxleWeight1..9, TotalAxles,
    ///                                            TotalOverWeight, Latitude, Longitude, ...
    /// افزودن ستون «TotalWeight» به جدول CameraPhotos خطای «Invalid column name 'TotalWeight'»
    /// را برطرف نمی‌کند، چون EF آن نام را از ویو می‌خواند. ویو باید آن را expose کند.
    ///
    /// تمام گام‌ها با COL_LENGTH/OBJECT_ID محافظت شده‌اند ⇒ اجرای مجدد بی‌خطر است.
    /// هیچ ستونی حذف یا بازنویسی نمی‌شود ⇒ بدون از دست رفتن داده و سازگار با نسخه‌ی قبلی.
    /// </summary>
    public partial class AddMissingCameraPhotoColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─────────────────────────────────────────────────────────────────────────────
            // گام ۱ — جدول dbo.CameraPhotos : ستون‌هایی که مدل و ویو لازم دارند
            // ─────────────────────────────────────────────────────────────────────────────
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.CameraPhotos', N'U') IS NULL
                    THROW 51000, 'dbo.CameraPhotos does not exist in this database. RmtoSync cannot operate against it.', 1;
                """);

            // کلید join که ویو برای اتصالVehicles به CameraPhotos استفاده می‌کند
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'VehicleId') IS NULL
                    ALTER TABLE dbo.CameraPhotos ADD VehicleId int NULL;
                """);

            // پلاک
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP1') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP1 nvarchar(10) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP2') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP2 nvarchar(10) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP3') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP3 nvarchar(10) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateP4') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateP4 nvarchar(10) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxLeft')   IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxLeft   int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxTop')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxTop    int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxWidth')  IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxWidth  int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateBoxHeight') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateBoxHeight int NULL;
                """);

            // مسیر تصاویر پلاک
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateFileName')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateFileName    nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateFullPath')    IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateFullPath    nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PlateRelativePath') IS NULL ALTER TABLE dbo.CameraPhotos ADD PlateRelativePath nvarchar(max) NULL;
                """);

            // فواصل محورها
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles12')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles12        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles23')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles23        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles34')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles34        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles45')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles45        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles56')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles56        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles67')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles67        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxles78')        IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxles78        int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'LengthAxlesMoreThan8') IS NULL ALTER TABLE dbo.CameraPhotos ADD LengthAxlesMoreThan8 int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightA')         IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightA         int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightB')         IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightB         int NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TotalWeightC')         IS NULL ALTER TABLE dbo.CameraPhotos ADD TotalWeightC         int NULL;
                """);

            // وضعیت تأیید/تلاش (ستون‌های صف RmtoSync)
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSent')            IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalSent            bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalSent            DEFAULT (0);
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSentAt')          IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalSentAt          datetime2 NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalLastError')       IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalLastError       nvarchar(4000) NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalTtoRegistered')   IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalTtoRegistered   bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalTtoRegistered   DEFAULT (0);
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalTtoRegisteredAt') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalTtoRegisteredAt datetime2 NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageDeadlineAt') IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageDeadlineAt datetime2 NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageExpired')    IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpired    bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalImageExpired    DEFAULT (0);
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalImageExpiredAt')  IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalImageExpiredAt  datetime2 NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalPassInfoId')      IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalPassInfoId      bigint NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalPackId')          IS NULL ALTER TABLE dbo.CameraPhotos ADD TerminalPackId          bigint NULL;
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PassInfoId')              IS NULL ALTER TABLE dbo.CameraPhotos ADD PassInfoId              bigint NULL;
                """);

            // PreviousDeviceCode برای اعتبارسنجی نوع سرعت لازم است (TtoPreSendPattern آن را
            // رد می‌کند اگر <= 0 باشد)، پس باید قابل انتخاب بماند.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'PreviousDeviceCode') IS NULL
                    ALTER TABLE dbo.CameraPhotos ADD PreviousDeviceCode bigint NULL;
                """);

            // 🔥 شمارش تلاش — بدون این‌ها رکوردِ دائماً ناموفق هر چرخه دوباره انتخاب می‌شود
            //    و جلوی پردازش بقیه‌ی صف را می‌گیرد (همان باگی که این مهاجرت رفعش می‌کند).
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSendAttempts') IS NULL
                    ALTER TABLE dbo.CameraPhotos ADD TerminalSendAttempts int NOT NULL CONSTRAINT DF_CameraPhotos_TerminalSendAttempts DEFAULT (0);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalLastAttemptAt') IS NULL
                    ALTER TABLE dbo.CameraPhotos ADD TerminalLastAttemptAt datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalAbandoned') IS NULL
                    ALTER TABLE dbo.CameraPhotos ADD TerminalAbandoned bit NOT NULL CONSTRAINT DF_CameraPhotos_TerminalAbandoned DEFAULT (0);
                """);

            // ─────────────────────────────────────────────────────────────────────────────
            // گام ۲ — جدول‌های Vehicles / Axles : ستون‌هایی که ویو به آن‌ها ارجاع می‌دهد
            //          (MaxAllowed* از مهاجرت‌های بعدی آمده و رایج‌ترین شکاف است)
            // ─────────────────────────────────────────────────────────────────────────────
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Vehicles', N'MaxAllowedSpeedForClass') IS NULL
                    ALTER TABLE dbo.Vehicles ADD MaxAllowedSpeedForClass int NOT NULL CONSTRAINT DF_Vehicles_MaxAllowedSpeedForClass DEFAULT (0);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Vehicles', N'MaxAllowedWeightForClass') IS NULL
                    ALTER TABLE dbo.Vehicles ADD MaxAllowedWeightForClass float NOT NULL CONSTRAINT DF_Vehicles_MaxAllowedWeightForClass DEFAULT (0);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Axles', N'AxleIndex') IS NULL ALTER TABLE dbo.Axles ADD AxleIndex int NULL;
                IF COL_LENGTH(N'dbo.Axles', N'Weight')    IS NULL ALTER TABLE dbo.Axles ADD Weight float NULL;
                """);

            // ─────────────────────────────────────────────────────────────────────────────
            // گام ۳ — 🔥 dbo.vw_CameraFullData : اصلاح خطاهای TotalWeight / VehicleClass /
            //          AxleWeight1..9 / PlateReadStatus / Allowed / …
            //
            //          این ویو تنها جایی است که آن نام‌ها برای EF تعریف شده‌اند، چون
            //          CameraPhotoRecord با ToView نگاشت شده است. بازسازی ویو به تعریف
            //          مرجع ۸۱ ستونی است که کوئری EF را معتبر می‌کند.
            // ─────────────────────────────────────────────────────────────────────────────
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Vehicles', N'U')     IS NULL THROW 51001, 'dbo.Vehicles is missing — cannot create vw_CameraFullData.', 1;
                """);
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Axles', N'U')        IS NULL THROW 51002, 'dbo.Axles is missing — cannot create vw_CameraFullData.', 1;
                """);
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.CameraPhotos', N'U') IS NULL THROW 51003, 'dbo.CameraPhotos is missing — cannot create vw_CameraFullData.', 1;
                """);

            migrationBuilder.Sql("""
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
                """);

            // ─────────────────────────────────────────────────────────────────────────────
            // گام ۴ — ایندکس صف: هر چرخه بر اساس همین ستون‌ها فیلتر و بر اساس Id مرتب می‌کند
            // ─────────────────────────────────────────────────────────────────────────────
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CameraPhotos_SendQueue' AND object_id = OBJECT_ID(N'dbo.CameraPhotos'))
                    CREATE NONCLUSTERED INDEX IX_CameraPhotos_SendQueue
                        ON dbo.CameraPhotos (TerminalSent, TerminalTtoRegistered, TerminalAbandoned, TerminalSendAttempts)
                        INCLUDE (TerminalLastAttemptAt, Id);
                """);
        }

        /// <inheritdoc />
        /// <remarks>
        /// فقط گام‌هایی که این مهاجرت اضافه کرده معکوس می‌شوند: ایندکس صف، سه ستون
        /// پیگیری تلاش، و ویو به تعریف پیشین. توجه: حذف این سه ستون، وضعیت تلاش/رهاسازی
        /// رکوردها را از بین می‌برد؛ هیچ ستون پیشینی حذف نمی‌شود.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CameraPhotos_SendQueue' AND object_id = OBJECT_ID(N'dbo.CameraPhotos'))
                    DROP INDEX IX_CameraPhotos_SendQueue ON dbo.CameraPhotos;
                """);

            // بازگرداندن ویو به تعریف پیشین (بدون ستون‌های پیگیری تلاش)
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.vw_CameraFullData', N'V') IS NOT NULL
                    EXEC(N'
                        ALTER VIEW dbo.vw_CameraFullData
                        AS
                        SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                                         v.PlateConfidence AS PlateConfidence, v.PlateReadStatus AS PlateReadStatus, v.PlateReadAt AS PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath,
                                         cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt,
                                         cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed AS VehicleSpeed, v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed,
                                         v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, v.SpeedType, v.VehicleClass AS CarClass13, v.CrimeCodes, v.PlateConfidence AS OcrScore, v.HeadGap, v.Gap,
                                         v.MaxAllowedSpeedForClass, v.MaxAllowedWeightForClass, v.VehicleLen AS FirstToLastAxlesLen, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC,
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
                                 cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC
                    ')
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalAbandoned') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_CameraPhotos_TerminalAbandoned')
                        ALTER TABLE dbo.CameraPhotos DROP CONSTRAINT DF_CameraPhotos_TerminalAbandoned;
                    ALTER TABLE dbo.CameraPhotos DROP COLUMN TerminalAbandoned;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalLastAttemptAt') IS NOT NULL
                    ALTER TABLE dbo.CameraPhotos DROP COLUMN TerminalLastAttemptAt;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.CameraPhotos', N'TerminalSendAttempts') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_CameraPhotos_TerminalSendAttempts')
                        ALTER TABLE dbo.CameraPhotos DROP CONSTRAINT DF_CameraPhotos_TerminalSendAttempts;
                    ALTER TABLE dbo.CameraPhotos DROP COLUMN TerminalSendAttempts;
                END
                """);
        }
    }
}
