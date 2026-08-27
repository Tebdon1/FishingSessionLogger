using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.SpeciesTypes;

// Species is a shared, global list - anyone can view/select it, only admins may add to or change it
public class SpeciesAppService :
    CrudAppService<
        Species,
        SpeciesDto,
        int,
        PagedAndSortedResultRequestDto,
        SpeciesUpdateDto>,
    ISpeciesAppService
{
    public SpeciesAppService(IRepository<Species, int> repository)
        : base(repository)
    {
        CreatePolicyName = SessionLoggerPermissions.Lookups.Create;
        UpdatePolicyName = SessionLoggerPermissions.Lookups.Edit;
        DeletePolicyName = SessionLoggerPermissions.Lookups.Delete;
    }
}
