using Microsoft.Extensions.FileSystemGlobbing;
using SessionLogger.Domain.Catches;
using SessionLogger.Search;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Sessions;

public class Session : AuditedAggregateRoot<int>, IItem
{
    public Session(int id) : base(id)
    {
    }

    public Session()
    {
        Catches = new HashSet<Catch>();
    }

    public DateTime SessionDate { get; set; }

    public string Venue { get; set; }

    public float Duration { get; set; }

    public virtual ICollection<Catch> Catches { get; set; }

}
