using SessionLogger.Domain.Methods;
using SessionLogger.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Methods;

// Method is a shared, global list - anyone can view/select it, only admins may add to or change it
public class MethodAppService :
    CrudAppService<
        Method,
        MethodDto,
        int,
        PagedAndSortedResultRequestDto,
        MethodUpdateDto>,
    IMethodAppService
{
    public MethodAppService(IRepository<Method, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }
}
