using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Catches;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.SpeciesTypes;
using SessionLogger.Venues;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace SessionLogger.Sessions;

public class SessionAppServiceTests : SessionLoggerApplicationTestBase
{
    private readonly ISessionAppService _sessionAppService;
    private readonly IVenueAppService _venueAppService;
    private readonly ISpeciesAppService _speciesAppService;

    public SessionAppServiceTests()
    {
        _sessionAppService = GetRequiredService<ISessionAppService>();
        _venueAppService = GetRequiredService<IVenueAppService>();
        _speciesAppService = GetRequiredService<ISpeciesAppService>();
    }

    // Regression test for a bug where GetListAsync's CreateFilteredQueryAsync override
    // included Venue but not Catches, so every session on the sessions/home page came
    // back with an empty Catches collection - "0 fish" regardless of what was actually
    // logged, and no species/best-catch chips either.
    [Fact]
    public async Task GetListAsync_Should_Include_Each_Session_Catches_With_Species_Name()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Park Lake " + Guid.NewGuid() });
        var species = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Common Carp " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });

        var created = await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-4),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>
            {
                new CreateUpdateCatchDto { SessionId = 0, SpeciesId = species.Id, WeightUnit = WeightUnit.Kilograms, WeightValue = 2.5m },
                new CreateUpdateCatchDto { SessionId = 0, SpeciesId = species.Id, WeightUnit = WeightUnit.Kilograms, WeightValue = 1.1m },
            },
        });

        var list = await _sessionAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        var listedSession = list.Items.ShouldHaveSingleItem();

        listedSession.Id.ShouldBe(created.Id);
        listedSession.Catches.Count.ShouldBe(2);
        listedSession.Catches.ShouldAllBe(c => c.SpeciesName == species.Name);
        listedSession.IsBlank.ShouldBeFalse();
    }

    // IsBlank isn't a client-supplied flag - it's derived by SessionAppService from
    // whether the session ends up with zero catches, so the angler never has to
    // separately tick a "this was a blank" checkbox.
    [Fact]
    public async Task IsBlank_Should_Be_True_When_Created_With_No_Catches()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Salmon Cut " + Guid.NewGuid() });

        var created = await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-2),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>(),
        });

        created.IsBlank.ShouldBeTrue();

        var single = await _sessionAppService.GetAsync(created.Id);
        single.IsBlank.ShouldBeTrue();

        var list = await _sessionAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        list.Items.Single(x => x.Id == created.Id).IsBlank.ShouldBeTrue();
    }

    [Fact]
    public async Task IsBlank_Should_Be_Recomputed_Whenever_A_Session_Is_Updated()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Ufton Canal " + Guid.NewGuid() });
        var species = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Barbel " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });

        var created = await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-2),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>(),
        });
        created.IsBlank.ShouldBeTrue();

        var updatedWithCatch = await _sessionAppService.UpdateAsync(created.Id, new CreateUpdateSessionDto
        {
            StartDateTime = created.StartDateTime,
            EndDateTime = created.EndDateTime,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>
            {
                new CreateUpdateCatchDto { SessionId = created.Id, SpeciesId = species.Id, WeightUnit = WeightUnit.Kilograms, WeightValue = 3.2m },
            },
        });
        updatedWithCatch.IsBlank.ShouldBeFalse();

        var updatedBackToBlank = await _sessionAppService.UpdateAsync(created.Id, new CreateUpdateSessionDto
        {
            StartDateTime = created.StartDateTime,
            EndDateTime = created.EndDateTime,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>(),
        });
        updatedBackToBlank.IsBlank.ShouldBeTrue();
    }

    // No targets is how "Any" is stored - there's no sentinel species row for it.
    [Fact]
    public async Task TargetSpecies_Should_Be_Empty_When_None_Given()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Any Water " + Guid.NewGuid() });

        var created = await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-2),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>(),
        });

        created.TargetSpeciesIds.ShouldBeEmpty();
        created.TargetSpeciesNames.ShouldBeEmpty();
        (await _sessionAppService.GetAsync(created.Id)).TargetSpeciesIds.ShouldBeEmpty();
    }

    [Fact]
    public async Task TargetSpecies_Should_Round_Trip_And_Be_Replaceable_On_Update()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Target Lake " + Guid.NewGuid() });
        var carp = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Carp " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var pike = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Pike " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });
        var tench = await _speciesAppService.CreateAsync(new SpeciesUpdateDto { Name = "Tench " + Guid.NewGuid(), WaterType = SpeciesWaterType.Freshwater });

        var created = await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-3),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            TargetSpeciesIds = new List<int> { carp.Id, pike.Id },
            Catches = new List<CreateUpdateCatchDto>(),
        });

        created.TargetSpeciesIds.OrderBy(x => x).ShouldBe(new[] { carp.Id, pike.Id }.OrderBy(x => x));

        var listed = (await _sessionAppService.GetListAsync(new PagedAndSortedResultRequestDto())).Items.ShouldHaveSingleItem();
        listed.TargetSpeciesIds.Count.ShouldBe(2);
        listed.TargetSpeciesNames.ShouldBe(new[] { carp.Name, pike.Name }.OrderBy(n => n));

        // Pike dropped, tench added, carp untouched.
        var updated = await _sessionAppService.UpdateAsync(created.Id, new CreateUpdateSessionDto
        {
            StartDateTime = created.StartDateTime,
            EndDateTime = created.EndDateTime,
            VenueId = venue.Id,
            TargetSpeciesIds = new List<int> { carp.Id, tench.Id },
            Catches = new List<CreateUpdateCatchDto>(),
        });
        updated.TargetSpeciesIds.OrderBy(x => x).ShouldBe(new[] { carp.Id, tench.Id }.OrderBy(x => x));

        // Back to "Any".
        var cleared = await _sessionAppService.UpdateAsync(created.Id, new CreateUpdateSessionDto
        {
            StartDateTime = created.StartDateTime,
            EndDateTime = created.EndDateTime,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>(),
        });
        cleared.TargetSpeciesIds.ShouldBeEmpty();
    }

    [Fact]
    public async Task TargetSpecies_Should_Reject_An_Unknown_Species()
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Ghost Pond " + Guid.NewGuid() });

        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() =>
            _sessionAppService.CreateAsync(new CreateUpdateSessionDto
            {
                StartDateTime = DateTime.Now.AddHours(-1),
                EndDateTime = DateTime.Now,
                VenueId = venue.Id,
                TargetSpeciesIds = new List<int> { 999999 },
                Catches = new List<CreateUpdateCatchDto>(),
            }));
    }
}
