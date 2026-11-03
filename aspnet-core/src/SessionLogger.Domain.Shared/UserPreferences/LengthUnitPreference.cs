namespace SessionLogger.Domain.UserPreferences;

// Distinct from Baits.SizeUnit - a specific catch always keeps whatever unit it was
// entered in. This only picks a family for values with no catch of their own (e.g.
// an average); Metric auto-scales between mm/cm/m by magnitude rather than pinning
// to one fixed unit.
public enum LengthUnitPreference
{
    Imperial = 0,
    Metric = 1
}
