using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class DropCameraPhotosRedundantColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop index that depends on PlateReadStatus before dropping the column
            migrationBuilder.Sql("DROP INDEX IX_CameraPhotos_TerminalTtoPending ON dbo.CameraPhotos;");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.CameraPhotos
                DROP COLUMN
                    CarClass13,
                    TotalOverWeight,
                    FirstToLastAxlesLen,
                    PlateReadAt,
                    Allowed,
                    WrongDirection,
                    PlateConfidence,
                    PlateReadStatus,
                    OcrScore;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.CameraPhotos ADD
                    PlateConfidence float NULL,
                    PlateReadStatus int NOT NULL CONSTRAINT DF_CameraPhotos_PlateReadStatus DEFAULT 0,
                    PlateReadAt datetime2 NULL,
                    Allowed bit NULL,
                    WrongDirection bit NULL,
                    CarClass13 tinyint NULL,
                    TotalOverWeight int NULL,
                    FirstToLastAxlesLen int NULL,
                    OcrScore decimal(18,2) NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_CameraPhotos_TerminalTtoPending
                ON dbo.CameraPhotos (PlateReadStatus, Id)
                WHERE ([PlateReadStatus]=(1) AND [TerminalSent]=(0) AND [TerminalTtoRegistered]=(0));
            ");
        }
    }
}
