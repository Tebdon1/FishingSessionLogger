using SessionLogger.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SessionLogger.Permissions;

public class SessionLoggerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SessionLoggerPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SessionLoggerPermissions.MyPermission1, L("Permission:MyPermission1"));
        myGroup.AddPermission(SessionLoggerPermissions.Lookups.Admin, L(SessionLoggerPermissions.Lookups.Admin), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.Lookups.Search, L(SessionLoggerPermissions.Lookups.Search), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.Lookups.Edit, L(SessionLoggerPermissions.Lookups.Edit), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.Lookups.Delete, L(SessionLoggerPermissions.Lookups.Delete), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.Lookups.Create, L(SessionLoggerPermissions.Lookups.Create), MultiTenancySides.Both);

        myGroup.AddPermission(SessionLoggerPermissions.SessionConfig.Admin, L(SessionLoggerPermissions.SessionConfig.Admin), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.SessionConfig.Search, L(SessionLoggerPermissions.SessionConfig.Search), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.SessionConfig.Edit, L(SessionLoggerPermissions.SessionConfig.Edit), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.SessionConfig.Delete, L(SessionLoggerPermissions.SessionConfig.Delete), MultiTenancySides.Both);
        myGroup.AddPermission(SessionLoggerPermissions.SessionConfig.Create, L(SessionLoggerPermissions.SessionConfig.Create), MultiTenancySides.Both);

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SessionLoggerResource>(name);
    }
}
