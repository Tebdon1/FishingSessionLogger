using System;
using System.ComponentModel.DataAnnotations;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;

namespace SessionLogger.Catches;

public class CreateUpdateCatchDto
{
    // Set when updating an existing catch; left null for a new one.
    public int? Id { get; set; }

    [Required]
    public int SessionId { get; set; }

    [Required]
    public int SpeciesId { get; set; }

    public int? BaitId { get; set; }

    // Input-time value(s) + unit; converted server-side to the canonical WeightG,
    // same approach as LengthValue/LengthUnit. WeightLbs/WeightOz are used for the
    // compound LbOz unit (UK anglers conventionally write weight that way, e.g.
    // "16lb 4oz"); WeightValue is used for the single-value Kilograms/Grams units.
    public WeightUnit WeightUnit { get; set; } = WeightUnit.LbOz;

    [Range(0, 9999)]
    public int? WeightLbs { get; set; }

    [Range(0, 15)]
    public int? WeightOz { get; set; }

    [Range(0.001, 100000)]
    public decimal? WeightValue { get; set; }

    public int? MethodId { get; set; }

    // Independent of Method - the same rig can be fished under different methods.
    public int? RigId { get; set; }

    // Input-time value + unit; converted server-side to the canonical LengthMm,
    // same approach as Bait's SizeValue/SizeUnit.
    [Range(0.01, 100000)]
    public decimal? LengthValue { get; set; }
    public SizeUnit LengthUnit { get; set; } = SizeUnit.Centimetres;

    // TimeOnly isn't available on netstandard2.0 (this project's other target
    // framework) - TimeSpan carries the same time-of-day value across the wire.
    public TimeSpan? CatchTime { get; set; }

    // Set to attach/replace the catch's photo; left null to leave it unchanged.
    // Capped well below Kestrel's default request body limit so an oversized upload
    // fails with a clean validation error instead of an obscure connection failure.
    [MaxLength(MaxPhotoBytes)]
    public byte[]? PhotoData { get; set; }

    [MaxLength(255)]
    public string? PhotoFileName { get; set; }

    // Set to delete the catch's existing photo without replacing it. Ignored if
    // PhotoData is also set (attaching a new photo takes precedence).
    public bool RemovePhoto { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public const int MaxPhotoBytes = 8 * 1024 * 1024;
}
