using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SessionLogger.Domain.UserPreferences;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.UserPreferences;

// A preference is meaningless for an anonymous caller, and UpdateAsync's insert path
// needs a real CurrentUser.Id to use as the entity's own key - unlike most services
// here, this one can't just degrade gracefully to "no rows" for anonymous, so it's
// the one place in this app that requires authentication explicitly.
[Authorize]
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
            PersonalBestMetric = entity?.PersonalBestMetric ?? PersonalBestMetric.Weight,
            PreferredWeightUnit = entity?.PreferredWeightUnit ?? WeightUnitPreference.Imperial,
            PreferredLengthUnit = entity?.PreferredLengthUnit ?? LengthUnitPreference.Metric
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
                PersonalBestMetric = input.PersonalBestMetric,
                PreferredWeightUnit = input.PreferredWeightUnit,
                PreferredLengthUnit = input.PreferredLengthUnit
            };
            await _repository.InsertAsync(entity);
        }
        else
        {
            entity.PersonalBestMetric = input.PersonalBestMetric;
            entity.PreferredWeightUnit = input.PreferredWeightUnit;
            entity.PreferredLengthUnit = input.PreferredLengthUnit;
            await _repository.UpdateAsync(entity);
        }

        return new UserPreferenceDto
        {
            PersonalBestMetric = entity.PersonalBestMetric,
            PreferredWeightUnit = entity.PreferredWeightUnit,
            PreferredLengthUnit = entity.PreferredLengthUnit
        };
    }
}
