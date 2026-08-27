namespace SessionLogger.Domain.Rigs;

// Separate from Catches.WeightUnit - jighead weight is virtually always quoted in
// grams or ounces, never as a compound lb+oz value, so this is a simpler two-way scale.
public enum HookWeightUnit
{
    Grams = 0,
    Ounces = 1
}
