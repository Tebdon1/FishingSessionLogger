using SessionLogger.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SessionLogger.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SessionLoggerController : AbpControllerBase
{
    protected SessionLoggerController()
    {
        LocalizationResource = typeof(SessionLoggerResource);
    }
}
