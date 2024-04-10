using System;
using System.Collections.Generic;
using System.Text;

namespace SessionLogger.RemoteWebAPI
{
    public class WebAPIResult<T> : WebAPIResult
    {
        public WebAPIResult() : base() { }
        public T Data { get; set; }
    }

    public class WebAPIResult
    {
        public WebAPIResult() { }

        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }

}
