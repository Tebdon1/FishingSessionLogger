using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Catches;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Sessions;
using SessionLogger.SpeciesTypes;
using SessionLogger.Statistics;
using SessionLogger.Tickets;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace SessionLogger.Venues;

// Two venues can share a name on different tickets (a canal stretch split between two
// clubs, say) - these cover that being allowed, the ticket being appended to tell them
// apart wherever they're shown by name, and the same name on the same ticket being
// rejected.
public class VenueAppServiceTests : SessionLoggerApplicationTestBase
{
    private readonly IVenueAppService _venueAppService;
    private readonly ITicketAppService _ticketAppService;
    private readonly ISessionAppService _sessionAppService;
    private readonly ISpeciesAppService _speciesAppService;
    private readonly IStatisticsAppService _statisticsAppService;

    public VenueAppServiceTests()
    {
        _venueAppService = GetRequiredService<IVenueAppService>();
        _ticketAppService = GetRequiredService<ITicketAppService>();
        _sessionAppService = GetRequiredService<ISessionAppService>();
        _speciesAppService = GetRequiredService<ISpeciesAppService>();
        _statisticsAppService = GetRequiredService<IStatisticsAppService>();
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Same_Name_On_Same_Ticket()
    {
        var ticket = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = ticket.Id });

        // Case and surrounding whitespace don't make it a different venue.
        await Should.ThrowAsync<UserFriendlyException>(() =>
            _venueAppService.CreateAsync(new VenueUpdateDto { Name = "  k&a canal midgham ", TicketId = ticket.Id }));
    }

    [Fact]
    public async Task CreateAsync_Should_Reject_Same_Name_When_Both_Have_No_Ticket()
    {
        await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Park Lake" });

        await Should.ThrowAsync<UserFriendlyException>(() =>
            _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Park Lake" }));
    }

    [Fact]
    public async Task UpdateAsync_Should_Reject_Renaming_Onto_An_Existing_Name_And_Ticket()
    {
        var ticket = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Midgham", TicketId = ticket.Id });
        var other = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Theale", TicketId = ticket.Id });

        await Should.ThrowAsync<UserFriendlyException>(() =>
            _venueAppService.UpdateAsync(other.Id, new VenueUpdateDto { Name = "Midgham", TicketId = ticket.Id }));
    }

    [Fact]
    public async Task UpdateAsync_Should_Not_Conflict_With_The_Venue_Being_Edited()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Midgham", Postcode = "RG7" });

        var updated = await _venueAppService.UpdateAsync(venue.Id, new VenueUpdateDto { Name = "Midgham", Postcode = "RG7 5" });

        updated.Postcode.ShouldBe("RG7 5");
    }

    [Fact]
    public async Task Same_Name_On_Different_Tickets_Should_Be_Allowed_And_Labelled_By_Ticket()
    {
        var rdaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RDAA" });
        var rtaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        var first = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rdaa.Id });
        var second = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rtaa.Id });
        var unrelated = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Park Lake", TicketId = rdaa.Id });

        var list = await _venueAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        list.Items.Single(v => v.Id == first.Id).DisplayName.ShouldBe("K&A Canal Midgham (RDAA)");
        list.Items.Single(v => v.Id == second.Id).DisplayName.ShouldBe("K&A Canal Midgham (RTAA)");
        // A name that's unique keeps its plain label - the ticket is only added to disambiguate.
        list.Items.Single(v => v.Id == unrelated.Id).DisplayName.ShouldBe("Willow Park Lake");
        // Name itself stays as entered, so the edit form isn't polluted with the suffix.
        list.Items.Single(v => v.Id == first.Id).Name.ShouldBe("K&A Canal Midgham");
    }

    [Fact]
    public async Task A_Venue_With_No_Ticket_Is_Labelled_As_Such_When_Its_Name_Is_Shared()
    {
        var rtaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        var ticketed = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Midgham", TicketId = rtaa.Id });
        var unticketed = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Midgham" });

        var list = await _venueAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        list.Items.Single(v => v.Id == ticketed.Id).DisplayName.ShouldBe("Midgham (RTAA)");
        list.Items.Single(v => v.Id == unticketed.Id).DisplayName.ShouldBe("Midgham (No ticket)");
    }

    [Fact]
    public async Task Sessions_Should_Show_The_Disambiguated_Venue_Label()
    {
        var rdaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RDAA" });
        var rtaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rdaa.Id });
        var rtaaVenue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rtaa.Id });

        var created = await _sessionAppService.CreateAsync(NewSession(rtaaVenue.Id));

        var fetched = await _sessionAppService.GetAsync(created.Id);
        fetched.VenueName.ShouldBe("K&A Canal Midgham (RTAA)");

        var list = await _sessionAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.Items.ShouldHaveSingleItem().VenueName.ShouldBe("K&A Canal Midgham (RTAA)");
    }

    // Regression: "Catches by venue" used to group by venue NAME, so two same-named venues
    // on different tickets were added together into a single bar.
    [Fact]
    public async Task CatchesByVenue_Should_Keep_Same_Named_Venues_On_Different_Tickets_Separate()
    {
        var rdaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RDAA" });
        var rtaa = await _ticketAppService.CreateAsync(new TicketUpdateDto { Name = "RTAA" });
        var rdaaVenue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rdaa.Id });
        var rtaaVenue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "K&A Canal Midgham", TicketId = rtaa.Id });
        var species = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Perch " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });

        await _sessionAppService.CreateAsync(NewSession(rdaaVenue.Id, species.Id, catchCount: 3));
        await _sessionAppService.CreateAsync(NewSession(rtaaVenue.Id, species.Id, catchCount: 2));

        var stats = await _statisticsAppService.GetAsync(new GetStatisticsInput());

        stats.CatchesByVenue.Count.ShouldBe(2);
        stats.CatchesByVenue.Single(v => v.Name == "K&A Canal Midgham (RDAA)").Count.ShouldBe(3);
        stats.CatchesByVenue.Single(v => v.Name == "K&A Canal Midgham (RTAA)").Count.ShouldBe(2);
        stats.TopVenues.Select(v => v.VenueName).ShouldBe(
            new[] { "K&A Canal Midgham (RDAA)", "K&A Canal Midgham (RTAA)" }, ignoreOrder: true);

        // Species names are attached separately from the catches (Species isn't Included,
        // to avoid reading its photo blob once per catch) - make sure that still lands.
        var bySpecies = stats.CatchesBySpecies.ShouldHaveSingleItem();
        bySpecies.Name.ShouldBe(species.Name);
        bySpecies.Count.ShouldBe(5);
        stats.TopSpecies.ShouldHaveSingleItem().SpeciesName.ShouldBe(species.Name);
    }

    private static CreateUpdateSessionDto NewSession(int venueId, int? speciesId = null, int catchCount = 0)
    {
        return new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-4),
            EndDateTime = DateTime.Now,
            VenueId = venueId,
            Catches = Enumerable.Range(0, catchCount)
                .Select(_ => new CreateUpdateCatchDto { SessionId = 0, SpeciesId = speciesId!.Value })
                .ToList(),
        };
    }
}
