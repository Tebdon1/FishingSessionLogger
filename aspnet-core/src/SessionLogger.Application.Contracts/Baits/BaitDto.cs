using SessionLogger.Domain.Baits;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Baits;

public class BaitDto : AuditedEntityDto<int>
{
    public string Name { get; set; }

    public BaitType BaitType { get; set; }

    public string? Brand { get; set; }
    public string? Range { get; set; }
    public string? Colour { get; set; }
    public string? Flavour { get; set; }
    public decimal? SizeMm { get; set; }
    public SizeUnit? SizeUnit { get; set; }
}
