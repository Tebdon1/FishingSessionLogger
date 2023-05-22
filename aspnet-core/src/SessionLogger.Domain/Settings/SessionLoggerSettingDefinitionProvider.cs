using Volo.Abp.Settings;

namespace SessionLogger.Settings;

public class SessionLoggerSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SessionLoggerSettings.MySetting1));
    }
}
