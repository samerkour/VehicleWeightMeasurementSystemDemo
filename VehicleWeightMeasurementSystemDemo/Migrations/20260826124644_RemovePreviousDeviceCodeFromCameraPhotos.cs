using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class RemovePreviousDeviceCodeFromCameraPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 اول ویو باید بدون PreviousDeviceCode بازسازی شود وگرنه DropColumn خطا می‌دهد
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.vw_CameraFullData', N'V') IS NOT NULL
                    EXEC(N'
                        ALTER VIEW dbo.vw_CameraFullData
                        AS
                        SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                                         v.PlateConfidence AS PlateConfidence, v.PlateReadStatus AS PlateReadStatus, v.PlateReadAt AS PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath,
                                         cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt,
                                         cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, v.Speed AS VehicleSpeed, v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed,
                                         v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, v.VehicleClass AS CarClass13, cp.CrimeCodes, v.PlateConfidence AS OcrScore, cp.HeadGap, cp.Gap,
                                         v.VehicleLen AS FirstToLastAxlesLen, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC,
                                         MAX(CASE WHEN a.AxleIndex = 1 THEN a.Weight END) AS AxleWeight1, MAX(CASE WHEN a.AxleIndex = 2 THEN a.Weight END) AS AxleWeight2, MAX(CASE WHEN a.AxleIndex = 3 THEN a.Weight END) AS AxleWeight3,
                                         MAX(CASE WHEN a.AxleIndex = 4 THEN a.Weight END) AS AxleWeight4, MAX(CASE WHEN a.AxleIndex = 5 THEN a.Weight END) AS AxleWeight5, MAX(CASE WHEN a.AxleIndex = 6 THEN a.Weight END) AS AxleWeight6,
                                         MAX(CASE WHEN a.AxleIndex = 7 THEN a.Weight END) AS AxleWeight7, MAX(CASE WHEN a.AxleIndex = 8 THEN a.Weight END) AS AxleWeight8, MAX(CASE WHEN a.AxleIndex = 9 THEN a.Weight END) AS AxleWeight9
                        FROM            dbo.Vehicles AS v LEFT OUTER JOIN
                                         dbo.CameraPhotos AS cp ON cp.VehicleId = v.Id LEFT OUTER JOIN
                                         dbo.Axles AS a ON a.VehicleId = v.Id
                        GROUP BY cp.Id, v.Id, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4, v.PlateConfidence, v.PlateReadStatus,
                                         v.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath, cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError,
                                         cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId,
                                         v.Speed, v.AverageSpeed, v.TotalWeight, v.AxleCount, v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, cp.CrimeCodes,
                                         cp.HeadGap, cp.Gap, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC
                    ')
                """);

            migrationBuilder.DropIndex(
                name: "IX_CameraPhotos_TerminalTtoPending",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "Allowed",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "CarClass13",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "FirstToLastAxlesLen",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "OcrScore",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PlateConfidence",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PlateReadAt",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PlateReadStatus",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PreviousDeviceCode",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "TotalOverWeight",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "WrongDirection",
                schema: "dbo",
                table: "CameraPhotos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Allowed",
                schema: "dbo",
                table: "CameraPhotos",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarClass13",
                schema: "dbo",
                table: "CameraPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FirstToLastAxlesLen",
                schema: "dbo",
                table: "CameraPhotos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OcrScore",
                schema: "dbo",
                table: "CameraPhotos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PlateConfidence",
                schema: "dbo",
                table: "CameraPhotos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlateReadAt",
                schema: "dbo",
                table: "CameraPhotos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlateReadStatus",
                schema: "dbo",
                table: "CameraPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviousDeviceCode",
                schema: "dbo",
                table: "CameraPhotos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalOverWeight",
                schema: "dbo",
                table: "CameraPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WrongDirection",
                schema: "dbo",
                table: "CameraPhotos",
                type: "bit",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Allowed", "CarClass13", "FirstToLastAxlesLen", "OcrScore", "PlateConfidence", "PlateReadAt", "PlateReadStatus", "PreviousDeviceCode", "TotalOverWeight", "WrongDirection" },
                values: new object[] { true, null, null, 95.2m, 92.5, new DateTime(2025, 1, 10, 8, 30, 1, 0, DateTimeKind.Unspecified), 1, null, null, false });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Allowed", "CarClass13", "FirstToLastAxlesLen", "OcrScore", "PlateConfidence", "PlateReadAt", "PlateReadStatus", "PreviousDeviceCode", "TotalOverWeight", "WrongDirection" },
                values: new object[] { true, null, null, 88.7m, 88.0, new DateTime(2025, 1, 10, 9, 15, 1, 0, DateTimeKind.Unspecified), 1, null, null, false });

            migrationBuilder.CreateIndex(
                name: "IX_CameraPhotos_TerminalTtoPending",
                schema: "dbo",
                table: "CameraPhotos",
                columns: new[] { "PlateReadStatus", "Id" },
                filter: "[TerminalSent] = 0 AND [TerminalTtoRegistered] = 0");

            // 🔥 بازگرداندن ویو به نسخه‌ی قبلی (دارای PreviousDeviceCode)
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.vw_CameraFullData', N'V') IS NOT NULL
                    EXEC(N'
                        ALTER VIEW dbo.vw_CameraFullData
                        AS
                        SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                                         cp.PlateConfidence, cp.PlateReadStatus, cp.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired,
                                         cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed AS VehicleSpeed,
                                         v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, cp.CarClass13, cp.CrimeCodes,
                                         cp.OcrScore, cp.HeadGap, cp.Gap, cp.FirstToLastAxlesLen, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC, MAX(CASE WHEN a.AxleIndex = 1 THEN a.Weight END) AS AxleWeight1, MAX(CASE WHEN a.AxleIndex = 2 THEN a.Weight END) AS AxleWeight2,
                                         MAX(CASE WHEN a.AxleIndex = 3 THEN a.Weight END) AS AxleWeight3, MAX(CASE WHEN a.AxleIndex = 4 THEN a.Weight END) AS AxleWeight4, MAX(CASE WHEN a.AxleIndex = 5 THEN a.Weight END) AS AxleWeight5,
                                         MAX(CASE WHEN a.AxleIndex = 6 THEN a.Weight END) AS AxleWeight6, MAX(CASE WHEN a.AxleIndex = 7 THEN a.Weight END) AS AxleWeight7, MAX(CASE WHEN a.AxleIndex = 8 THEN a.Weight END) AS AxleWeight8,
                                         MAX(CASE WHEN a.AxleIndex = 9 THEN a.Weight END) AS AxleWeight9
                        FROM            dbo.Vehicles AS v LEFT OUTER JOIN
                                         dbo.CameraPhotos AS cp ON cp.VehicleId = v.Id LEFT OUTER JOIN
                                         dbo.Axles AS a ON a.VehicleId = v.Id
                        GROUP BY cp.Id, v.Id, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4, cp.PlateConfidence, cp.PlateReadStatus,
                                         cp.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt,
                                         cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed, v.AverageSpeed, v.TotalWeight, v.AxleCount,
                                         v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, cp.CarClass13, cp.CrimeCodes, cp.OcrScore, cp.HeadGap, cp.Gap, cp.FirstToLastAxlesLen,
                                         cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8, cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC
                    ')
                """);
        }
    }
}
