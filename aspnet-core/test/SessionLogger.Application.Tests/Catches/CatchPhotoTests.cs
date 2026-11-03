using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.PersonalBests;
using SessionLogger.Sessions;
using SessionLogger.SpeciesTypes;
using SessionLogger.Venues;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SessionLogger.Catches;

// A catch's photo bytes live in a separate FileContent entity (sharing the AppFile row)
// so that loading a session, or the personal-best calculation, reads the photo's details
// without reading the image. These cover that the photo still round-trips - saved, kept
// across an edit, replaced, cleared, served, surfaced as a personal-best photo - and that
// an ordinary load of a session really does leave the image alone.
public class CatchPhotoTests : SessionLoggerApplicationTestBase
{
    private static readonly byte[] PhotoBytes = { 10, 20, 30, 40, 50 };

    private readonly ISessionAppService _sessionAppService;
    private readonly IVenueAppService _venueAppService;
    private readonly ISpeciesAppService _speciesAppService;
    private readonly IPersonalBestAppService _personalBestAppService;
    private readonly CatchPhotoAppService _photoAppService;
    private readonly IRepository<Session, int> _sessionRepository;

    public CatchPhotoTests()
    {
        _sessionAppService = GetRequiredService<ISessionAppService>();
        _venueAppService = GetRequiredService<IVenueAppService>();
        _speciesAppService = GetRequiredService<ISpeciesAppService>();
        _personalBestAppService = GetRequiredService<IPersonalBestAppService>();
        _photoAppService = GetRequiredService<CatchPhotoAppService>();
        _sessionRepository = GetRequiredService<IRepository<Session, int>>();
    }

    [Fact]
    public async Task A_Saved_Catch_Photo_Should_Be_Reported_And_Served()
    {
        var session = await CreateSessionWithPhotoAsync("heron.jpg");

        var fetched = await _sessionAppService.GetAsync(session.Id);
        var catchDto = fetched.Catches.ShouldHaveSingleItem();
        catchDto.PhotoId.ShouldNotBeNull();
        catchDto.PhotoFileName.ShouldBe("heron.jpg");

        var served = await GetPhotoAsync(catchDto.PhotoId!.Value);
        served.FileName.ShouldBe("heron.jpg");
        served.ContentType.ShouldBe("image/jpeg");
        ReadAll(served.GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task Resending_A_Catch_Without_New_Photo_Data_Should_Keep_Its_Photo()
    {
        var session = await CreateSessionWithPhotoAsync("heron.jpg");
        var before = (await _sessionAppService.GetAsync(session.Id)).Catches.Single();

        // What the frontend does for a catch it isn't editing: the id and the details, but
        // no PhotoData - which means "leave the photo as it is".
        var update = ToUpdate(await _sessionAppService.GetAsync(session.Id), photoData: null);
        await _sessionAppService.UpdateAsync(session.Id, update);

        var after = (await _sessionAppService.GetAsync(session.Id)).Catches.Single();
        after.PhotoId.ShouldBe(before.PhotoId);
        ReadAll((await GetPhotoAsync(after.PhotoId!.Value)).GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task Uploading_A_New_Photo_For_A_Catch_Should_Replace_The_Old_One()
    {
        var session = await CreateSessionWithPhotoAsync("old.jpg");

        var replacement = new byte[] { 99, 98, 97 };
        var update = ToUpdate(await _sessionAppService.GetAsync(session.Id), photoData: replacement, fileName: "new.png");
        await _sessionAppService.UpdateAsync(session.Id, update);

        var catchDto = (await _sessionAppService.GetAsync(session.Id)).Catches.Single();
        catchDto.PhotoFileName.ShouldBe("new.png");
        ReadAll((await GetPhotoAsync(catchDto.PhotoId!.Value)).GetStream()).ShouldBe(replacement);
    }

    [Fact]
    public async Task RemovePhoto_Should_Delete_A_Catchs_Photo()
    {
        var session = await CreateSessionWithPhotoAsync("heron.jpg");
        var photoId = (await _sessionAppService.GetAsync(session.Id)).Catches.Single().PhotoId!.Value;

        var update = ToUpdate(await _sessionAppService.GetAsync(session.Id), photoData: null, removePhoto: true);
        await _sessionAppService.UpdateAsync(session.Id, update);

        (await _sessionAppService.GetAsync(session.Id)).Catches.Single().PhotoId.ShouldBeNull();
        await Should.ThrowAsync<EntityNotFoundException>(() => GetPhotoAsync(photoId));
    }

    [Fact]
    public async Task Adding_A_Photo_To_A_Catch_That_Had_None_Should_Work()
    {
        var session = await CreateSessionAsync(photoData: null, fileName: null);
        (await _sessionAppService.GetAsync(session.Id)).Catches.Single().PhotoId.ShouldBeNull();

        var update = ToUpdate(await _sessionAppService.GetAsync(session.Id), photoData: PhotoBytes, fileName: "late.jpg");
        await _sessionAppService.UpdateAsync(session.Id, update);

        var catchDto = (await _sessionAppService.GetAsync(session.Id)).Catches.Single();
        catchDto.PhotoId.ShouldNotBeNull();
        ReadAll((await GetPhotoAsync(catchDto.PhotoId!.Value)).GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task The_Personal_Best_Should_Point_At_The_Record_Catchs_Photo()
    {
        var session = await CreateSessionWithPhotoAsync("record.jpg");
        var photoId = (await _sessionAppService.GetAsync(session.Id)).Catches.Single().PhotoId;

        var bests = await _personalBestAppService.GetListAsync();

        bests.ShouldHaveSingleItem().WeightPhotoId.ShouldBe(photoId);
    }

    // The point of the split: loading a session (it Includes its catches' photos by default,
    // for editing) must not read the photo bytes. If someone later adds Content to a default
    // Include, or maps the byte columns back onto File, this is what notices.
    [Fact]
    public async Task Loading_A_Session_Should_Load_Its_Photos_Details_But_Not_Their_Bytes()
    {
        var session = await CreateSessionWithPhotoAsync("heron.jpg");

        await WithUnitOfWorkAsync(async () =>
        {
            var loaded = await _sessionRepository.GetAsync(session.Id);
            var photo = loaded.Catches.Single().Photo;

            photo.ShouldNotBeNull();
            photo!.FileName.ShouldBe("heron.jpg");
            photo.Size.ShouldBe(PhotoBytes.Length);
            photo.Content.ShouldBeNull();

            var withContent = await _sessionRepository.WithDetailsAsync(x => x.Catches);
            var explicitlyLoaded = await withContent
                .Include(x => x.Catches).ThenInclude(c => c.Photo).ThenInclude(p => p.Content)
                .SingleAsync(x => x.Id == session.Id);
            explicitlyLoaded.Catches.Single().Photo!.Content!.Data.ShouldBe(PhotoBytes);
        });
    }

    private Task<SessionDto> CreateSessionWithPhotoAsync(string fileName)
    {
        return CreateSessionAsync(PhotoBytes, fileName);
    }

    private async Task<SessionDto> CreateSessionAsync(byte[] photoData, string fileName)
    {
        var venue = await _venueAppService.CreateAsync(new VenueUpdateDto { Name = "Willow Lake " + Guid.NewGuid() });
        var species = await _speciesAppService.CreateAsync(new SpeciesUpdateDto
        {
            Name = "Pike " + Guid.NewGuid(),
            WaterType = SpeciesWaterType.Freshwater,
        });

        return await _sessionAppService.CreateAsync(new CreateUpdateSessionDto
        {
            StartDateTime = DateTime.Now.AddHours(-4),
            EndDateTime = DateTime.Now,
            VenueId = venue.Id,
            Catches = new List<CreateUpdateCatchDto>
            {
                new CreateUpdateCatchDto
                {
                    SessionId = 0,
                    SpeciesId = species.Id,
                    WeightUnit = WeightUnit.Kilograms,
                    WeightValue = 4.2m,
                    PhotoData = photoData,
                    PhotoFileName = fileName,
                },
            },
        });
    }

    // Rebuilds an update from what the API returned, the way the frontend does for a
    // session it's editing: each catch resent by id with its weight in its stored unit.
    private static CreateUpdateSessionDto ToUpdate(
        SessionDto session, byte[] photoData, string fileName = null, bool removePhoto = false)
    {
        return new CreateUpdateSessionDto
        {
            StartDateTime = session.StartDateTime,
            EndDateTime = session.EndDateTime,
            VenueId = session.VenueId,
            Notes = session.Notes,
            Catches = session.Catches.Select(c => new CreateUpdateCatchDto
            {
                Id = c.Id,
                SessionId = session.Id,
                SpeciesId = c.SpeciesId,
                WeightUnit = WeightUnit.Kilograms,
                WeightValue = c.WeightG / 1000m,
                PhotoData = photoData,
                PhotoFileName = fileName,
                RemovePhoto = removePhoto,
            }).ToList(),
        };
    }

    // CatchPhotoAppService has no interface, so resolving it straight from the container
    // skips the unit of work ABP wraps around a real request - run it inside one, as the
    // HTTP pipeline would, or its database context is disposed before the query runs.
    private Task<IRemoteStreamContent> GetPhotoAsync(int id)
    {
        return WithUnitOfWorkAsync(() => _photoAppService.GetAsync(id));
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
