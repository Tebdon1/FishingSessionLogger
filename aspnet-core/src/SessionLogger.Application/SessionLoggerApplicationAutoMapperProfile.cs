using AutoMapper;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Files;
using SessionLogger.Domain.Sessions;
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
using SessionLogger.Domain.Methods;
using SessionLogger.Methods;
using SessionLogger.Domain.Rigs;
using SessionLogger.Rigs;

namespace SessionLogger;

public class SessionLoggerApplicationAutoMapperProfile : Profile
{
    public SessionLoggerApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<Session, SessionDto>();
        CreateMap<CreateUpdateSessionDto, Session>()
            // Catches are reconciled by id in SessionAppService, not blindly replaced -
            // AutoMapper's default list-mapping would otherwise delete and recreate every
            // catch (and cascade-delete their photos) on every session update.
            .ForMember(dest => dest.Catches, opt => opt.Ignore());

        CreateMap<Catch, CatchDto>();
        CreateMap<CreateUpdateCatchDto, Catch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // Computed in SessionAppService.ReconcileCatches from LengthValue/LengthUnit -
            // see ToMillimetres there (same approach as BaitAppService.ToMillimetres).
            .ForMember(dest => dest.LengthMm, opt => opt.Ignore())
            .ForMember(dest => dest.LengthUnit, opt => opt.Ignore())
            // Computed in SessionAppService.ReconcileCatches from WeightLbs/WeightOz -
            // see ToGrams there (same approach as LengthMm above).
            .ForMember(dest => dest.WeightG, opt => opt.Ignore())
            .ForMember(dest => dest.WeightUnit, opt => opt.Ignore())
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
                    Size = src.PhotoData.Length,
                    // Not a text document - nothing to feed the full-text search index.
                    FileDataSearch = string.Empty
                });
            });

        CreateMap<Bait, BaitDto>();
        CreateMap<BaitUpdateDto, Bait>()
            // Both computed in BaitAppService from the other fields - see ComputeName/ToMillimetres.
            .ForMember(dest => dest.Name, opt => opt.Ignore())
            .ForMember(dest => dest.SizeMm, opt => opt.Ignore());

        CreateMap<Ticket, TicketDto>();
        CreateMap<TicketUpdateDto, Ticket>();

        CreateMap<Venue, VenueDto>();
        CreateMap<VenueUpdateDto, Venue>();

        CreateMap<Species, SpeciesDto>()
            .ForMember(dest => dest.HasPhoto, opt => opt.MapFrom(src => src.PhotoData != null));
        CreateMap<SpeciesUpdateDto, Species>()
            // Handled in SpeciesAppService.ApplyPhoto instead - a null PhotoData here
            // means "unchanged", not "clear it", which a direct automap can't express.
            .ForMember(dest => dest.PhotoData, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoFileName, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoExtension, opt => opt.Ignore());

        CreateMap<Method, MethodDto>();
        CreateMap<MethodUpdateDto, Method>();

        CreateMap<Rig, RigDto>();
        CreateMap<RigUpdateDto, Rig>()
            // Computed in RigAppService from LengthValue/LengthUnit - see ToMillimetres there.
            .ForMember(dest => dest.LengthMm, opt => opt.Ignore())
            // Computed in RigAppService from HookWeightValue/HookWeightUnit - see ToGrams there.
            .ForMember(dest => dest.HookWeightG, opt => opt.Ignore());

    }
}
