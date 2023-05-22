using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Sessions;

public class CatchSummary : AuditedAggregateRoot<int>
{
    public CatchSummary(int id) : base(id)
    {

    }
    

    public CatchSummary()
    {
        this.CatchDetails = new HashSet<CatchDetail>();
    }
    public virtual ICollection<CatchDetail> CatchDetails { get; set; }

    public SpeciesType Species { get; set; }
    
    public int Quantity { get; set; }

    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    
    public virtual Session Session { get; set; }
}
