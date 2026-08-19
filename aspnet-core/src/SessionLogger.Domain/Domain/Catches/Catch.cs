using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Files;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.SpeciesTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Catches;

public class Catch : AuditedAggregateRoot<int>
{
    public Catch(int id) : base(id)
    {
    }

    public Catch()
    {
    }

    [Required]
    public int SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    [Required]
    public int SpeciesId { get; set; }

    [ForeignKey(nameof(SpeciesId))]
    public virtual Species Species { get; set; }

    public int? BaitId { get; set; }

    [ForeignKey(nameof(BaitId))]
    public virtual Bait? Bait { get; set; }

    // Not every fish gets weighed, e.g. bulk-logged catches
    public float? Weight { get; set; }

    // One optional photo per catch, owned by the catch (see File.CatchId)
    public virtual File? Photo { get; set; }
}
