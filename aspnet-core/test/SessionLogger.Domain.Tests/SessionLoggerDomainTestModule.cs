using SessionLogger.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SessionLogger;

[DependsOn(
    typeof(SessionLoggerEntityFrameworkCoreTestModule)
    )]
public class SessionLoggerDomainTestModule : AbpModule
{

}
