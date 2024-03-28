using SessionLogger.Domain.Sessions;
using SessionLogger.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Venues;

public class Venue : AuditedAggregateRoot<int>, IItem
{
    public Venue(int id) : base(id)
    {

    }

    public Venue()
    {

    }

    public string Name { get; set; }

    public string Postcode { get; set; }


}
