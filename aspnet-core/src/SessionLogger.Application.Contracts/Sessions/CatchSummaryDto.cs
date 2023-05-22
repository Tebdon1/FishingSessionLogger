using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Sessions;

public class CatchSummaryDto : AuditedEntityDto<int>
{
    public SpeciesType Species { get; set; }
    
    public int Quantity { get; set; }

    public virtual ICollection<CatchDetailDto> CatchDetails { get; set; }
}
