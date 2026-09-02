using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
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

    public override async Task<SpeciesDto> CreateAsync(SpeciesUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name);
        return await base.CreateAsync(input);
    }

    public override async Task<SpeciesDto> UpdateAsync(int id, SpeciesUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name, id);
        return await base.UpdateAsync(id, input);
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
    private static void ApplyPhoto(SpeciesUpdateDto input, Species entity)
    {
        if (input.PhotoData != null)
        {
            entity.PhotoData = input.PhotoData;
            entity.PhotoFileName = input.PhotoFileName;
            entity.PhotoExtension = Path.GetExtension(input.PhotoFileName);
        }
        else if (input.RemovePhoto)
        {
            entity.PhotoData = null;
            entity.PhotoFileName = null;
            entity.PhotoExtension = null;
        }
    }
}
