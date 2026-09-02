using System.Linq;
using System.Threading.Tasks;
using SessionLogger.Domain.UserPreferences;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.UserPreferences;

public class UserPreferenceAppService : SessionLoggerAppService, IUserPreferenceAppService
{
    private readonly IRepository<UserPreference, System.Guid> _repository;

    public UserPreferenceAppService(IRepository<UserPreference, System.Guid> repository)
    {
        _repository = repository;
    }

    public async Task<UserPreferenceDto> GetAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var entity = query.FirstOrDefault(x => x.Id == CurrentUser.Id);

        return new UserPreferenceDto
        {
            PersonalBestMetric = entity?.PersonalBestMetric ?? PersonalBestMetric.Weight
        };
    }

    public async Task<UserPreferenceDto> UpdateAsync(UpdateUserPreferenceDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var entity = query.FirstOrDefault(x => x.Id == CurrentUser.Id);

        if (entity == null)
        {
            entity = new UserPreference(CurrentUser.Id!.Value)
            {
                PersonalBestMetric = input.PersonalBestMetric
            };
            await _repository.InsertAsync(entity);
        }
        else
        {
            entity.PersonalBestMetric = input.PersonalBestMetric;
            await _repository.UpdateAsync(entity);
        }

        return new UserPreferenceDto { PersonalBestMetric = entity.PersonalBestMetric };
    }
}
