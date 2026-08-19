using SessionLogger.Domain.Baits;
using SessionLogger.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Baits;

// Bait is a shared, global list - anyone can view/select it, only admins may add to or change it
public class BaitAppService :
    CrudAppService<
        Bait,
        BaitDto,
        int,
        PagedAndSortedResultRequestDto,
        BaitUpdateDto>,
    IBaitAppService
{
    public BaitAppService(IRepository<Bait, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }
}
