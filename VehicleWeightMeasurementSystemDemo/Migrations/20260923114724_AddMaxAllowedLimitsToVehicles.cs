using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxAllowedLimitsToVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAllowedSpeedForClass",
                schema: "dbo",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 110);

            migrationBuilder.AddColumn<double>(
                name: "MaxAllowedWeightForClass",
                schema: "dbo",
                table: "Vehicles",
                type: "float",
                nullable: false,
                defaultValue: 44000.0);

            // 🔥 Backfill seed rows so HasData stays in sync with the model.
            // Existing production rows are already filled by the DEFAULT constraint at ALTER time.
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MaxAllowedSpeedForClass", "MaxAllowedWeightForClass" },
                values: new object[] { 110, 44000.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaxAllowedSpeedForClass", "MaxAllowedWeightForClass" },
                values: new object[] { 110, 44000.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaxAllowedSpeedForClass", "MaxAllowedWeightForClass" },
                values: new object[] { 110, 44000.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAllowedSpeedForClass",
                schema: "dbo",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MaxAllowedWeightForClass",
                schema: "dbo",
                table: "Vehicles");
        }
    }
}