using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDriversDistanceAndBookingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assigned_driver_id",
                table: "mst_car",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mst_driver",
                columns: table => new
                {
                    driver_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    driver_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    license_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_driver", x => x.driver_id);
                });

            migrationBuilder.CreateTable(
                name: "mst_location_distance",
                columns: table => new
                {
                    distance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    from_location_id = table.Column<int>(type: "int", nullable: false),
                    to_location_id = table.Column<int>(type: "int", nullable: false),
                    distance_km = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_location_distance", x => x.distance_id);
                    table.ForeignKey(
                        name: "FK_mst_location_distance_mst_location_from_location_id",
                        column: x => x.from_location_id,
                        principalTable: "mst_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mst_location_distance_mst_location_to_location_id",
                        column: x => x.to_location_id,
                        principalTable: "mst_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trn_booking_detail",
                columns: table => new
                {
                    booking_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    car_id = table.Column<int>(type: "int", nullable: false),
                    car_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    car_brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    from_location_id = table.Column<int>(type: "int", nullable: false),
                    from_location_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    to_location_id = table.Column<int>(type: "int", nullable: false),
                    to_location_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    distance_km = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    driver_id = table.Column<int>(type: "int", nullable: true),
                    driver_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    booking_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    booking_time = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trn_booking_detail", x => x.booking_detail_id);
                    table.ForeignKey(
                        name: "FK_trn_booking_detail_mst_driver_driver_id",
                        column: x => x.driver_id,
                        principalTable: "mst_driver",
                        principalColumn: "driver_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trn_booking_detail_mst_location_from_location_id",
                        column: x => x.from_location_id,
                        principalTable: "mst_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trn_booking_detail_mst_location_to_location_id",
                        column: x => x.to_location_id,
                        principalTable: "mst_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trn_booking_detail_trn_booking_booking_id",
                        column: x => x.booking_id,
                        principalTable: "trn_booking",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 1,
                column: "assigned_driver_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 2,
                column: "assigned_driver_id",
                value: 2);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 3,
                column: "assigned_driver_id",
                value: 3);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 4,
                column: "assigned_driver_id",
                value: 4);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 5,
                column: "assigned_driver_id",
                value: 5);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 6,
                column: "assigned_driver_id",
                value: 6);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 7,
                column: "assigned_driver_id",
                value: 7);

            migrationBuilder.UpdateData(
                table: "mst_car",
                keyColumn: "car_id",
                keyValue: 8,
                column: "assigned_driver_id",
                value: 8);

            migrationBuilder.InsertData(
                table: "mst_driver",
                columns: new[] { "driver_id", "created_date", "driver_name", "is_active", "license_number", "phone_number" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ramesh Yadav", true, "MP-DL-000001", "9800000001" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Suresh Kumar", true, "MP-DL-000002", "9800000002" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Anil Verma", true, "DL-DL-000003", "9800000003" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Vikram Singh", true, "MP-DL-000004", "9800000004" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rajesh Chauhan", true, "MH-DL-000005", "9800000005" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Manoj Tiwari", true, "MP-DL-000006", "9800000006" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sanjay Mehta", true, "DL-DL-000007", "9800000007" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deepak Rathore", true, "MP-DL-000008", "9800000008" }
                });

            migrationBuilder.InsertData(
                table: "mst_location_distance",
                columns: new[] { "distance_id", "distance_km", "from_location_id", "to_location_id" },
                values: new object[,]
                {
                    { 1, 190m, 1, 2 },
                    { 2, 190m, 2, 1 },
                    { 3, 740m, 1, 3 },
                    { 4, 740m, 3, 1 },
                    { 5, 780m, 1, 4 },
                    { 6, 780m, 4, 1 },
                    { 7, 820m, 2, 3 },
                    { 8, 820m, 3, 2 },
                    { 9, 580m, 2, 4 },
                    { 10, 580m, 4, 2 },
                    { 11, 1400m, 3, 4 },
                    { 12, 1400m, 4, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_mst_car_assigned_driver_id",
                table: "mst_car",
                column: "assigned_driver_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_location_distance_from_location_id_to_location_id",
                table: "mst_location_distance",
                columns: new[] { "from_location_id", "to_location_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_location_distance_to_location_id",
                table: "mst_location_distance",
                column: "to_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_detail_booking_id",
                table: "trn_booking_detail",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_detail_driver_id",
                table: "trn_booking_detail",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_detail_from_location_id",
                table: "trn_booking_detail",
                column: "from_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_detail_to_location_id",
                table: "trn_booking_detail",
                column: "to_location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mst_car_mst_driver_assigned_driver_id",
                table: "mst_car",
                column: "assigned_driver_id",
                principalTable: "mst_driver",
                principalColumn: "driver_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mst_car_mst_driver_assigned_driver_id",
                table: "mst_car");

            migrationBuilder.DropTable(
                name: "mst_location_distance");

            migrationBuilder.DropTable(
                name: "trn_booking_detail");

            migrationBuilder.DropTable(
                name: "mst_driver");

            migrationBuilder.DropIndex(
                name: "IX_mst_car_assigned_driver_id",
                table: "mst_car");

            migrationBuilder.DropColumn(
                name: "assigned_driver_id",
                table: "mst_car");
        }
    }
}
