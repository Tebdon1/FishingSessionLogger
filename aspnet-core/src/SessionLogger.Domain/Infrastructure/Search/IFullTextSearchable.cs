using SessionLogger.Domain.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Infrastructure.Search
{
    public interface IFullTextSearchable
    {
        public ICollection<File> Files { get; set; }
    }
}
