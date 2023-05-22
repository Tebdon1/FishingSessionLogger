using System.Threading.Tasks;

namespace SessionLogger.Data;

public interface ISessionLoggerDbSchemaMigrator
{
    Task MigrateAsync();
}
