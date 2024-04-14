using SessionLogger.Domain.Sessions;
using SessionLogger.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Baits;

public class Bait : AuditedAggregateRoot<int>, IItem
{
    public Bait(int id) : base(id)
    {

    }

    public Bait()
    {

    }

    public string Name { get; set; }

}
