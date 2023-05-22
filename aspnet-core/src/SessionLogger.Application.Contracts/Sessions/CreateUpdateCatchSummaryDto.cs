using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Sessions;

public class CreateUpdateCatchSummaryDto
{
    [Required]
    public int Quantity { get; set; } = 1;

    [Required]
    public SpeciesType Species { get; set; }

    public virtual ICollection<CreateUpdateCatchDetailDto> CatchDetails { get; set; }
}
