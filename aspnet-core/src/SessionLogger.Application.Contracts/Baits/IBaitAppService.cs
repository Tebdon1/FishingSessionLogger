using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Baits;

public interface IBaitAppService :
    ICrudAppService<
        BaitDto,
        int,
        PagedAndSortedResultRequestDto,
        BaitUpdateDto>
{

}
