using SessionLogger.Contracts.Search;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionLogger.Search
{
    public interface INoiseService
    {
        public List<string> Noise();
    }
}
