using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Venues;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Sessions;

public class Session : AuditedAggregateRoot<int>
{
    public Session(int id) : base(id)
    {
    }

    public Session()
    {
        Catches = new HashSet<Catch>();
    }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int VenueId { get; set; }

    [ForeignKey(nameof(VenueId))]
    public virtual Venue Venue { get; set; }

    public string? Notes { get; set; }

    public float Duration => (float)(EndDateTime - StartDateTime).TotalHours;

    public virtual ICollection<Catch> Catches { get; set; }

}
