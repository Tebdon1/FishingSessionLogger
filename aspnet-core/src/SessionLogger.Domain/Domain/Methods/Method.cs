using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Methods;

public class Method : AuditedAggregateRoot<int>
{
    public Method(int id) : base(id)
    {
    }

    public Method()
    {
    }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
}
