using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Catches;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Sessions;
using SessionLogger.SpeciesTypes;
using SessionLogger.Venues;
using Xunit;

namespace SessionLogger.Statistics;

public class StatisticsAppServiceTests : SessionLoggerApplicationTestBase
{
    private readonly IStatisticsAppService _statisticsAppService;
    private readonly ISessionAppService _sessionAppService;
    private readonly IVenueAppService _venueAppService;
    private readonly ISpeciesAppService _speciesAppService;

    public StatisticsAppServiceTests()
    {
        _statisticsAppService = GetRequiredService<IStatisticsAppService>();
        _sessionAppService = GetRequiredService<ISessionAppService>();
        _venueAppService = GetRequiredService<IVenueAppService>();
        _speciesAppService = GetRequiredService<ISpeciesAppService>();
    }

    [Fact]
    public async Task SessionsByMonthOfYear_Should_Always_Have_All_Twelve_Months()
    {
        var stats = await _statisticsAppService.GetAsync(new GetStatisticsInput());

        stats.SessionsByMonthOfYear.Select(m => m.Month).ShouldBe(Enumerable.Range(1, 12));
        stats.SessionsByMonthOfYear.ShouldAllBe(m => m.ProductiveSessionCount == 0 && m.BlankSessionCount == 0);
    }

    [Fact]
    public async Task SessionsByMonthOfYear_Should_Sum_Each_Month_Across_Years_And_Split_Productive_From_Blank()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Lake " + Guid.NewGuid() });
        var species = await _speciesAppService.CreateAsync(new SpeciesUpdateDto
        {
            Name = "Roach " + Guid.NewGuid(),
            WaterType = SpeciesWaterType.Freshwater,
        });

        // March in two different years: one productive, one blank - both land in March.
        await CreateSessionAsync(venue.Id, new DateTime(2024, 3, 10, 9, 0, 0), species.Id);
        await CreateSessionAsync(venue.Id, new DateTime(2025, 3, 22, 9, 0, 0), speciesId: null);
        // October: two productive sessions.
        await CreateSessionAsync(venue.Id, new DateTime(2025, 10, 4, 9, 0, 0), species.Id);
        await CreateSessionAsync(venue.Id, new DateTime(2025, 10, 18, 9, 0, 0), species.Id);

        var stats = await _statisticsAppService.GetAsync(new GetStatisticsInput());

        var march = stats.SessionsByMonthOfYear.Single(m => m.Month == 3);
        march.ProductiveSessionCount.ShouldBe(1);
        march.BlankSessionCount.ShouldBe(1);

        var october = stats.SessionsByMonthOfYear.Single(m => m.Month == 10);
        october.ProductiveSessionCount.ShouldBe(2);
        october.BlankSessionCount.ShouldBe(0);

        // Months with nothing are real zeros, not missing.
        stats.SessionsByMonthOfYear.Single(m => m.Month == 7).ProductiveSessionCount.ShouldBe(0);
        stats.SessionsByMonthOfYear.Sum(m => m.ProductiveSessionCount + m.BlankSessionCount).ShouldBe(4);
    }

    [Fact]
    public async Task SessionsByMonthOfYear_Should_Respect_The_Date_Range()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Lake " + Guid.NewGuid() });
        await CreateSessionAsync(venue.Id, new DateTime(2024, 3, 10, 9, 0, 0), speciesId: null);
        await CreateSessionAsync(venue.Id, new DateTime(2025, 3, 10, 9, 0, 0), speciesId: null);

        var stats = await _statisticsAppService.GetAsync(new GetStatisticsInput
        {
            DateFrom = new DateTime(2025, 1, 1),
        });

        stats.SessionsByMonthOfYear.Single(m => m.Month == 3).BlankSessionCount.ShouldBe(1);
    }

    private Task<SessionDto> CreateSessionAsync(int venueId, DateTime start, int? speciesId)
    {
        return _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = start,
            EndDateTime = start.AddHours(3),
            VenueId = venueId,
            Catches = speciesId == null
                ? new List<CreateUpdateCatchDto>()
                : new List<CreateUpdateCatchDto>
                {
                    new CreateUpdateCatchDto { SessionId = 0, SpeciesId = speciesId.Value },
                },
        });
    }
}
