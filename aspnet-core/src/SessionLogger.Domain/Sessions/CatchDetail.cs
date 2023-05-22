using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Sessions;

public class CatchDetail : AuditedAggregateRoot<int>

{

    public CatchDetail(int id) : base(id)
    {

    }

    public CatchDetail()
    {
        this.CatchWeights = new HashSet<CatchWeight>();
    }

    public virtual ICollection<CatchWeight> CatchWeights { get; set; }

    public string Bait { get; set; }

    public int Quantity { get; set; }

    public int CatchSummaryId { get; set; }

    [ForeignKey("CatchSummaryId")]

    public virtual CatchSummary CatchSummary { get; set; }
}
