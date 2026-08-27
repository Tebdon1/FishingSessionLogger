using System.ComponentModel.DataAnnotations;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Rigs;

namespace SessionLogger.Rigs;

public class RigUpdateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    // Input-time value + unit; converted server-side to the canonical LengthMm, same
    // approach as Bait's SizeValue/SizeUnit. Defaults to inches - hooklink length is
    // conventionally quoted that way (e.g. "6 inch rig").
    [Range(0.01, 100000)]
    public decimal? LengthValue { get; set; }
    public SizeUnit LengthUnit { get; set; } = SizeUnit.Inches;

    [MaxLength(50)]
    public string? HookSize { get; set; }

    // Input-time value + unit; converted server-side to the canonical HookWeightG.
    // Defaults to grams - jighead weight is virtually always quoted that way.
    [Range(0.01, 100000)]
    public decimal? HookWeightValue { get; set; }
    public HookWeightUnit HookWeightUnit { get; set; } = HookWeightUnit.Grams;

    [MaxLength(100)]
    public string? HookPattern { get; set; }

    [MaxLength(255)]
    public string? Materials { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
