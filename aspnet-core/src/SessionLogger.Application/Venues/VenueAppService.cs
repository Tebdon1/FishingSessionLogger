using Microsoft.EntityFrameworkCore;
using SessionLogger.Domain.Venues;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Venues;

// Venues are personal: each user only sees and manages their own.
public class VenueAppService :
    CrudAppService<
        Venue,
        VenueDto,
        int,
        PagedAndSortedResultRequestDto,
        VenueUpdateDto>,
    IVenueAppService
{
    private const int MaxVenuesPerUser = 50;

    public VenueAppService(IRepository<Venue, int> repository)
        : base(repository)
    {
    }

    protected override async Task<IQueryable<Venue>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        return query.Include(x => x.Ticket).Where(x => x.CreatorId == CurrentUser.Id);
    }

    protected override async Task<Venue> GetEntityByIdAsync(int id)
    {
        var entity = await Repository.GetAsync(id);
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new EntityNotFoundException(typeof(Venue), id);
        }

        return entity;
    }

    public override async Task<VenueDto> GetAsync(int id)
    {
        var dto = await base.GetAsync(id);
        await ApplyDisplayNamesAsync(dto);
        return dto;
    }

    public override async Task<PagedResultDto<VenueDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var result = await base.GetListAsync(input);
        await ApplyDisplayNamesAsync(result.Items.ToArray());
        return result;
    }

    public override async Task<VenueDto> CreateAsync(VenueUpdateDto input)
    {
        var query = await Repository.GetQueryableAsync();
        var ownedCount = query.Count(x => x.CreatorId == CurrentUser.Id);
        if (ownedCount >= MaxVenuesPerUser)
        {
            throw new UserFriendlyException($"You've reached the limit of {MaxVenuesPerUser} venues.");
        }

        await EnsureNotDuplicateAsync(input, excludeId: null);

        var dto = await base.CreateAsync(input);
        await ApplyDisplayNamesAsync(dto);
        return dto;
    }

    public override async Task<VenueDto> UpdateAsync(int id, VenueUpdateDto input)
    {
        await EnsureNotDuplicateAsync(input, excludeId: id);

        var dto = await base.UpdateAsync(id, input);
        await ApplyDisplayNamesAsync(dto);
        return dto;
    }

    // The same name on a different ticket is fine (see VenueLabels) - what's rejected is
    // the same name on the same ticket (or both with none), which would be two venues
    // nobody could tell apart.
    private async Task EnsureNotDuplicateAsync(VenueUpdateDto input, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return;
        }

        var query = await Repository.GetQueryableAsync();
        var mine = await AsyncExecuter.ToListAsync(query.Where(x => x.CreatorId == CurrentUser.Id));

        var name = VenueLabels.NormalizeName(input.Name);
        var duplicate = mine.Any(x =>
            x.Id != excludeId
            && VenueLabels.NormalizeName(x.Name) == name
            && x.TicketId == input.TicketId);

        if (duplicate)
        {
            throw new UserFriendlyException(
                $"You already have a venue called \"{input.Name.Trim()}\" on the same ticket. " +
                "If it's a different stretch, give it a distinct name; if a different ticket controls it, choose that ticket.");
        }
    }

    // Labelled from all of the user's venues, not just the ones being returned, so a
    // venue's label doesn't change with the page it happens to be on.
    private async Task ApplyDisplayNamesAsync(params VenueDto[] dtos)
    {
        var query = await Repository.GetQueryableAsync();
        var mine = await AsyncExecuter.ToListAsync(
            query.Include(x => x.Ticket).Where(x => x.CreatorId == CurrentUser.Id));
        var labels = VenueLabels.From(mine);

        foreach (var dto in dtos)
        {
            dto.DisplayName = labels.For(dto.Id, dto.Name);
        }
    }
}
