using System;
using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Catches;

public class CreateUpdateCatchDto
{
    [Required]
    public int SessionId { get; set; }

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
