using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Sessions;

public class CreateUpdateCatchDetailDto
{
    [StringLength(128)]
    public string Bait { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<CreateUpdateCatchWeightDto> CatchWeights { get; set; }
}
