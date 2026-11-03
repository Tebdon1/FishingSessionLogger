using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.SpeciesTypes;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.SpeciesTypes;

// Serves the raw bytes of a species' default photo for use in an <img> tag. No
// ownership check, unlike CatchPhotoAppService - species (and their photos) are a
// shared, global list visible to every user, same as SpeciesAppService itself.
public class SpeciesPhotoAppService : SessionLoggerAppService
{
    private readonly IRepository<Species, int> _speciesRepository;

    public SpeciesPhotoAppService(IRepository<Species, int> speciesRepository)
    {
        _speciesRepository = speciesRepository;
    }

    public async Task<IRemoteStreamContent> GetAsync(int id)
    {
        var query = await _speciesRepository.GetQueryableAsync();
        // Projects just the photo columns - this is the one place the image is meant to be read.
        var photo = query
            .Where(x => x.Id == id && x.Photo!.Data != null)
            .Select(x => new { x.Photo!.Data, x.Photo.FileName, x.Photo.Extension })
            .FirstOrDefault();

        if (photo?.Data == null)
        {
            throw new EntityNotFoundException(typeof(Species), id);
        }

        return new RemoteStreamContent(new MemoryStream(photo.Data), photo.FileName, GetContentType(photo.Extension));
    }

    private static string GetContentType(string extension) => extension?.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".heic" => "image/heic",
        _ => "application/octet-stream"
    };
}
