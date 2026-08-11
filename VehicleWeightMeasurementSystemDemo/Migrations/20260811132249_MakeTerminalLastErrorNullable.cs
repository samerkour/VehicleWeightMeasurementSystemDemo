using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class MakeTerminalLastErrorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TerminalLastError",
                schema: "dbo",
                table: "CameraPhotos",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 1L,
                column: "TerminalLastError",
                value: null);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 2L,
                column: "TerminalLastError",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TerminalLastError",
                schema: "dbo",
                table: "CameraPhotos",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 1L,
                column: "TerminalLastError",
                value: "");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "CameraPhotos",
                keyColumn: "Id",
                keyValue: 2L,
                column: "TerminalLastError",
                value: "");
        }
    }
}
