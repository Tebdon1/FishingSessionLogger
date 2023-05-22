using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SessionLogger;

[Dependency(ReplaceServices = true)]
public class SessionLoggerBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SessionLogger";
}
