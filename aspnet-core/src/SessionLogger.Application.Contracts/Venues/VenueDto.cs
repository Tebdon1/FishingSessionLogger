using System;
using System.Collections.Generic;
using System.Text;
using SessionLogger.Domain.Venues;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Venues;

public class VenueDto : AuditedEntityDto<int>
{

    // The venue's own name, as entered - what the edit form binds to.
    public string Name { get; set; }

    // What to show wherever the venue is picked or listed by name: the same as Name
    // unless another of the user's venues shares it, in which case the ticket is
    // appended (see VenueLabels).
    public string DisplayName { get; set; }

    public string Postcode { get; set; }

    public int? TicketId { get; set; }

    public string TicketName { get; set; }

    public WaterType? WaterType { get; set; }
}
