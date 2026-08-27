using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.Baits;
using SessionLogger.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Baits;

// Bait is a shared, global list - anyone can view/select it, only admins may add to or change it
public class BaitAppService :
    CrudAppService<
        Bait,
        BaitDto,
        int,
        PagedAndSortedResultRequestDto,
        BaitUpdateDto>,
    IBaitAppService
{
    public BaitAppService(IRepository<Bait, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }

    protected override async Task<Bait> MapToEntityAsync(BaitUpdateDto createInput)
    {
        var entity = await base.MapToEntityAsync(createInput);
        entity.SizeMm = ToMillimetres(createInput);
        // Kept purely so the size can be displayed the way it was entered, not just
        // silently reformatted to mm - see Bait.SizeUnit.
        entity.SizeUnit = entity.SizeMm != null ? createInput.SizeUnit : null;
        entity.Name = ComputeName(createInput, entity.SizeMm);
        return entity;
    }

    protected override async Task MapToEntityAsync(BaitUpdateDto updateInput, Bait entity)
    {
        await base.MapToEntityAsync(updateInput, entity);
        entity.SizeMm = ToMillimetres(updateInput);
        entity.SizeUnit = entity.SizeMm != null ? updateInput.SizeUnit : null;
        entity.Name = ComputeName(updateInput, entity.SizeMm);
    }

    // Converted once, at write time, to a single canonical unit - so every future
    // comparison/aggregation is a plain numeric one, with no per-query unit handling.
    private static decimal? ToMillimetres(BaitUpdateDto input)
    {
        if (input.BaitType == BaitType.Natural || input.SizeValue == null)
        {
            return null;
        }

        return input.SizeUnit switch
        {
            SizeUnit.Centimetres => input.SizeValue * 10m,
            SizeUnit.Inches => input.SizeValue * 25.4m,
            _ => input.SizeValue
        };
    }

    // Name is always derived, never taken directly from the client, so that the display
    // label and the structured fields it's built from can never drift out of sync -
    // except for Natural, where there's nothing to build it from.
    private static string ComputeName(BaitUpdateDto input, decimal? sizeMm)
    {
        if (input.BaitType == BaitType.Natural)
        {
            return input.Name?.Trim();
        }

        var colourOrFlavour = input.BaitType == BaitType.Lure ? input.Colour : input.Flavour;
        // sizeMm is only non-null when SizeValue was actually provided, so SizeUnit
        // here always reflects the unit that value was entered in - same "show it the
        // way it was entered" reasoning as Bait.SizeUnit.
        var size = sizeMm.HasValue ? FormatSize(sizeMm.Value, input.SizeUnit) : null;

        var parts = new[] { input.Brand, input.Range, colourOrFlavour, size }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim());

        return string.Join(" ", parts);
    }

    private static string FormatSize(decimal sizeMm, SizeUnit unit)
    {
        var (value, suffix) = unit switch
        {
            SizeUnit.Centimetres => (sizeMm / 10m, "cm"),
            SizeUnit.Inches => (sizeMm / 25.4m, "in"),
            _ => (sizeMm, "mm")
        };

        return $"{value.ToString("0.##")}{suffix}";
    }
}
