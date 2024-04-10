using SessionLogger.Contracts.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace SessionLogger.Folders
{
    public class FolderInfoVM
    {
        public bool UserCanAdmin { get; set; }
        public bool UserCanCreate { get; set; }
        public bool UserCanDelete { get; set; }
        public bool UserCanEdit { get; set; }
        public bool UserCanView { get; set; }
        public string AdminPermission { get; set; }
        public string CreatePermission { get; set; }
        public string DeletePermission { get; set; }
        public string EditPermission { get; set; }
        public string ViewPermission { get; set; }
        public List<ColumnInfo> ColumnDefinitions { get; set; }
        public UserViewVM UserView { get; set; }
    }
}
