using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class SplitFileContentIntoOwnEntity : Migration
    {
        // Deliberately empty. File.FileData/FileDataSearch became a separate FileContent entity
        // mapped onto the same AppFile columns (EF table splitting), so the database is
        // unchanged - this migration only records the new model shape in the snapshot. See
        // FileContent for why.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
