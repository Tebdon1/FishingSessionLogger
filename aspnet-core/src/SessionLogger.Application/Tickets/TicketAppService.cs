using SessionLogger.Domain.Tickets;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Tickets;

// Tickets are personal: each user only sees and manages their own.
public class TicketAppService :
    CrudAppService<
        Ticket,
        TicketDto,
        int,
        PagedAndSortedResultRequestDto,
        TicketUpdateDto>,
    ITicketAppService
{
    private const int MaxTicketsPerUser = 50;

    public TicketAppService(IRepository<Ticket, int> repository)
        : base(repository)
    {
    }

    protected override async Task<IQueryable<Ticket>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        return query.Where(x => x.CreatorId == CurrentUser.Id);
    }

    protected override async Task<Ticket> GetEntityByIdAsync(int id)
    {
        var entity = await Repository.GetAsync(id);
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new EntityNotFoundException(typeof(Ticket), id);
        }

        return entity;
    }

    public override async Task<TicketDto> CreateAsync(TicketUpdateDto input)
    {
        var query = await Repository.GetQueryableAsync();
        var ownedCount = query.Count(x => x.CreatorId == CurrentUser.Id);
        if (ownedCount >= MaxTicketsPerUser)
        {
            throw new UserFriendlyException($"You've reached the limit of {MaxTicketsPerUser} tickets.");
        }

        return await base.CreateAsync(input);
    }
}
