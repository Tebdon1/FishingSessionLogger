using System;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Catches;

public class CatchDto : AuditedEntityDto<int>
{
    public int SessionId { get; set; }

    public int SpeciesId { get; set; }
    public string SpeciesName { get; set; }

    public int? BaitId { get; set; }
    public string BaitName { get; set; }

    public decimal? WeightG { get; set; }
    public WeightUnit? WeightUnit { get; set; }

    public int? MethodId { get; set; }
    public string MethodName { get; set; }

    public int? RigId { get; set; }
    public string RigName { get; set; }

    public decimal? LengthMm { get; set; }
    public SizeUnit? LengthUnit { get; set; }

    // TimeOnly isn't available on netstandard2.0 (this project's other target
    // framework) - TimeSpan carries the same time-of-day value across the wire.
    public TimeSpan? CatchTime { get; set; }

    public int? PhotoId { get; set; }
    public string PhotoFileName { get; set; }

    public string Notes { get; set; }
}
