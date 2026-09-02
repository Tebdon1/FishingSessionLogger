using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using SessionLogger.Domain.SpeciesTypes;

namespace SessionLogger.SpeciesTypes;

public class SpeciesUpdateDto
{
    [MaxLength(255)]
    public string Name { get; set; }
    public SpeciesWaterType WaterType { get; set; }

    // Set to attach/replace the species' default photo; left null to leave it
    // unchanged. Capped well below Kestrel's default request body limit so an
    // oversized upload fails with a clean validation error instead of an obscure
    // connection failure - same approach as CreateUpdateCatchDto.PhotoData.
    [MaxLength(MaxPhotoBytes)]
    public byte[]? PhotoData { get; set; }

    [MaxLength(255)]
    public string? PhotoFileName { get; set; }

    // Set to delete the species' existing photo without replacing it. Ignored if
    // PhotoData is also set (attaching a new photo takes precedence).
    public bool RemovePhoto { get; set; }

    public const int MaxPhotoBytes = 8 * 1024 * 1024;
}
