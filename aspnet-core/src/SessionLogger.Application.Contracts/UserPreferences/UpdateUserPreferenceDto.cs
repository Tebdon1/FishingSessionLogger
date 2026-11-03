using SessionLogger.Domain.UserPreferences;

namespace SessionLogger.UserPreferences;

public class UpdateUserPreferenceDto
{
    public PersonalBestMetric PersonalBestMetric { get; set; }

    public WeightUnitPreference PreferredWeightUnit { get; set; }

    public LengthUnitPreference PreferredLengthUnit { get; set; }
}
