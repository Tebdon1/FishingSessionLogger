using SessionLogger.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SessionLogger.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SessionLoggerEntityFrameworkCoreModule),
    typeof(SessionLoggerApplicationContractsModule)
    )]
public class SessionLoggerDbMigratorModule : AbpModule
{

}
