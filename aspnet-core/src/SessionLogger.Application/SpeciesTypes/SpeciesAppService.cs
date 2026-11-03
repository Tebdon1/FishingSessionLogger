using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.SpeciesTypes;

// Species is a shared, global list - anyone can view/select it, only admins may add to or change it
public class SpeciesAppService :
    CrudAppService<
        Species,
        SpeciesDto,
        int,
        PagedAndSortedResultRequestDto,
        SpeciesUpdateDto>,
    ISpeciesAppService
{
    public SpeciesAppService(IRepository<Species, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }

    public override async Task<SpeciesDto> GetAsync(int id)
    {
        var dto = await base.GetAsync(id);
        await ApplyHasPhotoAsync(dto);
        return dto;
    }

    public override async Task<PagedResultDto<SpeciesDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var result = await base.GetListAsync(input);
        await ApplyHasPhotoAsync(result.Items.ToArray());
        return result;
    }

    public override async Task<SpeciesDto> CreateAsync(SpeciesUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name);
        var dto = await base.CreateAsync(input);
        await ApplyHasPhotoAsync(dto);
        return dto;
    }

    public override async Task<SpeciesDto> UpdateAsync(int id, SpeciesUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name, id);
        var dto = await base.UpdateAsync(id, input);
        await ApplyHasPhotoAsync(dto);
        return dto;
    }

    // Only the entity being edited needs its photo loaded - replacing or clearing it
    // (ApplyPhoto) has to see what's there. Ordinary reads leave it out; see SpeciesPhoto.
    protected override async Task<Species> GetEntityByIdAsync(int id)
    {
        var query = await Repository.WithDetailsAsync(x => x.Photo);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        return entity ?? throw new EntityNotFoundException(typeof(Species), id);
    }

    // HasPhoto is worked out in SQL (Photo.Data IS NOT NULL) so the image itself never
    // has to be read just to find out whether there is one.
    private async Task ApplyHasPhotoAsync(params SpeciesDto[] dtos)
    {
        var ids = dtos.Select(d => d.Id).ToList();
        var query = await Repository.GetQueryableAsync();
        var withPhoto = (await AsyncExecuter.ToListAsync(
                query.Where(x => ids.Contains(x.Id) && x.Photo!.Data != null).Select(x => x.Id)))
            .ToHashSet();

        foreach (var dto in dtos)
        {
            dto.HasPhoto = withPhoto.Contains(dto.Id);
        }
    }

    // Name also has a unique index at the DB level (see SessionLoggerDbContext), but
    // relying on that alone means a duplicate surfaces as a raw constraint-violation
    // exception - an opaque 500 to the user instead of a message saying what's wrong.
    private async Task ValidateUniqueNameAsync(string name, int? excludingId = null)
    {
        var query = await Repository.GetQueryableAsync();
        var nameTaken = excludingId.HasValue
            ? query.Any(x => x.Name == name && x.Id != excludingId.Value)
            : query.Any(x => x.Name == name);

        if (nameTaken)
        {
            throw new UserFriendlyException($"A species named \"{name}\" already exists.");
        }
    }

    protected override async Task<Species> MapToEntityAsync(SpeciesUpdateDto createInput)
    {
        var entity = await base.MapToEntityAsync(createInput);
        ApplyPhoto(createInput, entity);
        return entity;
    }

    protected override async Task MapToEntityAsync(SpeciesUpdateDto updateInput, Species entity)
    {
        await base.MapToEntityAsync(updateInput, entity);
        ApplyPhoto(updateInput, entity);
    }

    // A new PhotoData always replaces whatever's there (deliberately - a re-upload for
    // the same species is meant to become the new default, not sit alongside the old
    // one); RemovePhoto clears it when no new photo is supplied; omitting both leaves
    // the existing photo untouched (e.g. editing just the name).
    //
    // The existing SpeciesPhoto (loaded by GetEntityByIdAsync) is edited in place rather
    // than swapped for a new instance - same key, so EF would reject a second instance
    // as a duplicate of the one it's already tracking.
    private static void ApplyPhoto(SpeciesUpdateDto input, Species entity)
    {
        if (input.PhotoData != null)
        {
            var photo = entity.Photo ??= new SpeciesPhoto();
            photo.Data = input.PhotoData;
            photo.FileName = input.PhotoFileName;
            photo.Extension = Path.GetExtension(input.PhotoFileName);
        }
        else if (input.RemovePhoto && entity.Photo != null)
        {
            entity.Photo.Data = null;
            entity.Photo.FileName = null;
            entity.Photo.Extension = null;
        }
    }
}
