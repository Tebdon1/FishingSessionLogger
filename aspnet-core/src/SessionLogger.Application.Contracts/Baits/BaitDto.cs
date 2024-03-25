using SessionLogger.Sessions;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Baits
{
    public class BaitDto : AuditedEntityDto<int>
    {
        public string? Name { get; set; }

    }
}
