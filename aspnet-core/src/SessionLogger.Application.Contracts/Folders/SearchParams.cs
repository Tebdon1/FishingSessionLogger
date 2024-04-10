using SessionLogger.Contracts.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace SessionLogger.Folders
{
    public class SearchParams
    {
        public UserViewVM userView { get; set; }
        public bool idsOnly { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }
}
