using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Sessions;

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
