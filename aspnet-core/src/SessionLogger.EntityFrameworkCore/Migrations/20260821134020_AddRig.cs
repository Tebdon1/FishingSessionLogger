using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddRig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RigId",
                table: "AppCatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppRig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LengthMm = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    HookSize = table.Column<int>(type: "int", nullable: true),
                    HookPattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Materials = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRig", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCatch_RigId",
                table: "AppCatch",
                column: "RigId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRig_Name",
                table: "AppRig",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCatch_AppRig_RigId",
                table: "AppCatch",
                column: "RigId",
                principalTable: "AppRig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCatch_AppRig_RigId",
                table: "AppCatch");

            migrationBuilder.DropTable(
                name: "AppRig");

            migrationBuilder.DropIndex(
                name: "IX_AppCatch_RigId",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "RigId",
                table: "AppCatch");
        }
    }
}
