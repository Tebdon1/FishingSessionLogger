using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredUnitsToUserPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredLengthUnit",
                table: "AppUserPreference",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PreferredWeightUnit",
                table: "AppUserPreference",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredLengthUnit",
                table: "AppUserPreference");

            migrationBuilder.DropColumn(
                name: "PreferredWeightUnit",
                table: "AppUserPreference");
        }
    }
}
