using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Contracts.Search
{

    public class SearchParameters
    {
        public UserViewVM UserView { get; set; }
        public string FolderId { get; set; }
    }

    public class Query
    {
        public String FilterSQL { get; set; }
        public List<QueryLine> FilterLines { get; set; }
        public List<QueryLine> Lines { get; set; }
        public String Keywords { get; set; }
        public List<int> Tags { get; set; }
        public String SearchExpression { get; set; }
        public String SearchMethod { get; set; }
        public Query()
        {
            this.SearchMethod = "all";
            this.Lines = new List<QueryLine>();
            this.Tags = new List<int>();
        }
    }

    public class QueryLine
    {
        public string Field { get; set; }
        public string QueryOperator { get; set; }
        public string Value { get; set; }
    }

    public class SortField
    {
        public SortField()
        {
            IsAscending = true;
        }
        public string FieldName { get; set; }
        public string DisplayText { get { return FieldName; } }
        public bool IsAscending { get; set; }
        /// <summary>
        /// Returns the string value for the direction that dynamic linq needs
        /// </summary>
        public string DirectionString
        {
            get
            {
                return this.IsAscending ? "asc" : "desc";
            }
        }
    }

    public class CustomColumnWidth
    {
        public string Field { get; set; }
        public int Width { get; set; }
    }

    public class UserViewVM
    {
        public string EntityType { get; set; }
        public string EntityInfoId { get; set; }
        public string Name { get; set; }
        public List<string> SelectedColumnNames { get; set; }
        public List<CustomColumnWidth> CustomColumnWidths { get; set; }
        public Query Query { get; set; }
        public Object GridState { get; set; }
        public List<SortField> SortFields { get; set; }
        public bool IsPublic { get; set; }
        public bool UserCanEdit { get; set; }

        public UserViewVM()
        {
            SelectedColumnNames = new List<string>();
            CustomColumnWidths = new List<CustomColumnWidth>();
            SortFields = new List<SortField>();
        }

    }

    public class UserViewDto
    {
        public string EntityType { get; set; }
        public string Name { get; set; }
        public List<string> SelectedColumnNames { get; set; }
        public List<CustomColumnWidth> CustomColumnWidths { get; set; }
        public Query Query { get; set; }
        public Object GridState { get; set; }
        public List<SortField> SortFields { get; set; }

        public UserViewDto()
        {
            SelectedColumnNames = new List<string>();
            CustomColumnWidths = new List<CustomColumnWidth>();
            SortFields = new List<SortField>();
        }

    }

    public class SearchResult
    {

        public IEnumerable<dynamic> Data { get; set; }

        public int Total { get; set; }

    }

    public enum ColumnDataType
    {
        String,
        Integer,
        Decimal,
        Date,
        Boolean,
        Enum,
        CollectionMultiple,
        // only real difference between the two collection types is the wording: multiple uses contains, single uses equals. 
        CollectionSingle
   }

    public class ColumnInfo
    {
        //private string DisplayColumnName;
        public string Title { get; set; }
        public string HeaderTitle { get; set; }
        public string ColumnName { get; set; }
        public string Expression { get; set; } // used for calculated cols etc
        // use this column name in the select query:
        public string SelectColumnName { get; set; }
        public string ColumnAlias { get; set; }
        //[JsonIgnoreAttribute]
        public string Description { get; set; }
        public bool DescriptionLoaded { get; set; }
        public int Width { get; set; }
        public bool Hidden { get; set; }
        public bool Selected { get; set; }
        public string Format { get; set; }
        public bool Filterable { get; set; }
        public string FilterExpression { get; set; }
        public bool Sortable { get; set; }
        public bool Groupable { get; set; }
        public bool Locked { get; set; }
        public string Template { get; set; }
        public object Attributes { get; set; }
        public string Command { get; set; }
        public bool Encoded { get; set; }
        public bool IsCollection { get; set; }
        public bool FilterOnly { get; set; }
        /// <summary>
        /// If tru, data fetch in search always includes the field
        /// </summary>
        public bool AlwaysReturnInResults { get; set; }
        public bool Fixed { get; set; }
        public bool Virtual { get; set; }
        public string FilterColumn { get; set; }
        //public string CollectionFilterColumn { get; set; }
        public string ValueOptionsUrl { get; set; }
        public ColumnDataType DataType { get; set; }
        public ColumnDataType? FilterColumnDataType { get; set; }

        public string TemplateName { get; set; }


        public List<AdditionalCollectionFilter> AdditionalFiltersForCollection { get; set; }
        // this is the column that relates to the actual query line that the user selects:
        public ColumnInfo CollectionFilterColumn { get; set; }


        /// <summary>
        /// Display field name defaults to FieldName but may be overrridden
        ///// </summary>
        //public String DisplayColumnName
        //{ 
        //    get
        //    {
        //        return DisplayColumnName == null ? ColumnName : DisplayColumnName;
        //    } 
        //    set
        //    {
        //        DisplayColumnName = value;
        //    } 
        //}

        /// <summary>
        /// DataType is the primary data type for the column
        /// </summary>

        public ColumnInfo()
        {
            DataType = ColumnDataType.String;
            Width = 100;
            Selected = true;
            Filterable = true;
            Sortable = true;
            DescriptionLoaded = false;
            this.AdditionalFiltersForCollection = new List<AdditionalCollectionFilter>();
        }

        public class AdditionalCollectionFilter
        {
            // for the collection filter we have one field we query on (flagTypeId = x)
            // but we also want to allow additional fields (that the user doesn't control), such as IsDeleted = 0
            public ColumnInfo ColumnInfo { get; set; }
            public QueryLine QueryLine { get; set; }
        }
    }
}
