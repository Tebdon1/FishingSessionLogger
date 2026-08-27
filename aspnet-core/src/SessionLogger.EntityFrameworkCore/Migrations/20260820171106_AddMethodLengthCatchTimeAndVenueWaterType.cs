using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddMethodLengthCatchTimeAndVenueWaterType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WaterType",
                table: "AppVenue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CatchTime",
                table: "AppCatch",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthMm",
                table: "AppCatch",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MethodId",
                table: "AppCatch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMethod", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCatch_MethodId",
                table: "AppCatch",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMethod_Name",
                table: "AppMethod",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCatch_AppMethod_MethodId",
                table: "AppCatch",
                column: "MethodId",
                principalTable: "AppMethod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCatch_AppMethod_MethodId",
                table: "AppCatch");

            migrationBuilder.DropTable(
                name: "AppMethod");

            migrationBuilder.DropIndex(
                name: "IX_AppCatch_MethodId",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "WaterType",
                table: "AppVenue");

            migrationBuilder.DropColumn(
                name: "CatchTime",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "LengthMm",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "MethodId",
                table: "AppCatch");
        }
    }
}
