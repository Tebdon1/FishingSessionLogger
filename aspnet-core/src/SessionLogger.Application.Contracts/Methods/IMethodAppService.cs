using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Methods;

public interface IMethodAppService :
    ICrudAppService<
        MethodDto,
        int,
        PagedAndSortedResultRequestDto,
        MethodUpdateDto>
{
}
