using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SessionLogger.Domain.Sessions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Sessions;

public class CatchSummaryAppService :
    CrudAppService<
        CatchSummary,
        CatchSummaryDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCatchSummaryDto>,
    ICatchSummaryAppService
{
    public CatchSummaryAppService(IRepository<CatchSummary, int> repository)
        : base(repository)
    {

    }
}
