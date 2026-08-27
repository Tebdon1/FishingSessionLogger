using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalEntryUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LengthUnit",
                table: "AppRig",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LengthUnit",
                table: "AppCatch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeightUnit",
                table: "AppCatch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeUnit",
                table: "AppBait",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LengthUnit",
                table: "AppRig");

            migrationBuilder.DropColumn(
                name: "LengthUnit",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "SizeUnit",
                table: "AppBait");
        }
    }
}
