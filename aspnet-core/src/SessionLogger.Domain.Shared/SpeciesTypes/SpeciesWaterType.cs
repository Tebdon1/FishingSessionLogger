namespace SessionLogger.Domain.SpeciesTypes;

// Named SpeciesWaterType rather than WaterType to avoid confusion with
// SessionLogger.Domain.Venues.WaterType, which classifies a venue (river, lake,
// reservoir, ...) rather than a species' habitat.
public enum SpeciesWaterType
{
    Freshwater = 0,
    Saltwater = 1,
    // Some species turn up in both (e.g. eels, sea trout) - a plain freshwater/
    // saltwater bool can't represent that without picking one arbitrarily.
    Both = 2
}
