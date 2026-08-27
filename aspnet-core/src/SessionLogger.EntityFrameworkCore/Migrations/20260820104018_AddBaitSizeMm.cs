using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddBaitSizeMm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "AppBait");

            migrationBuilder.AddColumn<decimal>(
                name: "SizeMm",
                table: "AppBait",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeMm",
                table: "AppBait");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "AppBait",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
