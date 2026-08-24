using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddPlateImageFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 قبل از NOT NULL کردن ستون، مقادیر NULL صفر شوند (وگرنه ALTER روی دیتای موجود خطا می‌دهد)
            migrationBuilder.Sql(
                "UPDATE [dbo].[Vehicles] SET [TotalOverWeight] = 0 WHERE [TotalOverWeight] IS NULL");

            migrationBuilder.AlterColumn<double>(
                name: "TotalOverWeight",
                schema: "dbo",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlateFileName",
                schema: "dbo",
                table: "CameraPhotos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlateFullPath",
                schema: "dbo",
                table: "CameraPhotos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlateRelativePath",
                schema: "dbo",
                table: "CameraPhotos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "PlateFileName", "PlateFullPath", "PlateRelativePath" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "PlateFileName", "PlateFullPath", "PlateRelativePath" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 1,
                column: "LineName",
                value: "Line1");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 2,
                column: "LineName",
                value: "Line2");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 3,
                column: "LineName",
                value: "Line3");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 4,
                column: "LineName",
                value: "Line4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlateFileName",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PlateFullPath",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.DropColumn(
                name: "PlateRelativePath",
                schema: "dbo",
                table: "CameraPhotos");

            migrationBuilder.AlterColumn<double>(
                name: "TotalOverWeight",
                schema: "dbo",
                table: "Vehicles",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 1,
                column: "LineName",
                value: "Main Line");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 2,
                column: "LineName",
                value: "Secondary Line");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 3,
                column: "LineName",
                value: "Secondary Line");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 4,
                column: "LineName",
                value: "Secondary Line");
        }
    }
}
