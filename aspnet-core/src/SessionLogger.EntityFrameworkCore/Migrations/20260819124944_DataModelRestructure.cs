using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class DataModelRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppVenue_AppTicket_TicketId",
                table: "AppVenue");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "AppSession");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "AppSession");

            migrationBuilder.DropColumn(
                name: "Bait",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "Species",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "AppCatch");

            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "AppVenue",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppSpecies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AppSession",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VenueId",
                table: "AppSession",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<float>(
                name: "Weight",
                table: "AppCatch",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<int>(
                name: "BaitId",
                table: "AppCatch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpeciesId",
                table: "AppCatch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppBait",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AppFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatchId = table.Column<int>(type: "int", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileDataSearch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFile_AppCatch_CatchId",
                        column: x => x.CatchId,
                        principalTable: "AppCatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSession_VenueId",
                table: "AppSession",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCatch_BaitId",
                table: "AppCatch",
                column: "BaitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCatch_SpeciesId",
                table: "AppCatch",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBait_Name",
                table: "AppBait",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppFile_CatchId",
                table: "AppFile",
                column: "CatchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCatch_AppBait_BaitId",
                table: "AppCatch",
                column: "BaitId",
                principalTable: "AppBait",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppCatch_AppSpecies_SpeciesId",
                table: "AppCatch",
                column: "SpeciesId",
                principalTable: "AppSpecies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppSession_AppVenue_VenueId",
                table: "AppSession",
                column: "VenueId",
                principalTable: "AppVenue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppVenue_AppTicket_TicketId",
                table: "AppVenue",
                column: "TicketId",
                principalTable: "AppTicket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCatch_AppBait_BaitId",
                table: "AppCatch");

            migrationBuilder.DropForeignKey(
                name: "FK_AppCatch_AppSpecies_SpeciesId",
                table: "AppCatch");

            migrationBuilder.DropForeignKey(
                name: "FK_AppSession_AppVenue_VenueId",
                table: "AppSession");

            migrationBuilder.DropForeignKey(
                name: "FK_AppVenue_AppTicket_TicketId",
                table: "AppVenue");

            migrationBuilder.DropTable(
                name: "AppFile");

            migrationBuilder.DropIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies");

            migrationBuilder.DropIndex(
                name: "IX_AppSession_VenueId",
                table: "AppSession");

            migrationBuilder.DropIndex(
                name: "IX_AppCatch_BaitId",
                table: "AppCatch");

            migrationBuilder.DropIndex(
                name: "IX_AppCatch_SpeciesId",
                table: "AppCatch");

            migrationBuilder.DropIndex(
                name: "IX_AppBait_Name",
                table: "AppBait");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AppSession");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "AppSession");

            migrationBuilder.DropColumn(
                name: "BaitId",
                table: "AppCatch");

            migrationBuilder.DropColumn(
                name: "SpeciesId",
                table: "AppCatch");

            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "AppVenue",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppSpecies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<float>(
                name: "Duration",
                table: "AppSession",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "AppSession",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<float>(
                name: "Weight",
                table: "AppCatch",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bait",
                table: "AppCatch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Species",
                table: "AppCatch",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "AppCatch",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppBait",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_AppVenue_AppTicket_TicketId",
                table: "AppVenue",
                column: "TicketId",
                principalTable: "AppTicket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
