using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Venues;

public interface IVenueAppService :
    ICrudAppService<
        VenueDto,
        int,
        PagedAndSortedResultRequestDto,
        VenueUpdateDto>
{
}
