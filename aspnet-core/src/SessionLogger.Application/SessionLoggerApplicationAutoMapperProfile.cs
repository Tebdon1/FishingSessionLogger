using AutoMapper;
using SessionLogger.Contracts.Search;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Files;
using SessionLogger.Domain.Folders;
using SessionLogger.Domain.Sessions;
using SessionLogger.Folders;
using SessionLogger.Search;
using SessionLogger.Domain.Tickets;
using SessionLogger.Domain.Venues;
using System.Collections.Generic;
using System.Linq;
using SessionLogger.Sessions;
using SessionLogger.Baits;
using SessionLogger.Tickets;
using SessionLogger.Venues;
using SessionLogger.Domain.Catches;
using SessionLogger.Catches;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.SpeciesTypes;

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

        CreateMap<Catch, CatchDto>();
        CreateMap<CreateUpdateCatchDto, Catch>()
            .ForMember(dest => dest.Photo, opt =>
            {
                // PhotoData is only set when the caller wants to attach/replace a photo;
                // null means "leave the existing photo as-is" (matters for updates).
                opt.Condition(src => src.PhotoData != null);
                opt.MapFrom(src => new File
                {
                    FileData = src.PhotoData,
                    FileName = src.PhotoFileName,
                    Extension = System.IO.Path.GetExtension(src.PhotoFileName),
                    Size = src.PhotoData.Length
                });
            });

        CreateMap<Bait, BaitDto>();
        CreateMap<BaitUpdateDto, Bait>();

        CreateMap<Ticket, TicketDto>();
        CreateMap<TicketUpdateDto, Ticket>();

        CreateMap<Venue, VenueDto>();
        CreateMap<VenueUpdateDto, Venue>();

        CreateMap<Species, SpeciesDto>();
        CreateMap<SpeciesUpdateDto, Species>();

    }
}
