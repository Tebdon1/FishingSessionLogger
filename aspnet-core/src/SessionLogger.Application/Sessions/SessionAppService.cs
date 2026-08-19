using Microsoft.EntityFrameworkCore;
using SessionLogger.Domain.Sessions;
using SessionLogger.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Sessions;

// Sessions are personal: each user only sees and manages their own.
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
        GetPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        GetListPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        CreatePolicyName = SessionLoggerPermissions.SessionConfig.Create;
        UpdatePolicyName = SessionLoggerPermissions.SessionConfig.Edit;
        DeletePolicyName = SessionLoggerPermissions.SessionConfig.Delete;
    }

    // The paged list doesn't use includeDetails, but the grid still needs the venue name
    protected override async Task<IQueryable<Session>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        return query.Include(x => x.Venue).Where(x => x.CreatorId == CurrentUser.Id);
    }

    protected override async Task<Session> GetEntityByIdAsync(int id)
    {
        var entity = await Repository.GetAsync(id);
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new EntityNotFoundException(typeof(Session), id);
        }

        return entity;
    }
}
