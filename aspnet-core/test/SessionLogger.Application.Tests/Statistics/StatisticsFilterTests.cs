using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Baits;
using SessionLogger.Catches;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Domain.Venues;
using SessionLogger.Methods;
using SessionLogger.Rigs;
using SessionLogger.Sessions;
using SessionLogger.SpeciesTypes;
using SessionLogger.Tickets;
using SessionLogger.Venues;
using Xunit;

namespace SessionLogger.Statistics;

// What each statistics filter means (see GetStatisticsInput): session filters pick which
// sessions count and keep blanks; catch filters pick which catches count - all of them on
// the same fish - and a session only counts if it has a matching one.
public class StatisticsFilterTests : SessionLoggerApplicationTestBase
{
    private readonly IStatisticsAppService _statistics;
    private readonly ISessionAppService _sessions;
    private readonly IVenueAppService _venues;
    private readonly ISpeciesAppService _species;
    private readonly IBaitAppService _baits;
    private readonly IMethodAppService _methods;
    private readonly IRigAppService _rigs;
    private readonly ITicketAppService _tickets;

    private static readonly DateTime Day = new DateTime(2025, 6, 1, 9, 0, 0);

    public StatisticsFilterTests()
    {
        _statistics = GetRequiredService<IStatisticsAppService>();
        _sessions = GetRequiredService<ISessionAppService>();
        _venues = GetRequiredService<IVenueAppService>();
        _species = GetRequiredService<ISpeciesAppService>();
        _baits = GetRequiredService<IBaitAppService>();
        _methods = GetRequiredService<IMethodAppService>();
        _rigs = GetRequiredService<IRigAppService>();
        _tickets = GetRequiredService<ITicketAppService>();
    }

    // ---- catch filters --------------------------------------------------------------

    [Fact]
    public async Task A_Species_Filter_Should_Count_Only_That_Species_And_Only_Sessions_That_Caught_It()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var pike = await NewSpeciesAsync("Pike");

        await NewSessionAsync(venue, Catches((perch, null, null, null), (perch, null, null, null), (pike, null, null, null)));
        await NewSessionAsync(venue, Catches((pike, null, null, null)));
        await NewSessionAsync(venue, Catches()); // blank

        var stats = await _statistics.GetAsync(new GetStatisticsInput { SpeciesIds = new List<int> { perch } });

        stats.Summary.TotalSessions.ShouldBe(1);
        stats.Summary.TotalCatches.ShouldBe(2);
        stats.CatchesBySpecies.ShouldHaveSingleItem().Count.ShouldBe(2);
        stats.CatchesByVenue.ShouldHaveSingleItem().Count.ShouldBe(2);
    }

    [Fact]
    public async Task Several_Species_In_One_Filter_Should_Match_Any_Of_Them()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var pike = await NewSpeciesAsync("Pike");
        var roach = await NewSpeciesAsync("Roach");

        await NewSessionAsync(venue, Catches((perch, null, null, null)));
        await NewSessionAsync(venue, Catches((pike, null, null, null)));
        await NewSessionAsync(venue, Catches((roach, null, null, null)));

        var stats = await _statistics.GetAsync(new GetStatisticsInput { SpeciesIds = new List<int> { perch, pike } });

        stats.Summary.TotalSessions.ShouldBe(2);
        stats.Summary.TotalCatches.ShouldBe(2);
    }

    [Fact]
    public async Task Catch_Filters_Should_All_Have_To_Match_The_Same_Fish()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var pike = await NewSpeciesAsync("Pike");
        var worm = await NewBaitAsync("Worm");
        var lure = await NewBaitAsync("Lure");

        // Perch on a lure, and pike on a worm - the session has a perch and a worm, but never
        // a perch ON a worm.
        await NewSessionAsync(venue, Catches((perch, lure, null, null), (pike, worm, null, null)));

        var perchOnWorm = await _statistics.GetAsync(new GetStatisticsInput
        {
            SpeciesIds = new List<int> { perch },
            BaitIds = new List<int> { worm },
        });
        perchOnWorm.Summary.TotalSessions.ShouldBe(0);
        perchOnWorm.Summary.TotalCatches.ShouldBe(0);

        var perchOnLure = await _statistics.GetAsync(new GetStatisticsInput
        {
            SpeciesIds = new List<int> { perch },
            BaitIds = new List<int> { lure },
        });
        perchOnLure.Summary.TotalSessions.ShouldBe(1);
        perchOnLure.Summary.TotalCatches.ShouldBe(1);
    }

    [Fact]
    public async Task Bait_Method_And_Rig_Filters_Should_Each_Narrow_The_Catches()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var worm = await NewBaitAsync("Worm");
        var ledger = await NewMethodAsync("Ledger");
        var float_ = await NewMethodAsync("Float");
        var hair = await NewRigAsync("Hair Rig");

        await NewSessionAsync(venue, Catches(
            (perch, worm, ledger, hair),
            (perch, worm, float_, null),
            (perch, null, null, null)));

        // Three catches in the session, but one has no bait recorded - so it has nothing to
        // match a bait filter against, and only two count.
        (await _statistics.GetAsync(new GetStatisticsInput { BaitIds = new List<int> { worm } }))
            .Summary.TotalCatches.ShouldBe(2);
        (await _statistics.GetAsync(new GetStatisticsInput { MethodIds = new List<int> { ledger } }))
            .Summary.TotalCatches.ShouldBe(1);
        (await _statistics.GetAsync(new GetStatisticsInput { RigIds = new List<int> { hair } }))
            .Summary.TotalCatches.ShouldBe(1);
    }

    [Fact]
    public async Task A_Catch_Filter_Should_Leave_Blank_Sessions_Out()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");

        await NewSessionAsync(venue, Catches((perch, null, null, null)));
        await NewSessionAsync(venue, Catches());
        await NewSessionAsync(venue, Catches());

        var unfiltered = await _statistics.GetAsync(new GetStatisticsInput());
        unfiltered.Summary.TotalSessions.ShouldBe(3);
        unfiltered.Summary.BlankRatePercent.ShouldBeGreaterThan(0);

        var filtered = await _statistics.GetAsync(new GetStatisticsInput { SpeciesIds = new List<int> { perch } });
        filtered.Summary.TotalSessions.ShouldBe(1);
        filtered.Summary.BlankRatePercent.ShouldBe(0);
    }

    // A species filter on its own also counts the blank sessions that were aimed at that
    // species; they have no catches, so only the session figures move.
    [Fact]
    public async Task A_Species_Filter_Should_Also_Count_Blank_Sessions_That_Targeted_It()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var pike = await NewSpeciesAsync("Pike");

        await NewSessionAsync(venue, Catches((perch, null, null, null), (perch, null, null, null)));
        await NewSessionAsync(venue, Catches(), targets: new[] { perch });         // blank, targeted perch
        await NewSessionAsync(venue, Catches(), targets: new[] { pike });          // blank, targeted pike
        await NewSessionAsync(venue, Catches());                                   // blank, no target
        await NewSessionAsync(venue, Catches((pike, null, null, null)), targets: new[] { perch }); // caught pike while after perch

        var stats = await _statistics.GetAsync(new GetStatisticsInput { SpeciesIds = new List<int> { perch } });

        stats.Summary.TotalSessions.ShouldBe(2);
        stats.Summary.TotalCatches.ShouldBe(2);
        stats.Summary.BlankRatePercent.ShouldBe(50);
        stats.CatchesBySpecies.ShouldHaveSingleItem().Count.ShouldBe(2);
    }

    [Fact]
    public async Task Targeted_Blanks_Should_Respect_The_Session_Filters_And_Drop_Out_When_Another_Catch_Filter_Is_Set()
    {
        var venue = await NewVenueAsync();
        var otherVenue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        var worm = await NewBaitAsync("Worm");

        await NewSessionAsync(venue, Catches((perch, worm, null, null)));
        await NewSessionAsync(venue, Catches(), targets: new[] { perch });
        await NewSessionAsync(otherVenue, Catches(), targets: new[] { perch });

        var atVenue = await _statistics.GetAsync(new GetStatisticsInput
        {
            SpeciesIds = new List<int> { perch },
            VenueIds = new List<int> { venue },
        });
        atVenue.Summary.TotalSessions.ShouldBe(2);

        var withBait = await _statistics.GetAsync(new GetStatisticsInput
        {
            SpeciesIds = new List<int> { perch },
            BaitIds = new List<int> { worm },
        });
        withBait.Summary.TotalSessions.ShouldBe(1);
        withBait.Summary.BlankRatePercent.ShouldBe(0);
    }

    // ---- session filters ------------------------------------------------------------

    [Fact]
    public async Task A_Venue_Filter_Should_Keep_That_Venues_Blank_Sessions()
    {
        var lake = await NewVenueAsync("Lake");
        var river = await NewVenueAsync("River");
        var perch = await NewSpeciesAsync("Perch");

        await NewSessionAsync(lake, Catches((perch, null, null, null)));
        await NewSessionAsync(lake, Catches()); // blank, but still a session at this venue
        await NewSessionAsync(river, Catches((perch, null, null, null)));

        var stats = await _statistics.GetAsync(new GetStatisticsInput { VenueIds = new List<int> { lake } });

        stats.Summary.TotalSessions.ShouldBe(2);
        stats.Summary.TotalCatches.ShouldBe(1);
        stats.Summary.BlankRatePercent.ShouldBe(50);
    }

    [Fact]
    public async Task Water_Type_And_Ticket_Filters_Should_Follow_The_Sessions_Venue()
    {
        var club = await _tickets.CreateAsync(new TicketUpdateDto { Name = "Club " + Guid.NewGuid() });
        var other = await _tickets.CreateAsync(new TicketUpdateDto { Name = "Other " + Guid.NewGuid() });
        var canal = await NewVenueAsync("Canal", WaterType.Canal, club.Id);
        var lake = await NewVenueAsync("Lake", WaterType.Lake, other.Id);
        var unset = await NewVenueAsync("Unset");

        await NewSessionAsync(canal, Catches());
        await NewSessionAsync(lake, Catches());
        await NewSessionAsync(unset, Catches());

        (await _statistics.GetAsync(new GetStatisticsInput { WaterTypes = new List<WaterType> { WaterType.Canal } }))
            .Summary.TotalSessions.ShouldBe(1);
        (await _statistics.GetAsync(new GetStatisticsInput { WaterTypes = new List<WaterType> { WaterType.Canal, WaterType.Lake } }))
            .Summary.TotalSessions.ShouldBe(2);
        (await _statistics.GetAsync(new GetStatisticsInput { TicketIds = new List<int> { club.Id } }))
            .Summary.TotalSessions.ShouldBe(1);
    }

    // ---- no filter ------------------------------------------------------------------

    [Fact]
    public async Task Null_Or_Empty_Filter_Lists_Should_Mean_No_Filter()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        await NewSessionAsync(venue, Catches((perch, null, null, null)));
        await NewSessionAsync(venue, Catches());

        var none = await _statistics.GetAsync(new GetStatisticsInput());
        var empty = await _statistics.GetAsync(new GetStatisticsInput
        {
            VenueIds = new List<int>(),
            WaterTypes = new List<WaterType>(),
            TicketIds = new List<int>(),
            SpeciesIds = new List<int>(),
            BaitIds = new List<int>(),
            MethodIds = new List<int>(),
            RigIds = new List<int>(),
        });

        empty.Summary.TotalSessions.ShouldBe(none.Summary.TotalSessions);
        empty.Summary.TotalCatches.ShouldBe(none.Summary.TotalCatches);
        empty.Summary.TotalSessions.ShouldBe(2);
    }

    [Fact]
    public async Task Filters_Should_Combine_With_The_Date_Range()
    {
        var venue = await NewVenueAsync();
        var perch = await NewSpeciesAsync("Perch");
        await NewSessionAsync(venue, Catches((perch, null, null, null)), start: new DateTime(2024, 6, 1, 9, 0, 0));
        await NewSessionAsync(venue, Catches((perch, null, null, null)), start: new DateTime(2025, 6, 1, 9, 0, 0));

        var stats = await _statistics.GetAsync(new GetStatisticsInput
        {
            SpeciesIds = new List<int> { perch },
            DateFrom = new DateTime(2025, 1, 1),
        });

        stats.Summary.TotalSessions.ShouldBe(1);
    }

    // ---- helpers --------------------------------------------------------------------

    private async Task<int> NewVenueAsync(string name = "Venue", WaterType? waterType = null, int? ticketId = null)
    {
        var venue = await _venues.CreateAsync(new VenueUpdateDto
        {
            Name = name + " " + Guid.NewGuid(),
            WaterType = waterType,
            TicketId = ticketId,
        });
        return venue.Id;
    }

    private async Task<int> NewSpeciesAsync(string name)
    {
        var species = await _species.CreateAsync(new SpeciesUpdateDto
        {
            Name = name + " " + Guid.NewGuid(),
            WaterType = SpeciesWaterType.Freshwater,
        });
        return species.Id;
    }

    private async Task<int> NewBaitAsync(string name)
    {
        var bait = await _baits.CreateAsync(new BaitUpdateDto { BaitType = BaitType.Natural, Name = name + " " + Guid.NewGuid() });
        return bait.Id;
    }

    private async Task<int> NewMethodAsync(string name)
    {
        var method = await _methods.CreateAsync(new MethodUpdateDto { Name = name + " " + Guid.NewGuid() });
        return method.Id;
    }

    private async Task<int> NewRigAsync(string name)
    {
        var rig = await _rigs.CreateAsync(new RigUpdateDto { Name = name });
        return rig.Id;
    }

    private static List<CreateUpdateCatchDto> Catches(params (int Species, int? Bait, int? Method, int? Rig)[] catches)
    {
        return catches.Select(c => new CreateUpdateCatchDto
        {
            SessionId = 0,
            SpeciesId = c.Species,
            BaitId = c.Bait,
            MethodId = c.Method,
            RigId = c.Rig,
        }).ToList();
    }

    private Task<SessionDto> NewSessionAsync(int venueId, List<CreateUpdateCatchDto> catches, DateTime? start = null, int[]? targets = null)
    {
        var from = start ?? Day;
        return _sessions.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = from,
            EndDateTime = from.AddHours(3),
            VenueId = venueId,
            TargetSpeciesIds = (targets ?? Array.Empty<int>()).ToList(),
            Catches = catches,
        });
    }
}
