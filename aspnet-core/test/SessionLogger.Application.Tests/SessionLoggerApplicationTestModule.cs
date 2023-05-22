using Volo.Abp.Modularity;

namespace SessionLogger;

[DependsOn(
    typeof(SessionLoggerApplicationModule),
    typeof(SessionLoggerDomainTestModule)
    )]
public class SessionLoggerApplicationTestModule : AbpModule
{

}
