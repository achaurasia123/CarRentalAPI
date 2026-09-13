using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nullable-false with an empty-string default so existing rows
            // (Demo User, Admin) don't break - they just have no phone on
            // file until someone fills it in via Manage Customers.
            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "mst_user",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "mst_user");
        }
    }
}
