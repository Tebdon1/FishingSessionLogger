using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.SpeciesTypes;

public class SpeciesDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
    public bool IsSaltwater { get; set; }
}
