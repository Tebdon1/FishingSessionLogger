using System;
using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Catches;

public class CreateUpdateCatchDto
{
    [Required]
    public int SessionId { get; set; }

    [Required]
    public int SpeciesId { get; set; }

    public int? BaitId { get; set; }

    public float? Weight { get; set; }

    // Set to attach/replace the catch's photo; left null to leave it unchanged.
    public byte[]? PhotoData { get; set; }
    public string? PhotoFileName { get; set; }
}
