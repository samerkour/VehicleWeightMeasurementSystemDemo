using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleWeightMeasurementSystemDemo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Lines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PlateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Speed = table.Column<double>(type: "float", nullable: true),
                    AverageSpeed = table.Column<double>(type: "float", nullable: true),
                    AxleCount = table.Column<int>(type: "int", nullable: true),
                    TotalWeight = table.Column<double>(type: "float", nullable: true),
                    ADC1 = table.Column<string>(type: "varchar(16)", nullable: true),
                    ADC2 = table.Column<string>(type: "varchar(16)", nullable: true),
                    ADC3 = table.Column<string>(type: "varchar(16)", nullable: true),
                    ADC4 = table.Column<string>(type: "varchar(16)", nullable: true),
                    LineId = table.Column<int>(type: "int", nullable: true),
                    PlateConfidence = table.Column<double>(type: "float", nullable: true),
                    PlateReadStatus = table.Column<int>(type: "int", nullable: true),
                    PlateReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Allowed = table.Column<bool>(type: "bit", nullable: true),
                    WrongDirection = table.Column<bool>(type: "bit", nullable: true),
                    VehicleClass = table.Column<int>(type: "int", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    VehicleLen = table.Column<double>(type: "float", nullable: true),
                    TotalOverWeight = table.Column<double>(type: "float", nullable: true),
                    WimRawLine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Lines_LineId",
                        column: x => x.LineId,
                        principalSchema: "dbo",
                        principalTable: "Lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Axles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    AxleIndex = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    TimeMs = table.Column<double>(type: "float", nullable: true),
                    Distance = table.Column<double>(type: "float", nullable: true),
                    LengthToNext = table.Column<double>(type: "float", nullable: true),
                    IsOverweight = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Axles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Axles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "dbo",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CameraPhotos",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FullPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    PlateP1 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PlateP2 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PlateP3 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PlateP4 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PlateConfidence = table.Column<double>(type: "float", nullable: true),
                    PlateReadStatus = table.Column<int>(type: "int", nullable: true),
                    PlateReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlateBoxLeft = table.Column<int>(type: "int", nullable: true),
                    PlateBoxTop = table.Column<int>(type: "int", nullable: true),
                    PlateBoxWidth = table.Column<int>(type: "int", nullable: true),
                    PlateBoxHeight = table.Column<int>(type: "int", nullable: true),
                    TerminalSent = table.Column<bool>(type: "bit", nullable: false),
                    TerminalSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminalTtoRegistered = table.Column<bool>(type: "bit", nullable: false),
                    TerminalImageExpired = table.Column<bool>(type: "bit", nullable: false),
                    TerminalLastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SpeedType = table.Column<int>(type: "int", nullable: true),
                    Allowed = table.Column<bool>(type: "bit", nullable: true),
                    WrongDirection = table.Column<bool>(type: "bit", nullable: true),
                    FirstToLastAxlesLen = table.Column<double>(type: "float", nullable: true),
                    TerminalImageExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminalTtoRegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminalPassInfoId = table.Column<long>(type: "bigint", nullable: true),
                    TerminalPackId = table.Column<long>(type: "bigint", nullable: true),
                    TerminalImageDeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CarClass13 = table.Column<int>(type: "int", nullable: true),
                    CrimeCodes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OcrScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    HeadGap = table.Column<int>(type: "int", nullable: true),
                    Gap = table.Column<int>(type: "int", nullable: true),
                    LengthAxles12 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles23 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles34 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles45 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles56 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles67 = table.Column<int>(type: "int", nullable: true),
                    LengthAxles78 = table.Column<int>(type: "int", nullable: true),
                    LengthAxlesMoreThan8 = table.Column<int>(type: "int", nullable: true),
                    TotalWeightA = table.Column<int>(type: "int", nullable: true),
                    TotalWeightB = table.Column<int>(type: "int", nullable: true),
                    TotalWeightC = table.Column<int>(type: "int", nullable: true),
                    TotalOverWeight = table.Column<int>(type: "int", nullable: true),
                    PassInfoId = table.Column<long>(type: "bigint", nullable: true),
                    PreviousDeviceCode = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraPhotos_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "dbo",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Lines",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LineCode", "LineName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "L1", "Main Line" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "L2", "Secondary Line" }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Vehicles",
                columns: new[] { "Id", "ADC1", "ADC2", "ADC3", "ADC4", "Allowed", "AverageSpeed", "AxleCount", "Latitude", "LineId", "Longitude", "PlateConfidence", "PlateNumber", "PlateReadAt", "PlateReadStatus", "Speed", "Timestamp", "TotalOverWeight", "TotalWeight", "VehicleClass", "VehicleLen", "WimRawLine", "WrongDirection" },
                values: new object[,]
                {
                    { 1, null, null, null, null, true, 60.0, 3, null, 1, null, 92.5, "‏۱۱ ایران ۲۴۴ س ۱۸", new DateTime(2025, 1, 10, 8, 30, 1, 0, DateTimeKind.Unspecified), 1, 62.5, new DateTime(2025, 1, 10, 8, 30, 0, 0, DateTimeKind.Unspecified), 0.0, 24500.0, 2, 12.4, null, false },
                    { 2, null, null, null, null, true, 52.0, 2, null, 1, null, 88.0, "‏۴۴ ایران ۵۵۵ ب ۷۷", new DateTime(2025, 1, 10, 9, 15, 1, 0, DateTimeKind.Unspecified), 1, 55.0, new DateTime(2025, 1, 10, 9, 15, 0, 0, DateTimeKind.Unspecified), 500.0, 18000.0, 3, 9.8000000000000007, null, false },
                    { 3, null, null, null, null, false, 45.0, 4, null, 1, null, 75.5, "‏۳۳ ایران ۱۱۲ ب ۹۹", new DateTime(2025, 1, 10, 10, 5, 1, 0, DateTimeKind.Unspecified), 1, 48.0, new DateTime(2025, 1, 10, 10, 5, 0, 0, DateTimeKind.Unspecified), 3500.0, 32000.0, 4, 15.199999999999999, null, false }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Axles",
                columns: new[] { "Id", "AxleIndex", "Distance", "IsOverweight", "LengthToNext", "TimeMs", "VehicleId", "Weight" },
                values: new object[,]
                {
                    { 1, 1, 0.0, false, null, 100.0, 1, 8000.0 },
                    { 2, 2, 4.2000000000000002, false, null, 250.0, 1, 8200.0 },
                    { 3, 3, 8.0999999999999996, false, null, 400.0, 1, 8300.0 },
                    { 4, 1, 0.0, false, null, 120.0, 2, 9000.0 },
                    { 5, 2, 5.5999999999999996, true, null, 300.0, 2, 9000.0 },
                    { 6, 1, 0.0, false, null, 90.0, 3, 8000.0 },
                    { 7, 2, 4.0, false, null, 220.0, 3, 8000.0 },
                    { 8, 3, 8.0, false, null, 350.0, 3, 8000.0 },
                    { 9, 4, 12.0, true, null, 480.0, 3, 8000.0 }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "CameraPhotos",
                columns: new[] { "Id", "Allowed", "CapturedAt", "CarClass13", "CrimeCodes", "FileHash", "FileName", "FileSizeBytes", "FirstToLastAxlesLen", "FullPath", "Gap", "HeadGap", "ImportedAt", "LengthAxles12", "LengthAxles23", "LengthAxles34", "LengthAxles45", "LengthAxles56", "LengthAxles67", "LengthAxles78", "LengthAxlesMoreThan8", "OcrScore", "PassInfoId", "PlateBoxHeight", "PlateBoxLeft", "PlateBoxTop", "PlateBoxWidth", "PlateConfidence", "PlateP1", "PlateP2", "PlateP3", "PlateP4", "PlateReadAt", "PlateReadStatus", "PreviousDeviceCode", "RelativePath", "SpeedType", "TerminalImageDeadlineAt", "TerminalImageExpired", "TerminalImageExpiredAt", "TerminalLastError", "TerminalPackId", "TerminalPassInfoId", "TerminalSent", "TerminalSentAt", "TerminalTtoRegistered", "TerminalTtoRegisteredAt", "TotalOverWeight", "TotalWeightA", "TotalWeightB", "TotalWeightC", "VehicleId", "WrongDirection" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2025, 1, 10, 8, 30, 0, 0, DateTimeKind.Unspecified), null, null, null, "car_00001.jpg", 204800L, null, "C:\\Temp\\Snapshots\\car_00001.jpg", null, null, new DateTime(2025, 1, 10, 8, 30, 2, 0, DateTimeKind.Unspecified), null, null, null, null, null, null, null, null, 95.2m, null, null, null, null, null, 92.5, "۱۸", "س", "۲۴۴", "۱۱", new DateTime(2025, 1, 10, 8, 30, 1, 0, DateTimeKind.Unspecified), 1, null, "Photos/car_00001.jpg", 1, null, false, null, "", null, null, true, null, true, null, null, null, null, null, 1, false },
                    { 2L, true, new DateTime(2025, 1, 10, 9, 15, 0, 0, DateTimeKind.Unspecified), null, null, null, "car_00002.jpg", 185344L, null, "C:\\Temp\\Snapshots\\car_00002.jpg", null, null, new DateTime(2025, 1, 10, 9, 15, 2, 0, DateTimeKind.Unspecified), null, null, null, null, null, null, null, null, 88.7m, null, null, null, null, null, 88.0, "۷۷", "ب", "۵۵۵", "۴۴", new DateTime(2025, 1, 10, 9, 15, 1, 0, DateTimeKind.Unspecified), 1, null, "Photos/car_00002.jpg", 1, null, false, null, "", null, null, false, null, false, null, null, null, null, null, 2, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Axles_VehicleId",
                schema: "dbo",
                table: "Axles",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraPhotos_CapturedAt",
                schema: "dbo",
                table: "CameraPhotos",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CameraPhotos_TerminalSent_TerminalTtoRegistered_TerminalImageExpired",
                schema: "dbo",
                table: "CameraPhotos",
                columns: new[] { "TerminalSent", "TerminalTtoRegistered", "TerminalImageExpired" });

            migrationBuilder.CreateIndex(
                name: "IX_CameraPhotos_VehicleId",
                schema: "dbo",
                table: "CameraPhotos",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_LineCode",
                schema: "dbo",
                table: "Lines",
                column: "LineCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LineId",
                schema: "dbo",
                table: "Vehicles",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Timestamp",
                schema: "dbo",
                table: "Vehicles",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Axles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CameraPhotos",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Lines",
                schema: "dbo");
        }
    }
}
