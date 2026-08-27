namespace SessionLogger.Domain.Catches;

public enum WeightUnit
{
    // Compound unit - uses WeightLbs + WeightOz rather than a single decimal value,
    // since UK anglers conventionally write weight that way (e.g. "16lb 4oz").
    LbOz = 0,
    Kilograms = 1,
    Grams = 2
}
