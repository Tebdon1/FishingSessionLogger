using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Venues;

public class VenueUpdateDto
{
    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(8)]
    public string? Postcode { get; set; }

    public int TicketId { get; set; }
}
