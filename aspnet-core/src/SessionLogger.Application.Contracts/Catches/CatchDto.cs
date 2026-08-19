using System;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Catches;

public class CatchDto : AuditedEntityDto<int>
{
    public int SessionId { get; set; }

    public int SpeciesId { get; set; }
    public string SpeciesName { get; set; }

    public int? BaitId { get; set; }
    public string BaitName { get; set; }

    public float? Weight { get; set; }

    public int? PhotoId { get; set; }
    public string PhotoFileName { get; set; }
}
