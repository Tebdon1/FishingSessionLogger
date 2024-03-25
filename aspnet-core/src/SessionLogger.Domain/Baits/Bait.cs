using SessionLogger.Sessions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Baits
{
    public class Bait : AuditedAggregateRoot<int>

    {

        public Bait(int id) : base(id)
        {

        }

        public Bait()
        {

        }

        [MaxLength(255)]
        public string? Name { get; set; }

    }
}
