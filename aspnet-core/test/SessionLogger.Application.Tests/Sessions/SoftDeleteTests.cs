using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Baits;
using SessionLogger.Catches;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.SpeciesTypes;
using SessionLogger.Statistics;
using SessionLogger.Venues;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace SessionLogger.Sessions;

// Species, baits, methods, rigs and venues are soft-deleted so that deleting one never
// rewrites the history of the sessions and catches logged against it.
public class SoftDeleteTests : SessionLoggerApplicationTestBase
{
    private readonly ISessionAppService _sessions;
    private readonly IVenueAppService _venues;
    private readonly ISpeciesAppService _species;
    private readonly IBaitAppService _baits;
    private readonly IStatisticsAppService _statistics;

    public SoftDeleteTests()
    {
        _sessions = GetRequiredService<ISessionAppService>();
        _venues = GetRequiredService<IVenueAppService>();
        _species = GetRequiredService<ISpeciesAppService>();
        _baits = GetRequiredService<IBaitAppService>();
        _statistics = GetRequiredService<IStatisticsAppService>();
    }

    private async Task<SessionDto> LogSessionAsync(int venueId, int speciesId, int? baitId = null, params int[] targets)
    {
        return await _sessions.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-3),
            EndDateTime = DateTime.Now,
            VenueId = venueId,
            TargetSpeciesIds = targets.ToList(),
            Catches = new List<CreateUpdateCatchDto>
            {
                new CreateUpdateCatchDto
                {
                    SessionId = 0,
                    SpeciesId = speciesId,
                    BaitId = baitId,
                    WeightUnit = WeightUnit.Kilograms,
                    WeightValue = 2m,
                },
            },
        });
    }

    [Fact]
    public async Task Deleted_Species_Should_Be_Hidden_From_Lookups_But_Kept_On_Past_Catches_And_Targets()
    {
        var venue = await _venues.CreateAsync(new VenueUpdateDto { Name = "History Lake " + Guid.NewGuid() });
        var species = await _species.CreateAsync(new SpeciesUpdateDto { Name = "Zander " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var session = await LogSessionAsync(venue.Id, species.Id, null, species.Id);

        await _species.DeleteAsync(species.Id);

        (await _species.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 }))
            .Items.ShouldNotContain(s => s.Id == species.Id);

        var reloaded = await _sessions.GetAsync(session.Id);
        reloaded.Catches.Count.ShouldBe(1);
        reloaded.Catches.Single().SpeciesName.ShouldBe(species.Name);
        reloaded.TargetSpeciesIds.ShouldBe(new[] { species.Id });
        reloaded.TargetSpeciesNames.ShouldBe(new[] { species.Name });

        (await _sessions.GetListAsync(new PagedAndSortedResultRequestDto())).Items
            .ShouldHaveSingleItem().Catches.Single().SpeciesName.ShouldBe(species.Name);

        var stats = await _statistics.GetAsync(new GetStatisticsInput());
        stats.CatchesBySpecies.ShouldContain(x => x.Name == species.Name && x.Count == 1);
    }

    [Fact]
    public async Task A_Session_Should_Survive_And_Stay_Editable_After_Its_Venue_Is_Deleted()
    {
        var venue = await _venues.CreateAsync(new VenueUpdateDto { Name = "Gone Pond " + Guid.NewGuid() });
        var species = await _species.CreateAsync(new SpeciesUpdateDto { Name = "Roach " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var session = await LogSessionAsync(venue.Id, species.Id);

        await _venues.DeleteAsync(venue.Id);

        (await _venues.GetListAsync(new PagedAndSortedResultRequestDto())).Items.ShouldBeEmpty();

        var listed = (await _sessions.GetListAsync(new PagedAndSortedResultRequestDto())).Items.ShouldHaveSingleItem();
        listed.VenueId.ShouldBe(venue.Id);
        listed.VenueName.ShouldBe(venue.Name);

        (await _statistics.GetAsync(new GetStatisticsInput())).Summary.TotalCatches.ShouldBe(1);

        var updated = await _sessions.UpdateAsync(session.Id, new CreateUpdateSessionDto
        {
            StartDateTime = session.StartDateTime,
            EndDateTime = session.EndDateTime,
            VenueId = venue.Id,
            Notes = "Edited after the venue was deleted",
            Catches = new List<CreateUpdateCatchDto>(),
        });
        updated.Notes.ShouldBe("Edited after the venue was deleted");

        await _sessions.DeleteAsync(session.Id);
        (await _sessions.GetListAsync(new PagedAndSortedResultRequestDto())).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_Deleted_Bait_Should_Still_Be_Named_On_Past_Catches()
    {
        var venue = await _venues.CreateAsync(new VenueUpdateDto { Name = "Bait Lake " + Guid.NewGuid() });
        var species = await _species.CreateAsync(new SpeciesUpdateDto { Name = "Bream " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var bait = await _baits.CreateAsync(new BaitUpdateDto { BaitType = BaitType.Natural, Name = "Sweetcorn " + Guid.NewGuid() });
        await LogSessionAsync(venue.Id, species.Id, bait.Id);

        await _baits.DeleteAsync(bait.Id);

        var catchDto = (await _sessions.GetListAsync(new PagedAndSortedResultRequestDto())).Items
            .ShouldHaveSingleItem().Catches.ShouldHaveSingleItem();
        catchDto.BaitId.ShouldBe(bait.Id);
        catchDto.BaitName.ShouldBe(bait.Name);
    }

    [Fact]
    public async Task A_Deleted_Name_Should_Be_Reusable()
    {
        var name = "Mirror Carp " + Guid.NewGuid();
        var first = await _species.CreateAsync(new SpeciesUpdateDto { Name = name, WaterType = SpeciesWaterType.Freshwater });
        await _species.DeleteAsync(first.Id);

        var second = await _species.CreateAsync(new SpeciesUpdateDto { Name = name, WaterType = SpeciesWaterType.Freshwater });

        second.Id.ShouldNotBe(first.Id);
        (await _species.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 }))
            .Items.Count(s => s.Name == name).ShouldBe(1);
    }

    // A deleted species stays a target of the sessions already using it, but can't be
    // picked for a new one.
    [Fact]
    public async Task A_Deleted_Species_Should_Not_Be_Newly_Targetable()
    {
        var venue = await _venues.CreateAsync(new VenueUpdateDto { Name = "Retired Lake " + Guid.NewGuid() });
        var species = await _species.CreateAsync(new SpeciesUpdateDto { Name = "Rudd " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var older = await _sessions.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddDays(-1),
            EndDateTime = DateTime.Now.AddDays(-1).AddHours(2),
            VenueId = venue.Id,
            TargetSpeciesIds = new List<int> { species.Id },
            Catches = new List<CreateUpdateCatchDto>(),
        });

        await _species.DeleteAsync(species.Id);

        // The existing session can be re-saved with its deleted target intact...
        var resaved = await _sessions.UpdateAsync(older.Id, new CreateUpdateSessionDto
        {
            StartDateTime = older.StartDateTime,
            EndDateTime = older.EndDateTime,
            VenueId = venue.Id,
            TargetSpeciesIds = new List<int> { species.Id },
            Catches = new List<CreateUpdateCatchDto>(),
        });
        resaved.TargetSpeciesIds.ShouldBe(new[] { species.Id });

        // ...but a new session can't pick it.
        await Should.ThrowAsync<EntityNotFoundException>(() => _sessions.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-1),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            TargetSpeciesIds = new List<int> { species.Id },
            Catches = new List<CreateUpdateCatchDto>(),
        }));
    }
}
