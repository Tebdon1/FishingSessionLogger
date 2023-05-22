using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Sessions;

public class CatchDetailDto : AuditedEntityDto<int>
{
    public string Bait { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<CatchWeightDto> CatchWeights { get; set; }
}
