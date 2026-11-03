namespace SessionLogger.Domain.UserPreferences;

// Distinct from Catches.WeightUnit - a specific catch always keeps whatever unit it
// was entered in. This only picks a family for values with no catch of their own
// (e.g. an average); Metric auto-scales between g and kg by magnitude rather than
// pinning to one fixed unit.
public enum WeightUnitPreference
{
    Imperial = 0,
    Metric = 1
}
