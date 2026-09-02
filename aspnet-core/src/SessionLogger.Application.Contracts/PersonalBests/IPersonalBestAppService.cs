using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SessionLogger.PersonalBests;

public interface IPersonalBestAppService : IApplicationService
{
    // One entry per species the current user has ever caught, with their best
    // weight and/or best length for it - not every catch records both.
    Task<List<PersonalBestDto>> GetListAsync();
}
