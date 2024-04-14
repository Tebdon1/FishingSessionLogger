using SessionLogger.Contracts.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace SessionLogger.Folders
{
    public class EntityInfoVM
    {
        public int Id { get; set; }
        public EntityInfoVM EntityInfo { get; set; }
        public EntityInfoVM FolderEntityInfo { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public bool UserCanAdmin { get; set; }
        public bool UserCanCreate { get; set; }
        public bool UserCanEdit { get; set; }
        public bool UserCanView { get; set; }
        public bool UserCanDelete { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentFolder_Id { get; set; }
        public int DraftFileCount { get; set; }

        public List<ColumnInfo> ColumnDefinitions { get; set; }
        public List<string> DefaultSelectedColumnNames { get; set; }
        public Query DefaultQuery { get; set; }
        public List<SortField> DefaultSortColumns { get; set; }

        public string EntityTypeLabel { get; set; }
        public string EntityTypeLabelPlural { get; set; }
        //public string QuickSearchLabel { get; set; }
        public string TypeName { get; set; }

        public string PageTitle { get; set; }
        public string SearchControllerName { get; set; }
        public bool AllowTileView { get; set; }
        public bool AllowFolders { get; set; }
        public bool AllowViews { get; set; }
        public string JsonFormProperties { get; set; }
        public bool IsDocumentFolder { get; set; }

    }
}
