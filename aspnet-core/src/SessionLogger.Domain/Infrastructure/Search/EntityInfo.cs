using SessionLogger.Contracts.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Search
{
    public class EntityInfo
    {
        public EntityInfo()
        {
            DefaultSelectedColumnNames = new List<string>();
            SortColumns = new List<SortField>();
            SearchFieldNames = new List<string>();
            ColumnDefinitions = new List<ColumnInfo>();
        }

        public string Id { get; set; }
        public List<ColumnInfo> ColumnDefinitions { get; set; }
        public List<string> DefaultSelectedColumnNames { get; set; }
        public List<string> SearchFieldNames { get; set; }
        public bool UseFullTextSearch { get; set; }
        public List<SortField> SortColumns { get; set; }
        public string EntityTypeLabel { get; set; }
        public string EntityTypeLabelPlural { get; set; }
        public Type DbContextType { get; set; }
        public Type EntityType { get; set; }
        public Type ViewModelType { get; set; }
        public Type SearchResultsViewModelType { get; set; }
        public bool AllowViews { get; set; }
        public string CreatePermission { get; set; }
        public string EditPermission { get; set; }
        public string ViewPermission { get; set; }
        public string DeletePermission { get; set; }
        public string AdminPermission { get; set; }
        public Query DefaultQuery { get; set; }

    }
}
