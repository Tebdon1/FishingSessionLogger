using SessionLogger.Catches;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Sessions;

public class SessionDto : AuditedEntityDto<int>
{
    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int VenueId { get; set; }
    public string VenueName { get; set; }

    public string Notes { get; set; }

    public bool IsBlank { get; set; }

    // The species being targeted; empty means "Any". Names are filled in by
    // SessionAppService.ApplyDisplayNamesAsync, alongside the catches' species names.
    public ICollection<int> TargetSpeciesIds { get; set; } = new List<int>();
    public ICollection<string> TargetSpeciesNames { get; set; } = new List<string>();

    // Duration in hours, calculated from start and end times
    public float Duration => (float)(EndDateTime - StartDateTime).TotalHours;

    public virtual ICollection<CatchDto> Catches { get; set; }
}
