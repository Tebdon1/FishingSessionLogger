using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SessionLogger.Statistics;

public interface IStatisticsAppService : IApplicationService
{
    Task<StatisticsDto> GetAsync(GetStatisticsInput input);
}
