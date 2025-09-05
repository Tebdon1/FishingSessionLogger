using SessionLogger.Domain.Sessions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.SpeciesTypes;

public class Species : AuditedAggregateRoot<int>
{
    public Species(int id) : base(id)
    {
    }

    public Species()
    {
    }

    [Required]
    public string? Name { get; set; }
    public bool IsSaltwater { get; set; }
}
