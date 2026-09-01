using Microsoft.EntityFrameworkCore;
using SessionLogger.Catches;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Sessions;
using SessionLogger.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public SessionAppService(IRepository<Session, int> repository, IRepository<FileEntity, int> fileRepository)
        : base(repository)
    {
        _fileRepository = fileRepository;
        GetPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        GetListPolicyName = SessionLoggerPermissions.SessionConfig.Search;
        CreatePolicyName = SessionLoggerPermissions.SessionConfig.Create;
        UpdatePolicyName = SessionLoggerPermissions.SessionConfig.Edit;
        DeletePolicyName = SessionLoggerPermissions.SessionConfig.Delete;
    }

    // The paged list doesn't use includeDetails, but the cards still need the venue name
    // and each catch's species/bait/method/rig names - without these, every session in
    // the list comes back with an empty Catches collection (0 fish, no species chips).
    protected override async Task<IQueryable<Session>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        return query
            .Include(x => x.Venue)
            .Include(x => x.Catches).ThenInclude(c => c.Species)
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

    protected override async Task<Session> MapToEntityAsync(CreateUpdateSessionDto createInput)
    {
        var entity = await base.MapToEntityAsync(createInput);
        await ReconcileCatches(createInput.Catches, entity);
        entity.IsBlank = entity.Catches.Count == 0;
        return entity;
    }

    protected override async Task MapToEntityAsync(CreateUpdateSessionDto updateInput, Session entity)
    {
        await base.MapToEntityAsync(updateInput, entity);
        await ReconcileCatches(updateInput.Catches, entity);
        entity.IsBlank = entity.Catches.Count == 0;
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
