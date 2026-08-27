using System;
using System.Collections.Generic;
using System.Text;
using SessionLogger.Domain.Venues;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Venues;

public class VenueDto : AuditedEntityDto<int>
{

    public string Name { get; set; }

    public string Postcode { get; set; }

    public int? TicketId { get; set; }

    public string TicketName { get; set; }

    public WaterType? WaterType { get; set; }
}
