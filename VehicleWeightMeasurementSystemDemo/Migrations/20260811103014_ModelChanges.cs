using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class ModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Lines",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LineCode", "LineName" },
                values: new object[,]
                {
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "L3", "Secondary Line" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "L4", "Secondary Line" }
                });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1,
                column: "LineId",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3,
                column: "LineId",
                value: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Lines",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1,
                column: "LineId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3,
                column: "LineId",
                value: 1);
        }
    }
}
