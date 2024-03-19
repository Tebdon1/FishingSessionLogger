using AutoMapper;
using SessionLogger.Sessions;
using System.Collections.Generic;
using System.Linq;

namespace SessionLogger;

public class SessionLoggerApplicationAutoMapperProfile : Profile
{
    public SessionLoggerApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        //CreateMap<Session, SessionDto>();
        CreateMap<Session, SessionDto>();
            ;
        CreateMap<CreateUpdateSessionDto, Session>();

        CreateMap<CatchSummary, CatchSummaryDto>();
        CreateMap<CreateUpdateCatchSummaryDto, CatchSummary>();

        CreateMap<CatchDetail, CatchDetailDto>();
        CreateMap<CreateUpdateCatchDetailDto, CatchDetail>();

        CreateMap<CatchWeight, CatchWeightDto>();
        CreateMap<CreateUpdateCatchWeightDto, CatchWeight>();
    }
}
