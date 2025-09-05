using SessionLogger.Domain.Sessions;
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
    [StringLength(128)]
    public string Venue { get; set; }

    [Required]
    [StringLength(100)]
    public string Species { get; set; }

    [Required]
    public float Weight { get; set; }

    [Required]
    [StringLength(100)]
    public string Bait { get; set; }
}
