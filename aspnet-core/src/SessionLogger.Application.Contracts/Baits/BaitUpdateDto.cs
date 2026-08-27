using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SessionLogger.Domain.Baits;

namespace SessionLogger.Baits;

public class BaitUpdateDto : IValidatableObject
{
    [Required]
    public BaitType BaitType { get; set; }

    // Natural only - the display name is used as-is, nothing to build it from.
    [MaxLength(255)]
    public string? Name { get; set; }

    // Lure and Bait only - Name is built from these server-side.
    [MaxLength(255)]
    public string? Brand { get; set; }

    [MaxLength(255)]
    public string? Range { get; set; }

    [MaxLength(100)]
    public string? Colour { get; set; }

    [MaxLength(100)]
    public string? Flavour { get; set; }

    // Lure and Bait only - converted to Bait.SizeMm server-side. Leave both null to
    // record no size.
    [Range(0.01, 100000)]
    public decimal? SizeValue { get; set; }

    public SizeUnit SizeUnit { get; set; } = SizeUnit.Millimetres;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        switch (BaitType)
        {
            case BaitType.Natural:
                if (string.IsNullOrWhiteSpace(Name))
                {
                    yield return new ValidationResult("Name is required for a natural bait.", new[] { nameof(Name) });
                }
                break;

            case BaitType.Lure:
            case BaitType.Bait:
                if (string.IsNullOrWhiteSpace(Brand))
                {
                    yield return new ValidationResult("Brand is required.", new[] { nameof(Brand) });
                }
                break;
        }
    }
}
