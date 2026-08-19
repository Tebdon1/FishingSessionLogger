using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Tickets;

public interface ITicketAppService :
    ICrudAppService<
        TicketDto,
        int,
        PagedAndSortedResultRequestDto,
        TicketUpdateDto>
{
}
