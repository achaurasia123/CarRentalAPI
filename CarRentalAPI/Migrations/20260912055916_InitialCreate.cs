using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mst_car_type",
                columns: table => new
                {
                    car_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_car_type", x => x.car_type_id);
                });

            migrationBuilder.CreateTable(
                name: "mst_location",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    city_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_location", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "mst_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "mst_car",
                columns: table => new
                {
                    car_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    car_type_id = table.Column<int>(type: "int", nullable: false),
                    price_per_day = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    seats = table.Column<int>(type: "int", nullable: false),
                    transmission = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fuel_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    rating = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_available = table.Column<bool>(type: "bit", nullable: false),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_car", x => x.car_id);
                    table.ForeignKey(
                        name: "FK_mst_car_mst_car_type_car_type_id",
                        column: x => x.car_type_id,
                        principalTable: "mst_car_type",
                        principalColumn: "car_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mst_car_mst_location_location_id",
                        column: x => x.location_id,
                        principalTable: "mst_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mst_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mst_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_mst_user_mst_role_role_id",
                        column: x => x.role_id,
                        principalTable: "mst_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_refresh_token",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_revoked = table.Column<bool>(type: "bit", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_refresh_token", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_tbl_refresh_token_mst_user_user_id",
                        column: x => x.user_id,
                        principalTable: "mst_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trn_booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    car_id = table.Column<int>(type: "int", nullable: false),
                    from_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    to_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trn_booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_trn_booking_mst_car_car_id",
                        column: x => x.car_id,
                        principalTable: "mst_car",
                        principalColumn: "car_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trn_booking_mst_user_user_id",
                        column: x => x.user_id,
                        principalTable: "mst_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "mst_car_type",
                columns: new[] { "car_type_id", "type_name" },
                values: new object[,]
                {
                    { 1, "Sedan" },
                    { 2, "SUV" },
                    { 3, "Hatchback" },
                    { 4, "Luxury" },
                    { 5, "Electric" }
                });

            migrationBuilder.InsertData(
                table: "mst_location",
                columns: new[] { "location_id", "city_name" },
                values: new object[,]
                {
                    { 1, "Bhopal" },
                    { 2, "Indore" },
                    { 3, "Delhi" },
                    { 4, "Mumbai" }
                });

            migrationBuilder.InsertData(
                table: "mst_role",
                columns: new[] { "role_id", "created_date", "role_name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Customer" }
                });

            migrationBuilder.InsertData(
                table: "mst_car",
                columns: new[] { "car_id", "brand", "car_name", "car_type_id", "created_date", "fuel_type", "image_url", "is_available", "location_id", "price_per_day", "rating", "seats", "transmission" },
                values: new object[,]
                {
                    { 1, "Maruti Suzuki", "Swift Dzire", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Petrol", "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?auto=format&fit=crop&w=600&q=80", true, 1, 1800m, 4.4m, 5, "Manual" },
                    { 2, "Hyundai", "Creta", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diesel", "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=600&q=80", true, 2, 3200m, 4.7m, 5, "Automatic" },
                    { 3, "Tesla", "Model 3", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Electric", "https://images.unsplash.com/photo-1560958089-b8a1929cea89?auto=format&fit=crop&w=600&q=80", false, 3, 6500m, 4.9m, 5, "Automatic" },
                    { 4, "Honda", "City", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Petrol", "https://images.unsplash.com/photo-1622194993926-2f30edb5f9a7?auto=format&fit=crop&w=600&q=80", true, 1, 2600m, 4.5m, 5, "Automatic" },
                    { 5, "Toyota", "Fortuner", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diesel", "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=600&q=80", true, 4, 5800m, 4.8m, 7, "Automatic" },
                    { 6, "Maruti Suzuki", "Baleno", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Petrol", "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?auto=format&fit=crop&w=600&q=80", true, 2, 1500m, 4.2m, 5, "Manual" },
                    { 7, "BMW", "BMW 5 Series", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Petrol", "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80", true, 3, 9000m, 4.9m, 5, "Automatic" },
                    { 8, "Tata", "Nexon EV", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Electric", "https://images.unsplash.com/photo-1617469767053-d3b523a0b982?auto=format&fit=crop&w=600&q=80", true, 1, 2900m, 4.3m, 5, "Automatic" }
                });

            migrationBuilder.InsertData(
                table: "mst_user",
                columns: new[] { "user_id", "created_date", "email", "full_name", "is_active", "password_hash", "role_id" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "demo@driveon.com", "Demo User", true, "$2b$11$aYdgWAKTu6uNfv1xmRe6Lubhkti5R4pil8Q7l6M86Li80PtL5j8hq", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_mst_car_car_type_id",
                table: "mst_car",
                column: "car_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_car_location_id",
                table: "mst_car",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_mst_user_email",
                table: "mst_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mst_user_role_id",
                table: "mst_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_refresh_token_user_id",
                table: "tbl_refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_booking_no",
                table: "trn_booking",
                column: "booking_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_car_id",
                table: "trn_booking",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_booking_user_id",
                table: "trn_booking",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_refresh_token");

            migrationBuilder.DropTable(
                name: "trn_booking");

            migrationBuilder.DropTable(
                name: "mst_car");

            migrationBuilder.DropTable(
                name: "mst_user");

            migrationBuilder.DropTable(
                name: "mst_car_type");

            migrationBuilder.DropTable(
                name: "mst_location");

            migrationBuilder.DropTable(
                name: "mst_role");
        }
    }
}
