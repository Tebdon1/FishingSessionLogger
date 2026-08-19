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

    public override async Task<VenueDto> CreateAsync(VenueUpdateDto input)
    {
        var query = await Repository.GetQueryableAsync();
        var ownedCount = query.Count(x => x.CreatorId == CurrentUser.Id);
        if (ownedCount >= MaxVenuesPerUser)
        {
            throw new UserFriendlyException($"You've reached the limit of {MaxVenuesPerUser} venues.");
        }

        return await base.CreateAsync(input);
    }
}
