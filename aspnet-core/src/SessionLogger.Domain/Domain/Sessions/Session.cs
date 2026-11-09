using SessionLogger.Domain.Catches;
using SessionLogger.Domain.SpeciesTypes;
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
        TargetSpecies = new HashSet<Species>();
    }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int VenueId { get; set; }

    [ForeignKey(nameof(VenueId))]
    public virtual Venue Venue { get; set; }

    public string? Notes { get; set; }

    // Set by SessionAppService whenever catches are saved - true whenever the session
    // ends up with zero catches. Not a client-supplied input: the angler shouldn't have
    // to separately declare a blank, the absence of any catch already says it.
    public bool IsBlank { get; set; }

    public float Duration => (float)(EndDateTime - StartDateTime).TotalHours;

    public virtual ICollection<Catch> Catches { get; set; }

    // What the angler set out to catch. Empty means "Any" - no particular target - so
    // there's no sentinel species row for it, and a session can name several.
    public virtual ICollection<Species> TargetSpecies { get; set; }

}
