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
        CatchSummaries = new HashSet<CatchSummary>();
    }

    public DateTime SessionDate { get; set; }

    public string Venue { get; set; }

    public float Duration { get; set; }

    public virtual ICollection<CatchSummary> CatchSummaries { get; set; }

}
