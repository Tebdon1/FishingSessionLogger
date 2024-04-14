using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Search
{
    public static class FullTextPrefixes
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ContainsPrefix = "-CONTAINS-";

        /// <summary>
        /// 
        /// </summary>
        public const string FreetextPrefix = "-FREETEXT-";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static string Contains(string searchTerm)
        {
            return string.Format("({0}{1})", ContainsPrefix, searchTerm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static string Freetext(string searchTerm)
        {
            return string.Format("({0}{1})", FreetextPrefix, searchTerm);
        }

    }
}
