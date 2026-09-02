using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SessionLogger.Domain.Catches;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.PersonalBests;

// Computed on read from Catch/Session rather than stored - self-heals whenever a
// catch is corrected or deleted, with no separate "current PB" row to keep in sync.
public class PersonalBestAppService : SessionLoggerAppService, IPersonalBestAppService
{
    private readonly IRepository<Catch, int> _catchRepository;

    public PersonalBestAppService(IRepository<Catch, int> catchRepository)
    {
        _catchRepository = catchRepository;
    }

    public async Task<List<PersonalBestDto>> GetListAsync()
    {
        var query = await _catchRepository.GetQueryableAsync();

        // Materialized before grouping - per-user catch counts are small enough that
        // this is simpler and cheaper than pushing "top catch per species per metric"
        // down into SQL, and it lets weight and length pick different record catches
        // (a species' heaviest fish needn't also be its longest). Photo included since
        // the species tile needs it to show the actual record fish.
        var userCatches = query
            .Include(c => c.Photo)
            .Where(c => c.Session.CreatorId == CurrentUser.Id)
            .ToList();

        return userCatches
            .GroupBy(c => c.SpeciesId)
            .Select(g =>
            {
                var bestWeight = g.Where(c => c.WeightG != null).OrderByDescending(c => c.WeightG).FirstOrDefault();
                var bestLength = g.Where(c => c.LengthMm != null).OrderByDescending(c => c.LengthMm).FirstOrDefault();
                return new PersonalBestDto
                {
                    SpeciesId = g.Key,
                    WeightG = bestWeight?.WeightG,
                    WeightUnit = bestWeight?.WeightUnit,
                    WeightPhotoId = bestWeight?.Photo?.Id,
                    LengthMm = bestLength?.LengthMm,
                    LengthUnit = bestLength?.LengthUnit,
                    LengthPhotoId = bestLength?.Photo?.Id
                };
            })
            .ToList();
    }
}
