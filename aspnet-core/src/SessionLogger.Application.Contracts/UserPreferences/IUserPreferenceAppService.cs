using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SessionLogger.UserPreferences;

public interface IUserPreferenceAppService : IApplicationService
{
    Task<UserPreferenceDto> GetAsync();

    Task<UserPreferenceDto> UpdateAsync(UpdateUserPreferenceDto input);
}
