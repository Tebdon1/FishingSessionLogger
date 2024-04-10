using SessionLogger.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Domain.Venues
{
    public interface IVenueRepository : ISearchableRepository<Venue>
    {

    }
}
