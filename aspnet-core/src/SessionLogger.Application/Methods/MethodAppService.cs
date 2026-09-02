using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.Methods;
using SessionLogger.Permissions;
using Volo.Abp;
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

    public override async Task<MethodDto> CreateAsync(MethodUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name);
        return await base.CreateAsync(input);
    }

    public override async Task<MethodDto> UpdateAsync(int id, MethodUpdateDto input)
    {
        await ValidateUniqueNameAsync(input.Name, id);
        return await base.UpdateAsync(id, input);
    }

    // Name also has a unique index at the DB level (see SessionLoggerDbContext), but
    // relying on that alone means a duplicate surfaces as a raw constraint-violation
    // exception - an opaque 500 to the user instead of a message saying what's wrong.
    private async Task ValidateUniqueNameAsync(string name, int? excludingId = null)
    {
        var query = await Repository.GetQueryableAsync();
        var nameTaken = excludingId.HasValue
            ? query.Any(x => x.Name == name && x.Id != excludingId.Value)
            : query.Any(x => x.Name == name);

        if (nameTaken)
        {
            throw new UserFriendlyException($"A method named \"{name}\" already exists.");
        }
    }
}
