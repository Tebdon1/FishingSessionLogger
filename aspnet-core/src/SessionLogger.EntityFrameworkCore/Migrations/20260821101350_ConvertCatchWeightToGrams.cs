using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCatchWeightToGrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WeightG",
                table: "AppCatch",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            // Existing Weight values were decimal pounds (per the old "Weight (lbs)"
            // label) - convert to grams using the exact avoirdupois factor before the
            // column is dropped, so existing catches keep their weight.
            migrationBuilder.Sql(
                "UPDATE AppCatch SET WeightG = CAST(Weight AS decimal(18,6)) * 453.59237 WHERE Weight IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "AppCatch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Weight",
                table: "AppCatch",
                type: "real",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE AppCatch SET Weight = CAST(WeightG / 453.59237 AS real) WHERE WeightG IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "WeightG",
                table: "AppCatch");
        }
    }
}
