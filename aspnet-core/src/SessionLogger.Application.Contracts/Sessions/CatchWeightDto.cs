using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Sessions;

public class CatchWeightDto : AuditedEntityDto<int>
{
    public float Weight { get; set; }
}
