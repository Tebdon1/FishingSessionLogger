using SessionLogger.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Venues
{
    public class Venue : AuditedAggregateRoot<int>

    {

        public Venue(int id) : base(id)
        {

        }

        public Venue()
        {

        }

        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(8)]
        public string? Postcode { get; set; }

        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }


    }
}
