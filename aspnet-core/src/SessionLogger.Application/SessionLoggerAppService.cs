using System;
using System.Collections.Generic;
using System.Text;
using SessionLogger.Localization;
using Volo.Abp.Application.Services;

namespace SessionLogger;

/* Inherit your application services from this class.
 */
public abstract class SessionLoggerAppService : ApplicationService
{
    protected SessionLoggerAppService()
    {
        LocalizationResource = typeof(SessionLoggerResource);
    }
}
