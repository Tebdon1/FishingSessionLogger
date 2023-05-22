using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SessionLogger.Sessions;

public interface ICatchWeightAppService :
    ICrudAppService<
        CatchWeightDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCatchWeightDto>
{

}
