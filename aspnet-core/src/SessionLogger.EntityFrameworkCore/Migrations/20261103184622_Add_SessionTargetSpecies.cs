using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTargetSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSessionTargetSpecies",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    TargetSpeciesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSessionTargetSpecies", x => new { x.SessionId, x.TargetSpeciesId });
                    table.ForeignKey(
                        name: "FK_AppSessionTargetSpecies_AppSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AppSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSessionTargetSpecies_AppSpecies_TargetSpeciesId",
                        column: x => x.TargetSpeciesId,
                        principalTable: "AppSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSessionTargetSpecies_TargetSpeciesId",
                table: "AppSessionTargetSpecies",
                column: "TargetSpeciesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSessionTargetSpecies");
        }
    }
}
