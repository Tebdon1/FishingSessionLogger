using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Sessions;

public class CatchWeight : AuditedAggregateRoot<int>
{
    public CatchWeight(int id) : base(id)
    {
    }

    public CatchWeight()
    {

    }

    public float Weight { get; set; }

    public int CatchDetailId { get; set; }

    [ForeignKey("CatchDetailId")]

    public virtual CatchDetail CatchDetail { get; set; }
}
