using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies");

            migrationBuilder.DropIndex(
                name: "IX_AppMethod_Name",
                table: "AppMethod");

            migrationBuilder.DropIndex(
                name: "IX_AppBait_Name",
                table: "AppBait");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppVenue",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppVenue",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppVenue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppSpecies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppSpecies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppSpecies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppRig",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppRig",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppRig",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppMethod",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppMethod",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppMethod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppBait",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppBait",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppBait",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AppMethod_Name",
                table: "AppMethod",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AppBait_Name",
                table: "AppBait",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies");

            migrationBuilder.DropIndex(
                name: "IX_AppMethod_Name",
                table: "AppMethod");

            migrationBuilder.DropIndex(
                name: "IX_AppBait_Name",
                table: "AppBait");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppVenue");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppVenue");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppVenue");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppSpecies");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppSpecies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppSpecies");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppRig");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppRig");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppRig");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppMethod");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppMethod");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppMethod");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppBait");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppBait");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppBait");

            migrationBuilder.CreateIndex(
                name: "IX_AppSpecies_Name",
                table: "AppSpecies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMethod_Name",
                table: "AppMethod",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBait_Name",
                table: "AppBait",
                column: "Name",
                unique: true);
        }
    }
}
