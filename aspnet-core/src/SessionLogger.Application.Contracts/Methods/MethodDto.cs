using Volo.Abp.Application.Dtos;

namespace SessionLogger.Methods;

public class MethodDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
}
