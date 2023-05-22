using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Sessions;

[AllowAnonymous]
public class SessionAppService :
    CrudAppService<
        Session,
        SessionDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateSessionDto>,
    ISessionAppService
{
    public SessionAppService(IRepository<Session, int>repository)
        : base(repository)
    {

    }
}
