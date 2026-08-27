using System.ComponentModel.DataAnnotations;
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
