using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Sessions;

public class CatchWeightAppService :
    CrudAppService<
        CatchWeight,
        CatchWeightDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCatchWeightDto>,
    ICatchWeightAppService
{
    public CatchWeightAppService(IRepository<CatchWeight, int> repository)
        : base(repository)
    {

    }
}
