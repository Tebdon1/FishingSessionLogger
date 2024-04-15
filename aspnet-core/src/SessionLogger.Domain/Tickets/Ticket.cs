using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Tickets
{
    public class Ticket : AuditedAggregateRoot<int>

    {

        public Ticket(int id) : base(id)
        {

        }

        public Ticket()
        {

        }

        [MaxLength(255)]
        public string? Name { get; set; }

    }
}
