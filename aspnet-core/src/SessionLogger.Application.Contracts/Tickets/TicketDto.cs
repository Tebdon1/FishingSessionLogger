using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Tickets;

public class TicketDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
}
