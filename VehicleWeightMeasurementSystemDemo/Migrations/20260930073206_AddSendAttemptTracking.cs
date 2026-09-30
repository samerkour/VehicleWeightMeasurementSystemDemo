using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddSendAttemptTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TerminalAbandoned",
                schema: "dbo",
                table: "CameraPhotos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminalLastAttemptAt",
                schema: "dbo",
                table: "CameraPhotos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TerminalSendAttempts",
                schema: "dbo",
                table: "CameraPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TerminalAbandoned", "TerminalLastAttemptAt", "TerminalSendAttempts" },
                values: new object[] { false, null, 0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "TerminalAbandoned", "TerminalLastAttemptAt", "TerminalSendAttempts" },
                values: new object[] { false, null, 0 });

            // 🔥 صف RmtoSync از vw_CameraFullData می‌خواند؛ ویو باید ستون‌های جدید را expose کند،
            // وگرنه فیلتر «سقف تلاش» و backoff روی رکوردهای در حال ارسال اعمال نمی‌شود.
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
                    ')
                """);

            // 🔥 ایندکس صف: انتخاب batch هر چرخه با همین ستون‌ها فیلتر و بر اساس Id مرتب می‌شود.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CameraPhotos_SendQueue' AND object_id = OBJECT_ID(N'dbo.CameraPhotos'))
                    CREATE NONCLUSTERED INDEX IX_CameraPhotos_SendQueue
                        ON dbo.CameraPhotos (TerminalSent, TerminalTtoRegistered, TerminalAbandoned, TerminalSendAttempts)
                        INCLUDE (TerminalLastAttemptAt, Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_CameraPhotos_SendQueue ON dbo.CameraPhotos;");

            // 🔥 بازگرداندن ویو به نسخه‌ی قبل (بدون ستون‌های پیگیری تلاش)
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

            migrationBuilder.DropColumn(
                name: "TerminalAbandoned",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "TerminalLastAttemptAt",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "TerminalSendAttempts",
                schema: "dbo",
                table: "CameraPhotos");
        }
    }
}
