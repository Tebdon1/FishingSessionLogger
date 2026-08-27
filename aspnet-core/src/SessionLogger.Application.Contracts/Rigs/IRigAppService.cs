using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Rigs;

public interface IRigAppService :
    ICrudAppService<
        RigDto,
        int,
        PagedAndSortedResultRequestDto,
        RigUpdateDto>
{
}
