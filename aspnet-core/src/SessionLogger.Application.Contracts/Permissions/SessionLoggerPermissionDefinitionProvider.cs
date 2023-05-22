using SessionLogger.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SessionLogger.Permissions;

public class SessionLoggerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SessionLoggerPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SessionLoggerPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SessionLoggerResource>(name);
    }
}
