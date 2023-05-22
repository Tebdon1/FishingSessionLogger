using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Sessions;

public class CreateUpdateSessionDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime SessionDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(128)]
    public string Venue { get; set; }

    [Required]
    public float Duration { get; set; }

    public virtual ICollection<CreateUpdateCatchSummaryDto> CatchSummaries { get; set; }
}
