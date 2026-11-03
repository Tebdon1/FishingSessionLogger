using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SessionLogger.Domain.SpeciesTypes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SessionLogger.SpeciesTypes;

// A species' default photo is a separate SpeciesPhoto entity (sharing the AppSpecies row)
// so that loading a species doesn't read the image. These cover that the photo still
// round-trips - saved, replaced, cleared, served - and that it really is left alone by
// an ordinary species load.
public class SpeciesPhotoTests : SessionLoggerApplicationTestBase
{
    private static readonly byte[] PhotoBytes = { 1, 2, 3, 4, 5 };

    private readonly ISpeciesAppService _speciesAppService;
    private readonly SpeciesPhotoAppService _photoAppService;
    private readonly IRepository<Species, int> _speciesRepository;

    public SpeciesPhotoTests()
    {
        _speciesAppService = GetRequiredService<ISpeciesAppService>();
        _photoAppService = GetRequiredService<SpeciesPhotoAppService>();
        _speciesRepository = GetRequiredService<IRepository<Species, int>>();
    }

    [Fact]
    public async Task Created_Species_Without_A_Photo_Should_Report_No_Photo()
    {
        var species = await _speciesAppService.CreateAsync(NewSpecies());

        species.HasPhoto.ShouldBeFalse();
        (await _speciesAppService.GetAsync(species.Id)).HasPhoto.ShouldBeFalse();
        await Should.ThrowAsync<EntityNotFoundException>(() => GetPhotoAsync(species.Id));
    }

    [Fact]
    public async Task Created_Species_With_A_Photo_Should_Report_And_Serve_It()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "perch.jpg"));

        created.HasPhoto.ShouldBeTrue();
        (await _speciesAppService.GetAsync(created.Id)).HasPhoto.ShouldBeTrue();

        var listed = (await _speciesAppService.GetListAsync(new PagedAndSortedResultRequestDto())).Items
            .Single(s => s.Id == created.Id);
        listed.HasPhoto.ShouldBeTrue();

        var served = await GetPhotoAsync(created.Id);
        served.FileName.ShouldBe("perch.jpg");
        served.ContentType.ShouldBe("image/jpeg");
        ReadAll(served.GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task HasPhoto_Should_Be_Per_Species_In_A_Mixed_List()
    {
        var with = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "a.png"));
        var without = await _speciesAppService.CreateAsync(NewSpecies());

        var items = (await _speciesAppService.GetListAsync(new PagedAndSortedResultRequestDto())).Items;

        items.Single(s => s.Id == with.Id).HasPhoto.ShouldBeTrue();
        items.Single(s => s.Id == without.Id).HasPhoto.ShouldBeFalse();
    }

    [Fact]
    public async Task Editing_Only_The_Name_Should_Leave_The_Photo_Untouched()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "perch.jpg"));

        var update = NewSpecies();
        update.Name = created.Name + " renamed";
        var updated = await _speciesAppService.UpdateAsync(created.Id, update);

        updated.Name.ShouldBe(created.Name + " renamed");
        updated.HasPhoto.ShouldBeTrue();
        ReadAll((await GetPhotoAsync(created.Id)).GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task Uploading_A_New_Photo_Should_Replace_The_Old_One()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "old.jpg"));

        var replacement = new byte[] { 9, 8, 7 };
        var update = NewSpecies(replacement, "new.png");
        update.Name = created.Name;
        await _speciesAppService.UpdateAsync(created.Id, update);

        var served = await GetPhotoAsync(created.Id);
        served.FileName.ShouldBe("new.png");
        served.ContentType.ShouldBe("image/png");
        ReadAll(served.GetStream()).ShouldBe(replacement);
    }

    [Fact]
    public async Task Adding_A_Photo_To_A_Species_That_Had_None_Should_Work()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies());

        var update = NewSpecies(PhotoBytes, "late.jpg");
        update.Name = created.Name;
        var updated = await _speciesAppService.UpdateAsync(created.Id, update);

        updated.HasPhoto.ShouldBeTrue();
        ReadAll((await GetPhotoAsync(created.Id)).GetStream()).ShouldBe(PhotoBytes);
    }

    [Fact]
    public async Task RemovePhoto_Should_Clear_It_Without_Touching_The_Species()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "perch.jpg"));

        var update = NewSpecies();
        update.Name = created.Name;
        update.RemovePhoto = true;
        var updated = await _speciesAppService.UpdateAsync(created.Id, update);

        updated.HasPhoto.ShouldBeFalse();
        updated.Name.ShouldBe(created.Name);
        await Should.ThrowAsync<EntityNotFoundException>(() => GetPhotoAsync(created.Id));
    }

    // The point of the split: an ordinary load of a species must not read the image. If
    // someone later adds Photo to a default Include, or maps the photo columns back onto
    // Species, this is what notices.
    [Fact]
    public async Task Loading_A_Species_Should_Not_Load_Its_Photo()
    {
        var created = await _speciesAppService.CreateAsync(NewSpecies(PhotoBytes, "perch.jpg"));

        await WithUnitOfWorkAsync(async () =>
        {
            var plain = await _speciesRepository.GetAsync(created.Id, includeDetails: false);
            plain.Photo.ShouldBeNull();

            var query = await _speciesRepository.WithDetailsAsync(x => x.Photo);
            var withPhoto = query.Single(x => x.Id == created.Id);
            withPhoto.Photo.ShouldNotBeNull();
            withPhoto.Photo!.Data.ShouldBe(PhotoBytes);
        });
    }

    // SpeciesPhotoAppService has no interface, so resolving it straight from the container
    // skips the unit of work ABP wraps around a real request - run it inside one, as the
    // HTTP pipeline would, or its database context is disposed before the query runs.
    private Task<IRemoteStreamContent> GetPhotoAsync(int id)
    {
        return WithUnitOfWorkAsync(() => _photoAppService.GetAsync(id));
    }

    private static SpeciesUpdateDto NewSpecies(byte[] photo = null, string fileName = null)
    {
        return new SpeciesUpdateDto
        {
            Name = "Species " + Guid.NewGuid(),
            WaterType = SpeciesWaterType.Freshwater,
            PhotoData = photo,
            PhotoFileName = fileName,
        };
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
