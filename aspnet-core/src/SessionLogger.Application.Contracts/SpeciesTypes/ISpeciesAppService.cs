using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.SpeciesTypes;

public interface ISpeciesAppService :
    ICrudAppService<
        SpeciesDto,
        int,
        PagedAndSortedResultRequestDto,
        SpeciesUpdateDto>
{
}
