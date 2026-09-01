using SessionLogger.Catches;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Sessions;

public class CreateUpdateSessionDto
{
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime StartDateTime { get; set; } = DateTime.Now;
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EndDateTime { get; set; } = DateTime.Now;

    [Required]
    public int VenueId { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    // IsBlank isn't here - it's derived server-side in SessionAppService from whether
    // Catches ends up empty, not supplied by the client.

    // Duration is automatically calculated from StartDateTime and EndDateTime

    public virtual ICollection<CreateUpdateCatchDto> Catches { get; set; }
}
