using System;
using System.Collections.Generic;
using System.Text;
using SessionLogger.Domain.SpeciesTypes;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.SpeciesTypes;

public class SpeciesDto : AuditedEntityDto<int>
{
    public string Name { get; set; }
    public SpeciesWaterType WaterType { get; set; }

    // The photo itself is fetched separately (GET /api/app/species-photo/{id}) - this
    // just tells the frontend whether it's worth asking for one.
    public bool HasPhoto { get; set; }
}
