using SessionLogger.Sessions;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Weathers
{
    public class WeatherDto : AuditedEntityDto<int>
    {
        public string? WeatherType { get; set; }

    }
}
