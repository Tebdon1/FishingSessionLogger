using SessionLogger.Contracts.Search;
using SessionLogger.Folders;
using SessionLogger.Infrastructure.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SessionLogger.Search
{
    public static class SearchHelper
    {
        private static List<string> Noise = null;


        //public static async Task<SearchResult> Search(IQueryable baseQuery, EntityInfo entityInfo, SearchParameters searchParameters)
        //{
        //    string[] currentUserPermissions = new string[0]; //  Umbraco.MembershipHelper.GetCurrentUserRoles().ToArray();

        //    MethodInfo MI = this.GetType().GetMethod("GetDocumentsGeneric");

        //    MethodInfo searchGenericMethod = MI.MakeGenericMethod(new[] { entityInfo.EntityType });

        //    return (SearchResult) await searchGenericMethod.InvokeAsync(null, new object[] { baseQuery, entityInfo, searchParameters.UserView,  false, currentUserPermissions });
        //}


        public static async Task<SearchResult> Search<T>(
            IQueryable<T> baseQuery,
            EntityInfo entityInfo,
            UserViewVM userView = null,
            bool idsOnly = false,
            int skip = 0, 
            int take = 500,
            List<string> noise = null
        ) where T : IItem
        {
            if (noise == null)
            {
                noise = new List<string>();
            }
            var crossFolderTagSearchFolderIds = new List<int>();
            var crossFolderTagSearchTagIds = new List<int>();


            InitEntityInfo(entityInfo);

            var query = GetSearchQuery<T>(
                baseQuery: baseQuery,
                userView: userView,
                entityInfo: entityInfo,
                noise: noise
                );

            // build the dynamic select string

            // add in view access information
            var additionalFields = new StringBuilder();
            //additionalFields.Append(",Parent_Id as parentFolderId");
            //additionalFields.Append(",Private as isPrivate");
            //additionalFields.Append(",PrivateCode as privateCode");
            //additionalFields.Append(",RelatedOnly as relatedOnly");

            string selectString = "";
            if (idsOnly)
            {
                selectString = "new(Id as id)";
            }
            else
            {
                selectString = GetSelectColumns(entityInfo, userView.SelectedColumnNames, additionalFields.ToString());
            }

            string orderBy = "";
            foreach (var sortField in userView.SortFields.Where(x => !string.IsNullOrWhiteSpace(x.FieldName)))
            {
                var columnInfo = entityInfo.ColumnDefinitions.FirstOrDefault(x => string.Equals(x.ColumnName, sortField.FieldName, StringComparison.InvariantCultureIgnoreCase));
                if (columnInfo != null && columnInfo.Sortable)
                {
                    orderBy = orderBy + (orderBy.Length > 0 ? "," : "") + $"{sortField.FieldName} {sortField.DirectionString}";
                }
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = query.OrderBy(orderBy);
            }

            // select the columns:
            var countQuery = query.Select(x => x.Id);

            var count = countQuery.Count();

            if (take == 0)
            {
                take = count;
            }

            var results = await query.Select(selectString).Skip(skip).Take(take).ToDynamicArrayAsync();

            return new SearchResult() { Data= results, Total = count };
        }
        public static IQueryable<T> GetSearchQuery<T>(IQueryable<T> baseQuery, EntityInfo entityInfo, UserViewVM userView, List<string> noise) where T : IItem
        {
            IQueryable<T> query = baseQuery;

            var viewQuery = (Query)userView.Query;

            viewQuery.Lines = viewQuery.Lines.Where(x => x != null && x.Field != null).ToList();

            bool keywordSearch = false;

            int dummy = 0;

            if (!string.IsNullOrEmpty(viewQuery.Keywords) && viewQuery.Keywords.StartsWith("#") && viewQuery.Keywords.Length > 1 && int.TryParse(viewQuery.Keywords.Substring(1), out dummy))
            {
                // id search
                int id = int.Parse(viewQuery.Keywords.Substring(1));
                query = query.Where(x => x.Id == id);
            }
            else
            {
                keywordSearch = !string.IsNullOrEmpty(viewQuery.Keywords);
            }

            if (keywordSearch)
            {
                try
                {
                    if (entityInfo.UseFullTextSearch && FullTextValidKeywords(viewQuery.Keywords, noise))
                    {
                        // single file match method - runs the query against each individual file so if keywords spread across files it will not find a match
                        bool SingleFileMethod = false;
                        if (SingleFileMethod)
                        {
                            // try to use full text query
                            string searchPrefixed = FullTextPrefixes.Contains(viewQuery.Keywords);

                            // FileDataSearch is there as a dummy to allow us to use Contains - this gets replaced in the interceptor
                            query = query.Where(x => ((IFullTextSearchable)x).Files.Any(y => y.FileDataSearch.Contains(searchPrefixed)));
                        }
                        else
                        {
                            // process the query and build predicates for the logic
                            query = query.Where(BuildCrossFilePredicate<T>(viewQuery.Keywords, noise));
                        }

                        // prevent any further processing of keywords
                        keywordSearch = false;
                    }
                }
                catch 
                {
                    // full text failed so we will drop into simple search
                }
            }

            if (keywordSearch)
            {
                if (!entityInfo.UseFullTextSearch && FullTextValidKeywords(viewQuery.Keywords, noise))
                {
                    // single file match method - runs the query against each individual file so if keywords spread across files it will not find a match
                        // process the query and build predicates for the logic
                    query = query.Where(GetMultiFieldWHERE(entityInfo, viewQuery.Keywords, noise));

                    // prevent any further processing of keywords
                    keywordSearch = false;
                }
            }



            //if (viewQuery.Tags.Count > 0)
            //{
            //    foreach (int tagId in viewQuery.Tags)
            //    {
            //        query = query.Where(x => x.ItemTagAssignments.Any(y => y.Tag_Id == tagId));
            //    }
            //}

            //if (keywordSearch)
            //{
            //    // use entityHelper keyword search
            //    query = (IQueryable<T>)entityHelper.QuickSearch(query, viewQuery.Keywords);
            //}

            if (!string.IsNullOrWhiteSpace(viewQuery.FilterSQL))
            {
                query = query.Where(viewQuery.FilterSQL, new Object[] { });
            }

            if (viewQuery.FilterLines != null && viewQuery.FilterLines.Count() > 0)
            {
                var querySQL = GetQuerySQL(
                    entityInfo: entityInfo,
                    lines: viewQuery.FilterLines,
                    searchMethod: "all"
                );

                query = query.Where(querySQL.SQL, querySQL.Parameters);
            }

            if (viewQuery.Lines != null && viewQuery.Lines.Count() > 0)
            {
                CleanSearchExpression(viewQuery);

                // now add in advance filter lines
                var querySQL = GetQuerySQL(
                    entityInfo: entityInfo,
                    lines: viewQuery.Lines,
                    searchMethod: viewQuery.SearchMethod,
                    searchExpression: viewQuery.SearchExpression
                );

                query = query.Where(querySQL.SQL, querySQL.Parameters);
            }

            return query;
        }

        public static bool FullTextValidKeywords(string keywords, List<string> noise)
        {
            return (GetTerms(keywords, noise).Count > 0);
        }

        public class QuerySQL
        {
            public string SQL { get; set; }
            public List<object> Parameters { get; set; }
            public int ParameterIndex { get; set; }
        }

        public static QuerySQL GetQuerySQL(EntityInfo entityInfo, List<QueryLine> lines, string removePrefix = "", int parameterIndex = 0, string searchMethod="all", string searchExpression = "")
        {
            var parameters = new List<object>();
            String expression = "";
            var sb = new StringBuilder();

            switch (searchMethod)
            {
                case "all":
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(lines[i].Field))
                        {
                            if (i > 0)
                            {
                                sb.Append(" AND ");
                            }
                            QuerySQL querySQL = GetSearchCriterionForDynamicQuery(entityInfo, lines[i], removePrefix, parameterIndex);
                            parameters.AddRange(querySQL.Parameters);
                            parameterIndex = querySQL.ParameterIndex;
                            sb.Append(querySQL.SQL);
                        }
                    }

                    expression = sb.ToString();
                    break;
                case "any":
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(lines[i].Field))
                        {
                            if (i > 0)
                            {
                                sb.Append(" OR ");
                            }
                            QuerySQL querySQL = GetSearchCriterionForDynamicQuery(entityInfo, lines[i], removePrefix, parameterIndex);
                            if (querySQL != null)
                            {
                            }
                            parameters.AddRange(querySQL.Parameters);
                            parameterIndex = querySQL.ParameterIndex;
                            sb.Append(querySQL.SQL);
                        }
                    }
                    expression = sb.ToString();

                    break;
                case "advanced":
                    // we only allow digits, AND,OR,(,)
                    expression = searchExpression;

                    // replace expression numbers with markers - descenging order so we can replace 11 then 1 etc
                    for (int i = lines.Count; i > 0; i--)
                    {
                        expression = expression.Replace(i.ToString(), String.Format("_marker_{0}_", i));
                    }

                    // now replace markers with SQL expressions
                    for (int i = lines.Count; i > 0; i--)
                    {
                        if (!String.IsNullOrEmpty(lines[i - 1].Field))
                        {
                            QuerySQL querySQL = GetSearchCriterionForDynamicQuery(entityInfo, lines[i - 1], removePrefix, parameterIndex);
                            parameters.AddRange(querySQL.Parameters);
                            parameterIndex = querySQL.ParameterIndex;
                            expression = expression.Replace(String.Format("_marker_{0}_", i), querySQL.SQL);
                        }
                    }
                    break;
            }


            return new QuerySQL
            {
                SQL = expression,
                Parameters = parameters,
                ParameterIndex = parameterIndex
            };
        }

        /// <summary>
        /// Checks and cleans the filter expression
        /// </summary>
        public static void CleanSearchExpression(Query viewQuery)
        {
            // we only allow digits, AND,OR,(,)
            if (string.IsNullOrEmpty(viewQuery.SearchExpression))
            {
                viewQuery.SearchExpression = "";
            }
            string expression = viewQuery.SearchExpression.Trim().ToUpper().Replace("AND", "&").Replace("OR", "+").Replace("NOT", "-");

            if (String.IsNullOrEmpty(expression))
            {
                expression = "1";
            }

            // scan for validity
            var sbNew = new StringBuilder();
            int nestCount = 0;
            String lastChar = "";
            String lastNonSpaceChar = "";
            for (short i = 0; i <= expression.Length - 1; i++)
            {
                string c = expression.Substring(i, 1);
                bool use = true;

                switch (c)
                {
                    case "(":
                        nestCount += 1;
                        break;
                    case ")":
                        if (nestCount > 0)
                            nestCount -= 1;
                        else
                        {
                            // already closed all
                            use = false;
                        }
                        if (lastNonSpaceChar == "(")
                        {
                            // empty brackets
                            use = false;
                        }
                        break;
                    case "&":
                        if (lastNonSpaceChar.IndexOfAny("(&+-".ToCharArray()) > -1 || lastNonSpaceChar == "")
                        {
                            use = false;
                        }
                        break;
                    case "+":
                        // no AND OR,OR OR,(OR, NOT OR
                        if (lastNonSpaceChar.IndexOfAny("(&+-".ToCharArray()) > -1 || lastNonSpaceChar == "")
                        {
                            use = false;
                        }
                        break;
                    case "-":
                        // no NOT NOT
                        if (lastNonSpaceChar == "-")
                        {
                            use = false;
                        }
                        break;
                    case " ":
                        if (lastChar == " ")
                        {
                            // double space
                            use = false;
                        }
                        break;
                    default:
                        String digits = "123456789";
                        if (c.IndexOfAny(digits.ToCharArray()) > -1)
                        {
                            if (lastChar == " " && lastNonSpaceChar.IndexOfAny(digits.ToCharArray()) > -1)
                                // no n n 
                                use = false;
                        }
                        else
                        {
                            // disallowed character
                            use = false;
                        }
                        break;
                }

                if (use)
                {
                    if (c != " ")
                    {
                        lastNonSpaceChar = c;
                    }
                    lastChar = c;
                    sbNew.Append(c);
                }
            }
            expression = sbNew.ToString();

            bool ok = false;
            while (!ok)
            {
                ok = true;
                String check = "()=,( =(, )=),+)=),-)=),&)=),& &=&,- -=-,+ +=+,(+=(,(&=(";
                foreach (string replacement in check.Split(','))
                {
                    String search = replacement.Substring(0, replacement.IndexOf("="));
                    String replace = replacement.Substring(replacement.IndexOf("=") + 1);
                    while (expression.IndexOf(search) > -1)
                    {
                        expression = expression.Replace(search, replace);
                        ok = false;
                    }

                }
            }

            while (expression.EndsWith("&") || expression.EndsWith("-") || expression.EndsWith("+") || expression.EndsWith("("))
            {
                expression = expression.Substring(0, expression.Length - 1);
                expression = expression.Trim();
            }

            for (var i = 1; i < viewQuery.Lines.Count; i++)
            {
                expression = expression.Replace(String.Format("({0})", i), i.ToString());
            }

            expression = expression.Replace("&", "AND").Replace("+", "OR").Replace("-", "NOT");

            viewQuery.SearchExpression = expression;
        }

        public static String DynamicQueryValue(String value, ColumnDataType? dataType)
        {
            String r = "";
            if (string.IsNullOrEmpty(value))
            {
                r = "null";
            }
            else
            {
                switch (dataType)
                {
                    case ColumnDataType.String:
                        r = "\"" + value.Replace("\"", "\\" + "\"") + "\"";
                        break;
                    case ColumnDataType.Date:
                        if (string.IsNullOrEmpty(value))
                        {
                            r = null;
                        }
                        else
                        {
                            // r = DateTime.Parse(value).ToString("yyyy-MM-dd"); // +"T" + DateTime.Parse(value).ToString("HH:mm:ss");
                            r = String.Format("Convert.ToDateTime(\"{0}\")", r);
                        }
                        break;
                    case ColumnDataType.Integer:
                        r = Int32.Parse(value).ToString();
                        break;
                    case ColumnDataType.Decimal:
                        r = Decimal.Parse(value).ToString();
                        break;
                    case ColumnDataType.Boolean:
                        r = value;
                        break;
                    case ColumnDataType.Enum:
                        r = "\"" + value.Replace("\"", "\\" + "\"") + "\"";
                        break;
                    default:
                        throw (new Exception("Unhandled filterCriterion.Column.DataType"));
                }
            }
            return r;
        }

        private static QuerySQL GetSearchCriterionForDynamicQuery(EntityInfo entityInfo, QueryLine queryLine, string removePrefix = "", int parameterIndex = 0)
        {
            var column = entityInfo.ColumnDefinitions.Where(x => x.ColumnAlias == queryLine.Field).FirstOrDefault();

            return GetSearchCriterionForDynamicQuery(column, queryLine, removePrefix, parameterIndex);
        }

        public static QuerySQL GetSearchCriterionForDynamicQuery(ColumnInfo column, QueryLine queryLine, string removePrefix = "", int parameterIndex = 0)
        {
            var parameters = new List<object>();

            if (column == null)
            {
                throw new Exception("Field " + queryLine.Field + " not found");
            }

            string columnName = column.FilterColumn.Replace("__", ".");
            if (!string.IsNullOrEmpty(removePrefix))
            {
                if (columnName.StartsWith(removePrefix))
                {
                    columnName = columnName.Substring(removePrefix.Length);
                }
            }
            if (!string.IsNullOrEmpty(column.FilterExpression))
            {
                columnName = column.FilterExpression.Replace("__", ".");
            }
            if (column.FilterColumnDataType == null) column.FilterColumnDataType = column.DataType;

            var sb = new StringBuilder();
            sb.Append("(");
            String valueSQL = "";
            String expressionTemplate = "";
            var hasNoValue = false;
            if (queryLine.QueryOperator == "blank" || queryLine.QueryOperator == "notblank" || queryLine.QueryOperator == "true" || queryLine.QueryOperator == "false" )
            {
                hasNoValue = true;
                queryLine.Value = "";
            }
            if ((string.IsNullOrEmpty(queryLine.Value) || hasNoValue)
                && !new ColumnDataType[] { ColumnDataType.CollectionMultiple, ColumnDataType.CollectionSingle }.Contains(column.FilterColumnDataType.Value))
            {
                switch (queryLine.QueryOperator)
                {
                    case "blank":
                        if (column.FilterColumnDataType == ColumnDataType.String)
                        {
                            expressionTemplate = "({0} = null || string.IsNullOrEmpty({0}.Trim()))";
                        }
                        else
                        {
                            expressionTemplate = "{0} = null";
                        }
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "notblank":
                        if (column.FilterColumnDataType == ColumnDataType.String)
                        {
                            expressionTemplate = "({0} != null && !string.IsNullOrEmpty({0}.Trim()))";
                        }
                        else
                        {
                            expressionTemplate = "{0} != null";
                        }
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "true":
                        expressionTemplate = "{0} = true";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "false":
                        expressionTemplate = "{0} = false";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "equalto":
                        expressionTemplate = "{0} = null";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "notequalto":
                        expressionTemplate = "{0} != null";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "greaterthan":
                        expressionTemplate = "{0} != null";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "lessthan":
                        expressionTemplate = "{0} != null";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "contains":
                        expressionTemplate = "{0}.Contains(null)";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    case "notcontains":
                        expressionTemplate = "not ({0}.Contains(null))";
                        sb.Append(String.Format(expressionTemplate, columnName));
                        break;
                    default:
                        throw (new Exception("Invalid search criterion"));
                }
            }
            else
            {
                if (column.FilterColumnDataType == ColumnDataType.CollectionMultiple || column.FilterColumnDataType == ColumnDataType.CollectionSingle)
                {
                    var exp = new StringBuilder();
                    switch (queryLine.QueryOperator)
                    {
                        case "equalto":
                        case "contains":
                        case "notblank":
                            exp.Append(String.Format("{0}.Any(", columnName));
                            break;
                        case "notequalto":
                        case "notcontains":
                        case "blank":
                        default:
                            exp.Append(String.Format("not {0}.Any(", columnName));
                            break;
                    }
                    if (column.AdditionalFiltersForCollection.Any())
                    {
                        for (var i = 0; i < column.AdditionalFiltersForCollection.Count(); i++)
                        {
                            if (i > 0)
                            {
                                exp.Append(" AND ");
                            }
                            var filter = column.AdditionalFiltersForCollection[i];
                            exp.Append(GetSearchCriterionForDynamicQuery(filter.ColumnInfo, filter.QueryLine).SQL);
                        }
                    }
                    if (queryLine.QueryOperator != "blank" && queryLine.QueryOperator != "notblank")
                    {
                        if (column.AdditionalFiltersForCollection.Any())
                        {
                            exp.Append(" AND ");
                        }

                        var filterQueryLine = new QueryLine { Field = queryLine.Field, Value = queryLine.Value };
                        switch (queryLine.QueryOperator)
                        {
                            case "contains":
                            case "notcontains":
                                filterQueryLine.QueryOperator = "equalto";
                                break;
                            default:
                                filterQueryLine.QueryOperator = queryLine.QueryOperator;
                                break;
                        }
                        exp.Append(GetSearchCriterionForDynamicQuery(column.CollectionFilterColumn, filterQueryLine).SQL);
                    }
                    exp.Append(")");
                    sb.Append(exp.ToString());
                }
                else if (column.FilterColumnDataType == ColumnDataType.Date)
                {
                    DateTime queryDate = DateTime.Parse(queryLine.Value);
                    valueSQL = "DateTime(" + queryDate.ToString("yyyy,MM,dd") + ")";
                    switch (queryLine.QueryOperator)
                    {
                        case "equalto":
                            expressionTemplate = "{0} = {1}";
                            break;
                        case "notequalto":
                            expressionTemplate = "{0} <> {1}";
                            break;
                        case "greaterthan":
                            expressionTemplate = "{0} > {1}";
                            break;
                        case "lessthan":
                            expressionTemplate = "{0} < {1}";
                            break;
                        default:
                            throw (new Exception("Unhandled QueryOperator " + queryLine.QueryOperator));
                    }
                    sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                }
                else
                {
                    switch (queryLine.QueryOperator)
                    {
                        case "equalto":
                            expressionTemplate = "{0} = {1}";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        case "notequalto":
                            expressionTemplate = "{0} <> {1}";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        case "greaterthan":
                            expressionTemplate = "{0} > {1}";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        case "lessthan":
                            expressionTemplate = "{0} < {1}";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        case "contains":
                            if (column.IsCollection)
                            {
                                int matchId = Convert.ToInt32(queryLine.Value);
                                expressionTemplate = "{0} != null AND {0}.Any({1} = {2})";
                                sb.Append(String.Format(expressionTemplate, columnName, column.CollectionFilterColumn, matchId));
                            }
                            else
                            {
                                expressionTemplate = "{0}.Contains({1})";
                                valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                                sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            }
                            break;
                        case "notcontains":
                            if (column.IsCollection)
                            {
                                int matchId = Convert.ToInt32(queryLine.Value);

                                expressionTemplate = "not ({0}.Any({1} = {2}))";
                                sb.Append(String.Format(expressionTemplate, columnName, column.CollectionFilterColumn, matchId));

                            }
                            else
                            {
                                expressionTemplate = "({0} = null || not ({0}.Contains({1})))";
                                valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                                sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            }
                            break;
                        case "startswith":
                            expressionTemplate = "{0}.StartsWith({1})";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        case "endswith":
                            expressionTemplate = "{0}.EndsWith({1})";
                            valueSQL = DynamicQueryValue(queryLine.Value, column.FilterColumnDataType);
                            sb.Append(String.Format(expressionTemplate, columnName, valueSQL));
                            break;
                        default:
                            throw (new Exception("Unhandled QueryOperator " + queryLine.QueryOperator));
                    }

                }
            }
            sb.Append(")");
            return new QuerySQL
            {
                SQL = sb.ToString(),
                Parameters = parameters,
                ParameterIndex = parameterIndex
            };
        }


        private static Expression<TDelegate> Negate<TDelegate>(Expression<TDelegate> expression)
        {
            return Expression.Lambda<TDelegate>(Expression.Not(expression.Body), expression.Parameters);
        }



        public enum KeywordMode
        {
            All,
            Any
        }

        public static List<string> GetTerms(string keywords, List<string> noise)
        {
            int i = 0;
            bool finishedTerm = false;
            bool inQuote = false;
            string query = keywords;
            string term = "";
            bool termHadQuotes = false;
            var aterms = new List<string>();
            while (i < query.Length)
            {
                finishedTerm = false;
                i++;
                var c = query.Substring(i - 1, 1);

                if (c == "\"")
                {
                    //it's a quote

                    // is it a pair of quotes?
                    bool blnPair = false;
                    if (i < query.Length)
                    {
                        if (query.Substring(i, 1) == "\"")
                        {
                            // there's a quote after this
                            blnPair = true;
                        }
                    }
                    if (blnPair)
                    {
                        c = "\"";
                        i++;
                    }
                    else
                    {
                        // single quote
                        inQuote = !inQuote;
                        if (inQuote)
                        {
                            // now coming out of a quoted phrase
                            finishedTerm = true;
                        }
                        else
                        {
                            termHadQuotes = true;
                        }
                    }

                    // don't let quote through
                    c = "";
                }

                var ccopy = c;
                if (c == " " || c == "," || c == "(" || c == ")" && !inQuote)
                {
                    // these characters finish a term unless within a quote
                    c = "";
                    finishedTerm = true;
                }
                if (c == "-" && term.Trim() == "" && !inQuote)
                {
                    // - is the same as AND NOT unless within a term
                    c = "";
                    finishedTerm = true;
                }
                if (i == query.Length)
                {
                    // end of string
                    finishedTerm = true;
                }

                if (c == "+" && !inQuote)
                {
                    //ignore + signs
                    c = "";
                }

                term = term + c;
                if (finishedTerm)
                {
                    // finished with term

                    // double up double quotes for query
                    term = term.Replace("\"", "\"\"").Trim().ToLower();
                    if (!string.IsNullOrEmpty(term))
                    {
                        //does the term look like a document? if so, process it separately
                        bool blnNormalTerm = true;
                        if (blnNormalTerm) { }
                        // check we don't have to ignore this word
                        if (!termHadQuotes && (term.IndexOf("*") > -1 || term.IndexOf("?") > -1 || term.Trim().ToLower() == "or" || term.Trim().ToLower() == "and" || term.Trim().ToLower() == "not" || term.Trim().ToLower() == "near" || term.Trim().ToLower() == "-" || term.Trim().ToLower() == "+"))
                        {
                            aterms.Add(term.Trim().ToLower());
                        }
                        else
                        {
                            if (!noise.Contains(term.Trim()) && term.Length > 1)
                            {
                                // this is not a noise word
                                // but, a phrase may be made up of words that are all ignore and this will cause an error (e.g. "q&a")
                                int z = 0;
                                bool blnFoundValidWord = false;
                                bool gotWord = false;
                                string word = "";
                                word = "";
                                for (z = 1; z <= term.Length; z++)
                                {
                                    c = term.Substring(z - 1, 1);
                                    gotWord = false;
                                    if ("abcdefghijklmnopqrstuvwxyz0123456789".IndexOf(c) > -1)
                                    {
                                        // valid character
                                        word = word + c;
                                    }
                                    else
                                    {
                                        // punctuation
                                        gotWord = true;
                                    }
                                    if (z == term.Length)
                                    {
                                        gotWord = true;
                                    }
                                    if (gotWord)
                                    {
                                        if (word.Trim() == "")
                                        {
                                            gotWord = false;
                                        }
                                    }
                                    if (gotWord)
                                    {
                                        if (!noise.Contains(word))
                                        {
                                            // word is ok
                                            blnFoundValidWord = true;
                                            break;
                                        }
                                        word = "";
                                    }
                                }

                                if (blnFoundValidWord)
                                {
                                    aterms.Add((termHadQuotes ? "[quoted]" : "") + term);
                                };
                            }
                        }
                    }
                    term = "";
                    if (ccopy == "-" || ccopy == "(" || ccopy == ")")
                    {
                        aterms.Add(ccopy);
                    }
                    // prepare for next term
                    termHadQuotes = false;
                }
            }

            return aterms;
        }


        private static Expression<Func<T, bool>> BuildCrossFilePredicate<T>(string keywords, List<string> noise) where T : IItem
        {
            var terms = GetTerms(keywords, noise);

            int termPointer = 0;

            return GetPredicate<T>(terms, ref termPointer);
        }

        private static Expression<Func<T, bool>> GetPredicate<T>(List<string> terms, ref int termPointer) where T : IItem
        {
            var predicate = PredicateBuilder.True<T>();

            string expression = "";

            bool negated = false;

            string op = "";

            while (termPointer < terms.Count)
            {
                var term = terms[termPointer];
                termPointer++;

                switch (term.Trim().ToLower())
                {
                    case "near":
                        expression += " NEAR ";
                        break;
                    case "and":
                    case "+":
                        op = "AND";
                        break;
                    case "or":
                        op = "OR";
                        break;
                    case "not":
                    case "-":
                        negated = true;
                        break;
                    case "(":
                        // going into new level for expression
                        var nestedPredicate = GetPredicate<T>(terms, ref termPointer);

                        if (negated)
                        {
                            nestedPredicate = Negate(nestedPredicate);
                        }

                        switch (op)
                        {
                            case "AND":
                                predicate = predicate.And(nestedPredicate);
                                break;
                            case "OR":
                                predicate = predicate.Or(nestedPredicate);
                                break;
                        }

                        // reset for next term
                        negated = false;
                        expression = "";
                        op = "";

                        break;
                    case ")":
                        // finished with this level

                        return predicate;
                    default:
                        // got a term
                        expression += term;

                        string searchPrefixed = FullTextPrefixes.Contains(expression);

                        if (negated)
                        {
                            switch (op)
                            {
                                case "OR":
                                    predicate = predicate.Or(x => !((IFullTextSearchable)x).Files.Any(y => y.FileDataSearch.Contains(searchPrefixed)));
                                    break;
                                default:
                                    predicate = predicate.And(x => !((IFullTextSearchable)x).Files.Any(y => y.FileDataSearch.Contains(searchPrefixed)));
                                    break;
                            }
                        }
                        else
                        {
                            switch (op)
                            {
                                case "OR":
                                    predicate = predicate.Or(x => ((IFullTextSearchable)x).Files.Any(y => y.FileDataSearch.Contains(searchPrefixed)));
                                    break;
                                default:
                                    predicate = predicate.And(x => ((IFullTextSearchable)x).Files.Any(y => y.FileDataSearch.Contains(searchPrefixed)));
                                    break;
                            }
                        }

                        // reset for next term
                        negated = false;
                        expression = "";
                        op = "";

                        break;
                }
            }

            return predicate;
        }

        private static string GetMultiFieldWHERE(EntityInfo entityInfo, string keywords, List<string> noise) 
        {
            var terms = GetTerms(keywords, noise);

            int termPointer = 0;

            return GetMultiFieldWHEREInner(entityInfo, terms, entityInfo.SearchFieldNames, ref termPointer);
        }

        private static string GetMultiFieldWHEREInner(EntityInfo entityInfo, List<string> terms, List<string> fieldNames, ref int termPointer) 
        {
            var where = new StringBuilder();
            var expression = new StringBuilder();

            bool negated = false;

            string op = "";

            while (termPointer < terms.Count)
            {
                var term = terms[termPointer];
                termPointer++;

                switch (term.Trim().ToLower())
                {
                    //case "near":
                    //    expression += " NEAR ";
                    //    break;
                    case "and":
                    case "+":
                        op = "AND";
                        break;
                    case "or":
                        op = "OR";
                        break;
                    case "not":
                    case "-":
                        negated = true;
                        break;
                    case "(":
                        // going into new level for expression
                        var nestedClause = GetMultiFieldWHEREInner(entityInfo, terms, fieldNames, ref termPointer);

                        if (negated)
                        {
                            nestedClause = $"!({nestedClause})";
                        }

                        switch (op)
                        {
                            case "AND":
                                where.Append($" & ({nestedClause})");
                                break;
                            case "OR":
                                where.Append($" || ({nestedClause})");
                                break;
                        }

                        // reset for next term
                        negated = false;
                        expression = new StringBuilder();
                        op = "";

                        break;
                    case ")":
                        // finished with this level

                        return where.ToString();
                    default:
                        // got a term
                        var isQuoted = term.EndsWith("[quoted]");
                        term = term.Replace("[quoted]", "");

                        var clause = new StringBuilder();
                        clause.Append("(");

                        foreach (var fieldName in fieldNames)
                        {
                            bool skipFieldClause = false;

                            string fieldClause = "";
                                
                            var property = GetPropertyDeep(entityInfo.EntityType, fieldName);

                            var type = property.PropertyType;

                            if (type.FullName.Contains("Int32"))
                            {
                                int dummy;
                                if (int.TryParse(term, out dummy))
                                {
                                    fieldClause = $"{fieldName} = {term}";
                                }
                                else 
                                {
                                    skipFieldClause = true;
                                }
                            }
                            else
                            {
                                if (isQuoted)
                                {
                                    fieldClause = $"{fieldName} = \"{term}\"";
                                }
                                else
                                {
                                    fieldClause = $"{fieldName}.Contains(\"{term}\")";
                                }
                            }

                            if (!skipFieldClause)
                            {
                                if (clause.Length > 1)
                                {
                                    clause.Append(" OR ");
                                }
                                clause.Append(fieldClause);
                            }
                        }

                        if (clause.Length > 1)
                        {
                            clause.Append(")");

                            if (negated)
                            {
                                clause.Insert(0, "!");
                            }

                            if (where.Length == 0)
                            {
                                where.Append(clause);
                            }
                            else
                            {
                                switch (op)
                                {
                                    case "OR":
                                        where.Append($" OR {clause}");
                                        break;
                                    default:
                                        where.Append($" AND {clause}");
                                        break;
                                }
                            }
                        }

                        // reset for next term
                        negated = false;
                        expression = new StringBuilder();
                        op = "";

                        break;
                }
            }

            return where.ToString();
        }


        public static string GenerateColumnSelect(EntityInfo entityInfo, ColumnInfo columnInfo)
        {
            StringBuilder sbResult = new StringBuilder(); // the final result
            Type propertyType = null;
            string selectColumnName = string.IsNullOrWhiteSpace(columnInfo.SelectColumnName) ? columnInfo.ColumnName : columnInfo.SelectColumnName;
            bool skip = false;

            if (!string.IsNullOrWhiteSpace(columnInfo.Expression))
            {
                sbResult.Append(columnInfo.Expression);
            }
            else
            {
                string propertyPath = selectColumnName.Replace("__", ".");

                var dataType = columnInfo.DataType;

                string[] classes = propertyPath.Split('.');//colName.Split(new[] {"__"}, StringSplitOptions.None);

                StringBuilder currentProperty = new StringBuilder(); // keep track of the property we are up to (for example - Member.Account.DDMandate)
                currentProperty.Append(classes[0]);

                var pi = GetNestedPropertyInfo(propertyPath, entityInfo.EntityType);

                if (pi == null)
                {
                    // property not found - may be virtual / function
                    skip = true;
                }
                else
                {

                    propertyType = pi.PropertyType;

                    // open brackets for all null checks for class path (e.g. Property.SubProperty.SubProperty2 may have null Property or SubProperty
                    if (classes.Length > 1)
                    {
                        for (var i = 0; i <= classes.Length - 2; i++)
                        { // this loops through all members but the last                
                            if (i > 0)
                            {
                                currentProperty.Append(".");
                                currentProperty.Append(classes[i]);
                            }
                            if (propertyType == typeof(Boolean))
                            {
                                sbResult.Append(string.Concat("iif(", currentProperty.ToString(), "==null,false,"));
                            }
                            else
                            {
                                sbResult.Append(string.Concat("iif(", currentProperty.ToString(), "==null,null,"));
                            }
                        }

                        currentProperty.Append(".");
                        // append the last one:
                        currentProperty.Append(classes[classes.Length - 1]);
                    }

                    if (columnInfo.Virtual)
                    {
                        sbResult.Append("0");
                    }
                    else
                    {
                        //if (propertyType == typeof(Int32)) {
                        //    sbResult.Append(string.Concat("Int32? ", currentProperty));
                        //}
                        //else if (propertyType == typeof(Decimal)) {
                        //    sbResult.Append(string.Concat("Decimal? ", currentProperty));
                        //}
                        //else if (propertyType == typeof(DateTime)) {
                        //    sbResult.Append(string.Concat("DateTime? ", currentProperty));
                        //}
                        //else if (propertyType.IsEnum) {
                        //    sbResult.Append(string.Concat("Int32? ", currentProperty));
                        //}
                        //else {

                        sbResult.Append(currentProperty);
                        //}
                    }
                    // close brackets for all null checks for class path
                    if (classes.Length > 1)
                    {
                        sbResult.Append(')', classes.Length - 1);
                    }
                }

                if (!skip && columnInfo.DataType == ColumnDataType.CollectionSingle)
                {
                    sbResult.Append(".FirstOrDefault(");
                    if (columnInfo.AdditionalFiltersForCollection.Any())
                    {
                        for (var i = 0; i < columnInfo.AdditionalFiltersForCollection.Count(); i++)
                        {
                            if (i > 0)
                            {
                                sbResult.Append(" AND ");
                            }
                            var filter = columnInfo.AdditionalFiltersForCollection[i];
                            sbResult.Append(GetSearchCriterionForDynamicQuery(filter.ColumnInfo, filter.QueryLine).SQL);
                        }
                    }
                    sbResult.Append(").");
                    sbResult.Append(columnInfo.CollectionFilterColumn.ColumnName);
                }
            }

            if (!skip)
            {
                if (propertyType != null && propertyType.IsEnum)
                { // handle the sillyness of Enums:
                    sbResult.Append(string.Concat(" as ", columnInfo.ColumnName, "__Value")); // this will hold the integer value of the enum
                    sbResult.Append(string.Concat(", \"\" as ", columnInfo.ColumnName)); // we will replace this empty string with the display string for the enum
                }
                else
                {
                    sbResult.Append(string.Concat(" as ", string.IsNullOrEmpty(columnInfo.ColumnAlias) ? selectColumnName : columnInfo.ColumnAlias));
                }
            }

            return sbResult.ToString();
        }

        //public static string GetSelectColumns(IEntityHelper entityHelper, UserViewViewModel userView)
        //{
        //    return GetSelectColumns(entityHelper.EntityInfo, userView.SelectedColumnNames);
        //}

        public static string GetSelectColumns(EntityInfo entityInfo, List<string> selectedColumns, string additionalFields = "")
        {
            var useColumns = new List<ColumnInfo>();

            // build the dynamic select string
            var selectString = new StringBuilder();
            selectString.Append("new(1 as open_btn");

            // add fixed / always return / columns first
            foreach (var mc in entityInfo.ColumnDefinitions.Where(x => (x.Fixed || x.AlwaysReturnInResults) && !x.FilterOnly))
            {
                if (!string.IsNullOrEmpty(mc.ColumnName))
                {
                    useColumns.Add(mc);
                }
            }

            // add selected columns
            foreach (var colName in selectedColumns)
            {
                var mc = entityInfo.ColumnDefinitions.Where(x => x.ColumnName == colName && !x.Hidden).FirstOrDefault();
                if (mc != null && !useColumns.Contains(mc))
                {
                    useColumns.Add(mc);
                }
            }

            // convert to string
            foreach (var uc in useColumns)
            {
                if (!uc.Virtual)
                {
                    string stringColumnSelect = GenerateColumnSelect(entityInfo, uc);

                    if (!string.IsNullOrEmpty(stringColumnSelect))
                    {
                        selectString.Append(string.Concat(", ", stringColumnSelect));
                    }
                }
            }

            selectString.Append(additionalFields);

            selectString.Append(")");

            return selectString.ToString();
        }

        public static PropertyInfo GetNestedPropertyInfo(string propertyPath, Type type)
        {
            Type currentType = type;
            PropertyInfo returnPI = null;
            propertyPath = propertyPath.Replace(string.Concat(type.Name, "."), "");
            foreach (string part in propertyPath.Split('.'))
            {
                returnPI = currentType.GetProperty(part);

                if (returnPI == null)
                {
                    return null;
                }
                else
                {
                    currentType = returnPI.PropertyType;
                }
            }
            return returnPI;
        }

        public static string GetFullTextContainsQueryExpression(string keywords, List<string> noise, bool matchSimilar = false, KeywordMode keywordMode = KeywordMode.All)
        {
            var aterms = GetTerms(keywords, noise);

            bool lastTermWasPhrase = false;

            // strWHERE contains the boolean where clause and aterms contains the list of terms
            // the actual WHERE clause to use will depend on the search mode

            // keyword modes:
            // Indexing Server searches
            // all - search key1 key2 key3 use CONTAINS('"key1" AND "key2" AND "key3"') 
            // any - search key1 key2 key3 use CONTAINS('"key1" OR "key2" OR "key3"') 
            // exact - search key1 key2 key3 use CONTAINS('"key1 key2 key3"') 
            // boolean - search key1 AND key2 key3 -key4 use CONTAINS('"key1" AND "key2" AND "key3" AND NOT "key4"')
            // also supports NEAR and brackets ()

            string expression = "";
            switch (keywordMode)
            {
                case KeywordMode.All:
                    expression = "";
                    bool addAndNextTime = false;
                    foreach (var testTerm in aterms)
                    {
                        switch (testTerm.Trim().ToLower())
                        {
                            case "near":
                                expression += " NEAR ";
                                addAndNextTime = false;
                                break;
                            case "and":
                            case "+":
                                expression += " AND ";
                                addAndNextTime = false;
                                break;
                            case "or":
                                expression += " OR ";
                                addAndNextTime = false;
                                break;
                            case "not":
                            case "-":
                                if (addAndNextTime)
                                {
                                    expression += " AND ";
                                }
                                expression += " NOT ";
                                addAndNextTime = false;
                                break;
                            case "(":
                                if (addAndNextTime)
                                {
                                    expression += " AND ";
                                }
                                expression += "( ";
                                addAndNextTime = false;
                                break;
                            case ")":
                                expression += ") ";
                                addAndNextTime = true;
                                break;
                            default:
                                if (addAndNextTime)
                                {
                                    expression += " AND ";
                                }
                                if (matchSimilar && testTerm.IndexOf("*") == -1 && testTerm.IndexOf("[quoted]") == -1)
                                {
                                    expression += "FORMSOF(INFLECTIONAL,\"" + testTerm + "\") ";
                                }
                                else
                                {
                                    expression += " \"" + testTerm.Replace("[quoted]", "") + "\" ";
                                }
                                addAndNextTime = true;
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(expression))
                    {
                        expression = "'" + expression + "'";
                    }

                    break;

                case KeywordMode.Any:
                    foreach (var testTerm in aterms)
                    {
                        if (!Noise.Contains(testTerm.ToLower()) && testTerm.Length > 1)
                        {
                            // this is not a noise word
                            if (lastTermWasPhrase)
                            {
                                expression += " OR ";
                            }
                            if (matchSimilar && testTerm.IndexOf("*") == -1 && testTerm.IndexOf("[quoted]") == -1)
                            {
                                expression += "FORMSOF(INFLECTIONAL,\"" + testTerm + "\") ";
                            }
                            else
                            {
                                expression += " \"" + testTerm.Replace("[quoted]", "") + "\" ";
                            }
                            lastTermWasPhrase = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(expression))
                    {
                        expression = "'" + expression + "'";
                    }

                    break;
            }

            return expression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public static void RewriteFullTextQuery(DbCommand cmd, List<string> noise)
        {
            var text = cmd.CommandText;
            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                var parameter = cmd.Parameters[i];
                if (
                    !parameter.DbType.In(DbType.String, DbType.AnsiString, DbType.StringFixedLength,
                        DbType.AnsiStringFixedLength)) continue;
                if (parameter.Value == DBNull.Value)
                    continue;
                var value = (string)parameter.Value;
                if (value.IndexOf(FullTextPrefixes.ContainsPrefix, StringComparison.Ordinal) >= 0)
                {
                    parameter.Size = 4096;
                    parameter.DbType = DbType.AnsiStringFixedLength;
                    value = value.Replace(FullTextPrefixes.ContainsPrefix, ""); // remove prefix we added n linq query
                    value = value.Substring(1, value.Length - 2); // remove %% escaping by linq translator from string.Contains to sql LIKE
                    parameter.Value = value;
                    //cmd.CommandText = Regex.Replace(text,
                    //    string.Format(
                    //        @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}\s?(?:ESCAPE N?'~')", parameter.ParameterName),
                    //    string.Format(@"CONTAINS([$1].[$2], @{0})", parameter.ParameterName));

                    // replace command text with full text query statement
                    cmd.CommandText = Regex.Replace(text,
                        string.Format(
                            @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}\s?(?:ESCAPE N?'~')", parameter.ParameterName),
                            string.Format(@"CONTAINS([$1].[$2], {0})", GetFullTextContainsQueryExpression(keywords:value, noise:noise)));

                    // replace FileDataSearch with FileData in searches (as LINQ won't let you use Byte.Contains(string)

                    cmd.CommandText = cmd.CommandText.Replace("FileDataSearch", "FileData");

                    //PageLocker.Logging.AddLogFileEntry("fulltext", cmd.CommandText);

                    if (text == cmd.CommandText)
                        throw new Exception("FTS was not replaced on: " + text);
                    text = cmd.CommandText;
                }
                else if (value.IndexOf(FullTextPrefixes.FreetextPrefix, StringComparison.Ordinal) >= 0)
                {
                    parameter.Size = 4096;
                    parameter.DbType = DbType.AnsiStringFixedLength;
                    value = value.Replace(FullTextPrefixes.FreetextPrefix, ""); // remove prefix we added n linq query
                    value = value.Substring(1, value.Length - 2); // remove %% escaping by linq translator from string.Contains to sql LIKE
                    parameter.Value = value;
                    cmd.CommandText = Regex.Replace(text,
                        string.Format(
                            @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}\s?(?:ESCAPE N?'~')", parameter.ParameterName),
                        string.Format(@"FREETEXT([$1].[$2], @{0})", parameter.ParameterName));
                    if (text == cmd.CommandText)
                        throw new Exception("FTS was not replaced on: " + text);
                    text = cmd.CommandText;
                }
            }
        }

        public static UserViewVM GetDefaultUserView(EntityInfo entityInfo)
        {
            InitEntityInfo(entityInfo);

            var vm = new UserViewVM
            {
                Name = "Default view",
                EntityType = entityInfo.EntityType.Name,
                IsPublic = true,
                SelectedColumnNames = entityInfo.DefaultSelectedColumnNames,
                SortFields = entityInfo.SortColumns,
                Query = entityInfo.DefaultQuery,
                UserCanEdit = false
            };

            if (vm.SortFields.Count() > 0)
            {
                // transfer columns to grid state
                List<GridSortEntry> sortInfo = vm.SortFields.Select(x => new GridSortEntry { field = x.FieldName, dir = x.IsAscending ? "asc" : "desc" }).ToList();
                vm.GridState = JsonConvert.SerializeObject(new { sort = sortInfo.ToArray() });
            }

            // return a clone or we'll get into trouble when building queries from it
            var serialized = JsonConvert.SerializeObject(vm);

            return JsonConvert.DeserializeObject<UserViewVM>(serialized);
        }

        private static void InitEntityInfo(EntityInfo entityInfo)
        {
            foreach (var colInfo in entityInfo.ColumnDefinitions)
            {
                SetDynamicColumnInfoFields(colInfo);
            }

            if (entityInfo.DefaultSelectedColumnNames == null || entityInfo.DefaultSelectedColumnNames.Count == 0)
            {
                entityInfo.DefaultSelectedColumnNames = entityInfo.ColumnDefinitions.Where(x => x.Selected && !x.Hidden && !String.IsNullOrEmpty(x.ColumnName) && !x.Hidden).Select(x => x.ColumnName).ToList();
            }

            if (entityInfo.DefaultQuery == null)
            {
                entityInfo.DefaultQuery = new Query();
            }
        }

        private static void SetDynamicColumnInfoFields(ColumnInfo colInfo)
        {
            if (string.IsNullOrEmpty(colInfo.HeaderTitle))
            {
                colInfo.HeaderTitle = colInfo.Title;
            }
            if (string.IsNullOrEmpty(colInfo.ColumnAlias))
            {
                colInfo.ColumnAlias = colInfo.ColumnName;
            }
            if (string.IsNullOrEmpty(colInfo.FilterColumn))
            {
                colInfo.FilterColumn = colInfo.ColumnAlias;
            }
            if (colInfo.FilterColumnDataType == null)
            {
                colInfo.FilterColumnDataType = colInfo.DataType;
            }

            if (colInfo.CollectionFilterColumn != null && colInfo.CollectionFilterColumn != null)
            {
                SetDynamicColumnInfoFields(colInfo.CollectionFilterColumn);
            }
            if (colInfo.AdditionalFiltersForCollection != null && colInfo.AdditionalFiltersForCollection.Any())
            {
                foreach (var filter in colInfo.AdditionalFiltersForCollection)
                {
                    if (filter.ColumnInfo != null)
                    {
                        SetDynamicColumnInfoFields(filter.ColumnInfo);
                    }
                }
            }
        }

        public static PropertyInfo GetPropertyDeep(Type type, string propertyName)
        {
            var propertyNames = propertyName.Split(".");

            var property = type.GetProperty(propertyNames[0]);
            
            if (property != null && propertyNames.Length > 1)
            {
                return GetPropertyDeep(property.PropertyType, propertyName.Substring(propertyName.IndexOf(".") + 1));
            }
            else
            {
                return property;
            }
        }

    }
}
