using System;
using System.Collections.Generic;
using System.Text;

namespace SessionLogger.Folders
{
    public class SaveResult<TReadDto>
    {
        public SaveResult()
        {
            Errors = new List<string>();
        }

        public bool Error { get; set; }
        public List<string> Errors { get; set; }
        public TReadDto Value { get; set; }
    }
}
