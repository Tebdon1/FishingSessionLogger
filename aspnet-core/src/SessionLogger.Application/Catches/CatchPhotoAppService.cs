using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using FileEntity = SessionLogger.Domain.Files.File;

namespace SessionLogger.Catches;

// Serves the raw bytes of a catch photo for use in an <img> tag.
public class CatchPhotoAppService : SessionLoggerAppService
{
    private readonly IRepository<FileEntity, int> _fileRepository;

    public CatchPhotoAppService(IRepository<FileEntity, int> fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<IRemoteStreamContent> GetAsync(int id)
    {
        var query = await _fileRepository.GetQueryableAsync();
        var file = query.FirstOrDefault(x => x.Id == id);

        if (file == null || file.CreatorId != CurrentUser.Id)
        {
            throw new EntityNotFoundException(typeof(FileEntity), id);
        }

        return new RemoteStreamContent(new MemoryStream(file.FileData), file.FileName, GetContentType(file.Extension));
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
