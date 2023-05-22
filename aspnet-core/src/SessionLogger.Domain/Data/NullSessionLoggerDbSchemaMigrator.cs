using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SessionLogger.Data;

/* This is used if database provider does't define
 * ISessionLoggerDbSchemaMigrator implementation.
 */
public class NullSessionLoggerDbSchemaMigrator : ISessionLoggerDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
