using System.ComponentModel.DataAnnotations;
using SessionLogger.Domain.Baits;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Rigs;

public class Rig : AuditedAggregateRoot<int>
{
    public Rig(int id) : base(id)
    {
    }

    public Rig()
    {
    }

    // The rig pattern name, e.g. "Ronnie Rig" - free-typed, not computed from the
    // fields below (unlike Bait.Name, there's no natural composition for this one).
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    // Always millimetres, converted from whatever unit the caller entered - so every
    // comparison/aggregation is a plain numeric one, with no unit-conversion logic
    // needed at query time (same approach as Bait.SizeMm / Catch.LengthMm).
    public decimal? LengthMm { get; set; }

    // The unit LengthMm was originally entered in, kept purely for display - never
    // used for comparisons. Null for rows created before this field existed.
    public SizeUnit? LengthUnit { get; set; }

    // Free text, not numeric - hook sizes aren't purely numeric (aught sizes like
    // "1/0", "2/0" get larger as the number grows, the opposite of standard sizes).
    // Jighead weight is a separate field (HookWeightG) rather than folded in here, so
    // it stays comparable instead of being buried inside an arbitrary string.
    [MaxLength(50)]
    public string? HookSize { get; set; }

    // Jighead weight - always grams, converted from whatever unit the caller entered,
    // same approach as LengthMm. Null for hook-only rigs with no lead.
    public decimal? HookWeightG { get; set; }

    // The unit HookWeightG was originally entered in, kept purely for display - never
    // used for comparisons.
    public HookWeightUnit? HookWeightUnit { get; set; }

    [MaxLength(100)]
    public string? HookPattern { get; set; }

    [MaxLength(255)]
    public string? Materials { get; set; }

    // Free-form extras that don't fit a structured field, e.g. hair position, tweaks
    // to a standard pattern - same idea as Catch.Notes.
    public string? Notes { get; set; }
}
