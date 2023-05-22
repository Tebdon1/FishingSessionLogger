using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Sessions;

public class SessionDto : AuditedEntityDto<int>
{
    public DateTime SessionDate { get; set; }

    public string Venue { get; set; }

    public float Duration { get; set; }

    public virtual ICollection<CatchSummaryDto> CatchSummaries { get; set; }

}
