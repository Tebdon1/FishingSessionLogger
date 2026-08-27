using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Baits;

public class Bait : AuditedAggregateRoot<int>
{
    public Bait(int id) : base(id)
    {

    }

    public Bait()
    {

    }

    // Computed by BaitAppService from the fields below (or, for Natural, from the
    // free-typed name) - never set directly from a client request.
    public string Name { get; set; }

    public BaitType BaitType { get; set; }

    // Lure and Bait only
    public string? Brand { get; set; }
    public string? Range { get; set; }

    // Lure only
    public string? Colour { get; set; }

    // Bait only
    public string? Flavour { get; set; }

    // Lure and Bait only. Always millimetres, converted from whatever unit the
    // caller entered - so every comparison/aggregation is a plain numeric one,
    // with no unit-conversion logic needed at query time.
    public decimal? SizeMm { get; set; }

    // The unit SizeMm was originally entered in, kept purely for display (e.g. show
    // "6in" instead of silently reformatting to "152.4mm") - never used for
    // comparisons, which always go through the canonical SizeMm. Null for rows
    // created before this field existed; display falls back to mm for those.
    public SizeUnit? SizeUnit { get; set; }
}
