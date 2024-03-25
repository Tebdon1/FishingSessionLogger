using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Venues;

public class VenueDto : AuditedEntityDto<int>
{
    public string? Name { get; set; }

    public string? Postcode { get; set; }

    public int TicketId { get; set; }
}

