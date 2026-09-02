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
        var species = query.FirstOrDefault(x => x.Id == id);

        if (species?.PhotoData == null)
        {
            throw new EntityNotFoundException(typeof(Species), id);
        }

        return new RemoteStreamContent(new MemoryStream(species.PhotoData), species.PhotoFileName, GetContentType(species.PhotoExtension));
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
