using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;

namespace SessionLogger.PersonalBests;

public class PersonalBestDto
{
    public int SpeciesId { get; set; }

    public decimal? WeightG { get; set; }

    // The unit the record-weight catch was originally entered in - kept purely for
    // display, same reasoning as Catch.WeightUnit itself.
    public WeightUnit? WeightUnit { get; set; }

    // Photo of the record-weight catch, if it has one - lets the species tile show
    // the actual fish rather than a generic default image.
    public int? WeightPhotoId { get; set; }

    public decimal? LengthMm { get; set; }

    public SizeUnit? LengthUnit { get; set; }

    // Photo of the record-length catch - a separate field from WeightPhotoId since
    // the heaviest and longest catch of a species needn't be the same fish.
    public int? LengthPhotoId { get; set; }
}
