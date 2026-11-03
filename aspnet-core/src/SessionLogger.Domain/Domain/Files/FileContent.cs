namespace SessionLogger.Domain.Files
{
    // The bytes (and extracted text) of a File, split off from the File's own details so
    // that loading a File - or anything that Includes it, like a catch's Photo - doesn't
    // load the image too.
    //
    // Mapped onto the same AppFile row (EF "table splitting" - see SessionLoggerDbContext),
    // so there's no second table to keep in step; what changes is only what a query
    // selects. Without this, anything that included a catch's photo just to read its Id or
    // file name (opening a session to edit it, the personal-best calculation) read every
    // photo in full, which grows with every photo saved. Content is only loaded by a query
    // that asks for it - Include(x => x.Content), or a projection that reads from it.
    //
    // Content is null on a File that was loaded without it, so it can't tell you whether a
    // file has content (every file does) - only whether it was loaded.
    public class FileContent
    {
        // Same value as the owning File.Id - the shared primary key.
        public int FileId { get; set; }

        public byte[] Data { get; set; }

        // Extracted text content, used by the generic full-text search infrastructure
        public string Search { get; set; }
    }
}
