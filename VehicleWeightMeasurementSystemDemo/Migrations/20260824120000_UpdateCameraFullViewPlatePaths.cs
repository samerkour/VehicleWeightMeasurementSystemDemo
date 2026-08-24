using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    [Migration("20260824120000_UpdateCameraFullViewPlatePaths")]
    public partial class UpdateCameraFullViewPlatePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.vw_CameraFullData', N'V') IS NOT NULL
                    EXEC(N'
                        ALTER VIEW dbo.vw_CameraFullData
                        AS
                        SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                                         cp.PlateConfidence, cp.PlateReadStatus, cp.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath,
                                         cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt,
                                         cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed AS VehicleSpeed, v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed,
                                         v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, cp.CarClass13, cp.CrimeCodes, cp.OcrScore, cp.HeadGap, cp.Gap, cp.FirstToLastAxlesLen,
                                         cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8, cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC,
                                         MAX(CASE WHEN a.AxleIndex = 1 THEN a.Weight END) AS AxleWeight1, MAX(CASE WHEN a.AxleIndex = 2 THEN a.Weight END) AS AxleWeight2, MAX(CASE WHEN a.AxleIndex = 3 THEN a.Weight END) AS AxleWeight3,
                                         MAX(CASE WHEN a.AxleIndex = 4 THEN a.Weight END) AS AxleWeight4, MAX(CASE WHEN a.AxleIndex = 5 THEN a.Weight END) AS AxleWeight5, MAX(CASE WHEN a.AxleIndex = 6 THEN a.Weight END) AS AxleWeight6,
                                         MAX(CASE WHEN a.AxleIndex = 7 THEN a.Weight END) AS AxleWeight7, MAX(CASE WHEN a.AxleIndex = 8 THEN a.Weight END) AS AxleWeight8, MAX(CASE WHEN a.AxleIndex = 9 THEN a.Weight END) AS AxleWeight9
                        FROM            dbo.Vehicles AS v LEFT OUTER JOIN
                                         dbo.CameraPhotos AS cp ON cp.VehicleId = v.Id LEFT OUTER JOIN
                                         dbo.Axles AS a ON a.VehicleId = v.Id
                        GROUP BY cp.Id, v.Id, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4, cp.PlateConfidence, cp.PlateReadStatus,
                                         cp.PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath, cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError,
                                         cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode,
                                         v.Speed, v.AverageSpeed, v.TotalWeight, v.AxleCount, v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, cp.SpeedType, cp.CarClass13, cp.CrimeCodes,
                                         cp.OcrScore, cp.HeadGap, cp.Gap, cp.FirstToLastAxlesLen, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC
                    ')
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
