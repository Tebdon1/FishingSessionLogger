using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using System.Threading.Tasks;

namespace SessionLogger.SpeciesTypes;

public interface ISpeciesAppService :
    ICrudAppService<
        SpeciesDto,
        int,
        PagedAndSortedResultRequestDto,
        SpeciesUpdateDto>
{
    Task<PagedResultDto<SpeciesDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}
