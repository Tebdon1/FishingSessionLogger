using AutoMapper;
using SessionLogger.Contracts.Search;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Folders;
using SessionLogger.Domain.Sessions;
using SessionLogger.Folders;
using SessionLogger.Search;
using SessionLogger.Domain.Tickets;
using SessionLogger.Domain.Venues;
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
        CreateMap<EntityInfo, FolderInfoVM>();

        CreateMap<UserViewVM, UserView>();
        CreateMap<UserView, UserViewVM>();

        CreateMap<Session, SessionDto>();
        CreateMap<CreateUpdateSessionDto, Session>();

        CreateMap<CatchSummary, CatchSummaryDto>();
        CreateMap<CreateUpdateCatchSummaryDto, CatchSummary>();

        CreateMap<CatchDetail, CatchDetailDto>();
        CreateMap<CreateUpdateCatchDetailDto, CatchDetail>();

        CreateMap<CatchWeight, CatchWeightDto>();
        CreateMap<CreateUpdateCatchWeightDto, CatchWeight>();

        CreateMap<Bait, BaitDto>();
        CreateMap<BaitUpdateDto, Bait>();

        CreateMap<Ticket, TicketDto>();
        CreateMap<TicketUpdateDto, Ticket>();

        CreateMap<Venue, VenueDto>();
        CreateMap<VenueUpdateDto, Venue>();

    }
}
