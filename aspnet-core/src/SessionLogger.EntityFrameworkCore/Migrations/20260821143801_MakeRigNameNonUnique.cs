using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class MakeRigNameNonUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRig_Name",
                table: "AppRig");

            migrationBuilder.CreateIndex(
                name: "IX_AppRig_Name",
                table: "AppRig",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRig_Name",
                table: "AppRig");

            migrationBuilder.CreateIndex(
                name: "IX_AppRig_Name",
                table: "AppRig",
                column: "Name",
                unique: true);
        }
    }
}
