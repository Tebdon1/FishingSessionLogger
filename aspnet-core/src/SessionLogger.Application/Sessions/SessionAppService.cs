using Microsoft.EntityFrameworkCore;
using SessionLogger.Catches;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Domain.Venues;
using SessionLogger.Permissions;
using SessionLogger.Venues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using FileEntity = SessionLogger.Domain.Files.File;

namespace SessionLogger.Sessions;

// Sessions are personal: each user only sees and manages their own.
public class SessionAppService :
    CrudAppService<
        Session,
        SessionDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateSessionDto>,
    ISessionAppService
{
    private readonly IRepository<FileEntity, int> _fileRepository;
    private readonly IRepository<Venue, int> _venueRepository;
    private readonly IRepository<Species, int> _speciesRepository;

    public SessionAppService(
        IRepository<Session, int> repository,
        IRepository<FileEntity, int> fileRepository,
        IRepository<Venue, int> venueRepository,
        IRepository<Species, int> speciesRepository)
        : base(repository)
    {
        _fileRepository = fileRepository;
        _venueRepository = venueRepository;
        _speciesRepository = speciesRepository;
        GetPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        GetListPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        CreatePolicyName = SessionLoggerPermissions.SessionConfig.Create;
        UpdatePolicyName = SessionLoggerPermissions.SessionConfig.Edit;
        DeletePolicyName = SessionLoggerPermissions.SessionConfig.Delete;
    }

    // The paged list doesn't use includeDetails, but the cards still need the venue name
    // and each catch's species/bait/method/rig names - without these, every session in
    // the list comes back with an empty Catches collection (0 fish, no species chips).
    //
    // Species is deliberately NOT included here, even though the cards need its name:
    // Include loads the whole species row, including its default photo (a varbinary(max)
    // image), and in a join that row is repeated once per catch - so listing ~200 sessions
    // made SQL Server read the same few species photos hundreds of times over (~160MB per
    // page load, which was most of the request's time). The names are filled in from a
    // tiny Id/Name lookup instead - see ApplyDisplayNamesAsync.
    protected override async Task<IQueryable<Session>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        return query
            .Include(x => x.Venue)
            .Include(x => x.TargetSpecies)
            .Include(x => x.Catches).ThenInclude(c => c.Bait)
            .Include(x => x.Catches).ThenInclude(c => c.Method)
            .Include(x => x.Catches).ThenInclude(c => c.Rig)
            .Where(x => x.CreatorId == CurrentUser.Id);
    }

    protected override async Task<Session> GetEntityByIdAsync(int id)
    {
        var entity = await Repository.GetAsync(id);
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new EntityNotFoundException(typeof(Session), id);
        }

        return entity;
    }

    // Venues, species, baits, methods and rigs are soft-deleted, not removed, so a session
    // keeps showing what it was logged against. The soft-delete filter has to be off for
    // everything a session touches: left on, EF drops any session or catch whose (required)
    // venue/species has been deleted, as though it never existed.
    public override async Task<SessionDto> GetAsync(int id)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            var dto = await base.GetAsync(id);
            await ApplyDisplayNamesAsync(dto);
            return dto;
        }
    }

    public override async Task<PagedResultDto<SessionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            var result = await base.GetListAsync(input);
            await ApplyDisplayNamesAsync(result.Items.ToArray());
            return result;
        }
    }

    public override async Task<SessionDto> CreateAsync(CreateUpdateSessionDto input)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            return await base.CreateAsync(input);
        }
    }

    public override async Task<SessionDto> UpdateAsync(int id, CreateUpdateSessionDto input)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            return await base.UpdateAsync(id, input);
        }
    }

    public override async Task DeleteAsync(int id)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            await base.DeleteAsync(id);
        }
    }

    // Two things the DTOs need that the query deliberately doesn't load:
    //  - Venue: two venues can share a name on different tickets - VenueName carries the
    //    disambiguated label (see VenueLabels) so session cards tell them apart.
    //  - Species: only the name is wanted, so it comes from an Id/Name-only query rather
    //    than Include(Species), which would drag each species' photo blob along with it
    //    (see CreateFilteredQueryAsync).
    private async Task ApplyDisplayNamesAsync(params SessionDto[] dtos)
    {
        var venueQuery = await _venueRepository.GetQueryableAsync();
        var venues = await AsyncExecuter.ToListAsync(
            venueQuery.Include(v => v.Ticket).Where(v => v.CreatorId == CurrentUser.Id));
        var labels = VenueLabels.From(venues);

        var speciesQuery = await _speciesRepository.GetQueryableAsync();
        var speciesNames = (await AsyncExecuter.ToListAsync(speciesQuery.Select(s => new { s.Id, s.Name })))
            .ToDictionary(s => s.Id, s => s.Name);

        foreach (var dto in dtos)
        {
            dto.VenueName = labels.For(dto.VenueId, dto.VenueName);

            // Ids and names stay in step (both by name) so the form can pair them up.
            var targets = dto.TargetSpeciesIds
                .Where(speciesNames.ContainsKey)
                .Select(id => (Id: id, Name: speciesNames[id]))
                .OrderBy(t => t.Name)
                .ToList();
            dto.TargetSpeciesIds = targets.Select(t => t.Id).ToList();
            dto.TargetSpeciesNames = targets.Select(t => t.Name).ToList();

            foreach (var catchDto in dto.Catches ?? Enumerable.Empty<CatchDto>())
            {
                catchDto.SpeciesName = speciesNames.TryGetValue(catchDto.SpeciesId, out var name)
                    ? name
                    : catchDto.SpeciesName;
            }
        }
    }

    protected override async Task<Session> MapToEntityAsync(CreateUpdateSessionDto createInput)
    {
        var entity = await base.MapToEntityAsync(createInput);
        await ReconcileCatches(createInput.Catches, entity);
        await ReconcileTargetSpecies(createInput.TargetSpeciesIds, entity);
        entity.IsBlank = entity.Catches.Count == 0;
        return entity;
    }

    protected override async Task MapToEntityAsync(CreateUpdateSessionDto updateInput, Session entity)
    {
        await base.MapToEntityAsync(updateInput, entity);
        await ReconcileCatches(updateInput.Catches, entity);
        await ReconcileTargetSpecies(updateInput.TargetSpeciesIds, entity);
        entity.IsBlank = entity.Catches.Count == 0;
    }

    // Makes the session's targets exactly the species asked for, adding and removing
    // only the difference so unchanged links aren't deleted and re-inserted. An empty or
    // missing list clears the targets, which is how "Any" is stored.
    private async Task ReconcileTargetSpecies(ICollection<int> incoming, Session session)
    {
        var wanted = (incoming ?? new List<int>()).ToHashSet();

        foreach (var existing in session.TargetSpecies.Where(s => !wanted.Contains(s.Id)).ToList())
        {
            session.TargetSpecies.Remove(existing);
        }

        var missing = wanted.Except(session.TargetSpecies.Select(s => s.Id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        // A species that has since been deleted can stay a target of the sessions already
        // using it (they're kept above), but can't be newly chosen - so this lookup is the
        // one place the soft-delete filter is switched back on.
        List<Species> found;
        using (DataFilter.Enable<ISoftDelete>())
        {
            var speciesQuery = await _speciesRepository.GetQueryableAsync();
            found = await AsyncExecuter.ToListAsync(speciesQuery.Where(s => missing.Contains(s.Id)));
        }

        var unknown = missing.Except(found.Select(s => s.Id)).ToList();
        if (unknown.Count > 0)
        {
            throw new EntityNotFoundException(typeof(Species), unknown[0]);
        }

        foreach (var species in found)
        {
            session.TargetSpecies.Add(species);
        }
    }

    // AutoMapper's default list-mapping would delete and recreate every catch (and
    // cascade-delete their photos) on every save. Instead: match incoming catches to
    // existing ones by Id, update those in place, add ones with no Id, and remove
    // existing ones that are no longer present.
    private async Task ReconcileCatches(ICollection<CreateUpdateCatchDto> incoming, Session session)
    {
        var incomingIds = incoming
            .Where(c => c.Id.HasValue)
            .Select(c => c.Id!.Value)
            .ToHashSet();

        foreach (var existing in session.Catches.Where(c => !incomingIds.Contains(c.Id)).ToList())
        {
            session.Catches.Remove(existing);
        }

        foreach (var dto in incoming)
        {
            var existing = dto.Id.HasValue
                ? session.Catches.FirstOrDefault(c => c.Id == dto.Id.Value)
                : null;

            if (existing != null)
            {
                ObjectMapper.Map(dto, existing);
                existing.LengthMm = ToMillimetres(dto);
                // Kept purely so the value can be displayed the way it was entered,
                // not just silently reformatted to mm/lb+oz - see Catch.LengthUnit.
                // The frontend resends a catch it isn't actively editing using its
                // own already-stored unit (a lossless round-trip through the same
                // conversion), so this never overwrites the real unit with a default.
                existing.LengthUnit = existing.LengthMm != null ? dto.LengthUnit : null;
                existing.WeightG = ToGrams(dto);
                existing.WeightUnit = existing.WeightG != null ? dto.WeightUnit : null;

                // Explicit delete rather than just nulling the navigation - the FK is
                // required, so leaving EF Core to infer intent from an orphaned
                // reference is unnecessary risk when we can just say what we mean.
                if (dto.RemovePhoto && dto.PhotoData == null)
                {
                    var photoQuery = await _fileRepository.GetQueryableAsync();
                    var photo = photoQuery.FirstOrDefault(f => f.CatchId == existing.Id);
                    if (photo != null)
                    {
                        await _fileRepository.DeleteAsync(photo);
                        existing.Photo = null;
                    }
                }
            }
            else
            {
                var newCatch = ObjectMapper.Map<CreateUpdateCatchDto, Catch>(dto);
                newCatch.LengthMm = ToMillimetres(dto);
                newCatch.LengthUnit = newCatch.LengthMm != null ? dto.LengthUnit : null;
                newCatch.WeightG = ToGrams(dto);
                newCatch.WeightUnit = newCatch.WeightG != null ? dto.WeightUnit : null;
                session.Catches.Add(newCatch);
            }
        }
    }

    // Converted once, at write time, to a single canonical unit - so every future
    // comparison/aggregation is a plain numeric one, with no per-query unit handling
    // (same approach as BaitAppService.ToMillimetres).
    private static decimal? ToMillimetres(CreateUpdateCatchDto input)
    {
        if (input.LengthValue == null)
        {
            return null;
        }

        return input.LengthUnit switch
        {
            SizeUnit.Centimetres => input.LengthValue * 10m,
            SizeUnit.Inches => input.LengthValue * 25.4m,
            _ => input.LengthValue
        };
    }

    // Converted once, at write time, to grams (using the exact avoirdupois factors for
    // LbOz), so every future comparison/aggregation is a plain numeric one, with no
    // per-query unit handling.
    private static decimal? ToGrams(CreateUpdateCatchDto input)
    {
        switch (input.WeightUnit)
        {
            case WeightUnit.Kilograms:
                return input.WeightValue == null ? null : input.WeightValue * 1000m;

            case WeightUnit.Grams:
                return input.WeightValue;

            default: // LbOz
                if (input.WeightLbs == null && input.WeightOz == null)
                {
                    return null;
                }

                var lbs = input.WeightLbs ?? 0;
                var oz = input.WeightOz ?? 0;
                return lbs * 453.59237m + oz * 28.349523125m;
        }
    }
}
