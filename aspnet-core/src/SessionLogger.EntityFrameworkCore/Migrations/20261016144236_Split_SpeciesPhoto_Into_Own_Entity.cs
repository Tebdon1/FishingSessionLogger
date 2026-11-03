using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SessionLogger.Migrations
{
    /// <inheritdoc />
    public partial class SplitSpeciesPhotoIntoOwnEntity : Migration
    {
        // Deliberately empty. Species.PhotoData/PhotoFileName/PhotoExtension became a separate
        // SpeciesPhoto entity mapped onto the same AppSpecies columns (EF table splitting), so
        // the database is unchanged - this migration only records the new model shape in the
        // snapshot. See SpeciesPhoto for why.

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
