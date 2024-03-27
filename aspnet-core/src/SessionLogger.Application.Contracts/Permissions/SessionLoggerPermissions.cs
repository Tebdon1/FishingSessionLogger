namespace SessionLogger.Permissions;

public static class SessionLoggerPermissions
{
    public const string GroupName = "SessionLogger";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
    public static class Lookups
    {
        private const string Group = GroupName + ".Lookups";
        public const string Admin = Group + ".Admin";
        public const string Search = Group + ".Search";
        public const string Create = Group + ".Create";
        public const string Delete = Group + ".Delete";
        public const string Edit = Group + ".Edit";
    }

    public static class SessionConfig
    {
        private const string Group = GroupName + ".SessionConfig";
        public const string Admin = Group + ".Admin";
        public const string Search = Group + ".Search";
        public const string Create = Group + ".Create";
        public const string Delete = Group + ".Delete";
        public const string Edit = Group + ".Edit";
    }
}
