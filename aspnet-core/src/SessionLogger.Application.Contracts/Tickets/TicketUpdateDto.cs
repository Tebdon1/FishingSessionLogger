using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Tickets;

public class TicketUpdateDto
{
    [MaxLength(255)]
    public string Name { get; set; }
}
