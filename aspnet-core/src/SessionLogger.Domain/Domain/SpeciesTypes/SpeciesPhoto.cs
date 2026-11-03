namespace SessionLogger.Domain.SpeciesTypes;

// A species' default tile image, split off from Species so that loading a species doesn't
// load the image with it.
//
// Mapped onto the same AppSpecies row (EF "table splitting" - see SessionLoggerDbContext),
// so there's no separate table to keep in step and no data to move; what changes is only
// what a query selects. The image is a varbinary(max), and any query that Includes Species
// (to read a name, say) used to select it too - repeated once per row in a join, so listing
// ~200 sessions made SQL Server read the same few species photos hundreds of times over.
// Now it's only read by a query that asks for Photo, or projects from it, explicitly.
//
// Photo is null both when the species has no photo and when the navigation wasn't loaded -
// so to ask "does it have one" without loading it, query on Photo.Data != null (which is
// evaluated in SQL) rather than checking the navigation on an entity you loaded.
public class SpeciesPhoto
{
    // Same value as the owning Species.Id - the shared primary key.
    public int SpeciesId { get; set; }

    public byte[]? Data { get; set; }

    public string? FileName { get; set; }

    public string? Extension { get; set; }
}
