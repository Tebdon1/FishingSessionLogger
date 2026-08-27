using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Rigs;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Rigs;

public class RigDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
    public decimal? LengthMm { get; set; }
    public SizeUnit? LengthUnit { get; set; }
    public string? HookSize { get; set; }
    public decimal? HookWeightG { get; set; }
    public HookWeightUnit? HookWeightUnit { get; set; }
    public string? HookPattern { get; set; }
    public string? Materials { get; set; }
    public string? Notes { get; set; }
}
