using SessionLogger.Search;
using SessionLogger.GridData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Domain.Sessions
{
    public interface ISessionRepository : ISearchableRepository<Session>
    {

    }
}
