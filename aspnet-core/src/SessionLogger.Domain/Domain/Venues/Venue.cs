using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using SessionLogger.Domain.Tickets;

namespace SessionLogger.Domain.Venues;

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

    public int? TicketId { get; set; }

    [ForeignKey("TicketId")]
    public virtual Ticket? Ticket { get; set; }

    public WaterType? WaterType { get; set; }

}
