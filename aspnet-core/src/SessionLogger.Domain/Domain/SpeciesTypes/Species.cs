using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.SpeciesTypes;

public class Species : FullAuditedAggregateRoot<int>
{
    public Species(int id) : base(id)
    {
    }

    public Species()
    {
    }

    [Required]
    public string? Name { get; set; }
    public SpeciesWaterType WaterType { get; set; }

    // The default tile image for this species, set by an admin - overridden per-user
    // by a catch photo when the user has their own personal best for this species
    // (see PersonalBestAppService), falling back further to a generic image/icon on
    // the frontend when neither exists.
    //
    // Not loaded unless asked for (Include(x => x.Photo)) - see SpeciesPhoto for why.
    public virtual SpeciesPhoto? Photo { get; set; }
}
