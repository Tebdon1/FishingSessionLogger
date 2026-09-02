using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSpeciesIsSaltwaterToWaterType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WaterType(0)=Freshwater, WaterType(1)=Saltwater - added and populated from
            // the old bool before dropping it, so existing rows keep their classification
            // instead of collapsing to the new column's Freshwater default.
            migrationBuilder.AddColumn<int>(
                name: "WaterType",
                table: "AppSpecies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE AppSpecies SET WaterType = 1 WHERE IsSaltwater = 1;");

            migrationBuilder.DropColumn(
                name: "IsSaltwater",
                table: "AppSpecies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSaltwater",
                table: "AppSpecies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // WaterType(2)=Both has no bool equivalent - treated as saltwater on rollback
            // since that's the more restrictive/safer classification to lose silently.
            migrationBuilder.Sql(
                "UPDATE AppSpecies SET IsSaltwater = 1 WHERE WaterType IN (1, 2);");

            migrationBuilder.DropColumn(
                name: "WaterType",
                table: "AppSpecies");
        }
    }
}
