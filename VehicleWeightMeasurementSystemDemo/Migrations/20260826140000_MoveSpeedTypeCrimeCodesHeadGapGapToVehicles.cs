using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class MoveSpeedTypeCrimeCodesHeadGapGapToVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpeedType",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrimeCodes",
                table: "Vehicles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadGap",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gap",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE v SET
                    v.SpeedType = cp.SpeedType,
                    v.CrimeCodes = cp.CrimeCodes,
                    v.HeadGap = cp.HeadGap,
                    v.Gap = cp.Gap
                FROM dbo.Vehicles v
                INNER JOIN dbo.CameraPhotos cp ON cp.VehicleId = v.Id
                WHERE cp.SpeedType IS NOT NULL OR cp.CrimeCodes IS NOT NULL OR cp.HeadGap IS NOT NULL OR cp.Gap IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.CameraPhotos
                DROP COLUMN SpeedType, CrimeCodes, HeadGap, Gap;
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.vw_CameraFullData', N'V') IS NOT NULL
                    EXEC(N'
                        ALTER VIEW dbo.vw_CameraFullData
                        AS
                        SELECT        cp.Id AS PhotoId, v.Id AS VehicleId, v.LineId, v.Timestamp, cp.FileName, cp.RelativePath, cp.FullPath, cp.FileSizeBytes, cp.FileHash, cp.CapturedAt, cp.ImportedAt, cp.PlateP1, cp.PlateP2, cp.PlateP3, cp.PlateP4,
                                         v.PlateConfidence AS PlateConfidence, v.PlateReadStatus AS PlateReadStatus, v.PlateReadAt AS PlateReadAt, cp.PlateBoxLeft, cp.PlateBoxTop, cp.PlateBoxWidth, cp.PlateBoxHeight, cp.PlateFileName, cp.PlateFullPath, cp.PlateRelativePath,
                                         cp.TerminalSent, cp.TerminalSentAt, cp.TerminalLastError, cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt,
                                         cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode, v.Speed AS VehicleSpeed, v.AverageSpeed, v.TotalWeight, v.AxleCount AS TotalAxles, v.VehicleClass, v.Allowed,
                                         v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, v.SpeedType, v.VehicleClass AS CarClass13, v.CrimeCodes, v.PlateConfidence AS OcrScore, v.HeadGap, v.Gap,
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
                                         cp.TerminalImageExpired, cp.TerminalImageExpiredAt, cp.TerminalImageDeadlineAt, cp.TerminalTtoRegistered, cp.TerminalTtoRegisteredAt, cp.TerminalPassInfoId, cp.TerminalPackId, cp.PassInfoId, cp.PreviousDeviceCode,
                                         v.Speed, v.AverageSpeed, v.TotalWeight, v.AxleCount, v.VehicleClass, v.Allowed, v.WrongDirection, v.Longitude, v.Latitude, v.VehicleLen, v.TotalOverWeight, v.WimRawLine, v.SpeedType, v.CrimeCodes,
                                         v.HeadGap, v.Gap, cp.LengthAxles12, cp.LengthAxles23, cp.LengthAxles34, cp.LengthAxles45, cp.LengthAxles56, cp.LengthAxles67, cp.LengthAxles78, cp.LengthAxlesMoreThan8,
                                         cp.TotalWeightA, cp.TotalWeightB, cp.TotalWeightC
                    ');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeedType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CrimeCodes",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "HeadGap",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Gap",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "SpeedType",
                table: "CameraPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrimeCodes",
                table: "CameraPhotos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadGap",
                table: "CameraPhotos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gap",
                table: "CameraPhotos",
                type: "int",
                nullable: true);
        }
    }
}
