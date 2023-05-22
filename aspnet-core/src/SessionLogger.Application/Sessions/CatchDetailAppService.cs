using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Sessions;

public class CatchDetailAppService :
    CrudAppService<
        CatchDetail,
        CatchDetailDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCatchDetailDto>,
    ICatchDetailAppService
{
    public CatchDetailAppService(IRepository<CatchDetail, int> repository)
        : base(repository)
    {

    }
}
