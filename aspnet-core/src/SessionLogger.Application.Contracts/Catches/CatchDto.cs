using System;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Catches;

public class CatchDto : AuditedEntityDto<int>
{
    public int SessionId { get; set; }
    public string Venue { get; set; }
    public string Species { get; set; }
    public float Weight { get; set; }
    public string Bait { get; set; }
}
