using System.Threading.Tasks;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Rigs;
using SessionLogger.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Rigs;

// Rig is a shared, global list - anyone can view/select it, only admins may add to or change it
public class RigAppService :
    CrudAppService<
        Rig,
        RigDto,
        int,
        PagedAndSortedResultRequestDto,
        RigUpdateDto>,
    IRigAppService
{
    public RigAppService(IRepository<Rig, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }

    protected override async Task<Rig> MapToEntityAsync(RigUpdateDto createInput)
    {
        var entity = await base.MapToEntityAsync(createInput);
        entity.LengthMm = ToMillimetres(createInput);
        // Kept purely so the length can be displayed the way it was entered, not
        // just silently reformatted to mm - see Rig.LengthUnit.
        entity.LengthUnit = entity.LengthMm != null ? createInput.LengthUnit : null;
        entity.HookWeightG = ToGrams(createInput);
        entity.HookWeightUnit = entity.HookWeightG != null ? createInput.HookWeightUnit : null;
        return entity;
    }

    protected override async Task MapToEntityAsync(RigUpdateDto updateInput, Rig entity)
    {
        await base.MapToEntityAsync(updateInput, entity);
        entity.LengthMm = ToMillimetres(updateInput);
        entity.LengthUnit = entity.LengthMm != null ? updateInput.LengthUnit : null;
        entity.HookWeightG = ToGrams(updateInput);
        entity.HookWeightUnit = entity.HookWeightG != null ? updateInput.HookWeightUnit : null;
    }

    // Converted once, at write time, to a single canonical unit - so every future
    // comparison/aggregation is a plain numeric one, with no unit-conversion logic
    // needed at query time (same approach as BaitAppService.ToMillimetres).
    private static decimal? ToMillimetres(RigUpdateDto input)
    {
        if (input.LengthValue == null)
        {
            return null;
        }

        return input.LengthUnit switch
        {
            SizeUnit.Centimetres => input.LengthValue * 10m,
            SizeUnit.Inches => input.LengthValue * 25.4m,
            _ => input.LengthValue
        };
    }

    // Converted once, at write time, to grams - so every future comparison/aggregation
    // is a plain numeric one, with no unit-conversion logic needed at query time.
    private static decimal? ToGrams(RigUpdateDto input)
    {
        if (input.HookWeightValue == null)
        {
            return null;
        }

        return input.HookWeightUnit switch
        {
            HookWeightUnit.Ounces => input.HookWeightValue * 28.349523125m,
            _ => input.HookWeightValue
        };
    }
}
