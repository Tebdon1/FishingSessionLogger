using System;
using Volo.Abp.Domain.Entities;

namespace SessionLogger.Domain.UserPreferences;

// Id is the user's own Id, not a generated key - one row per user, so a lookup is
// a straight primary-key read and "does this user have a preference yet" collapses
// to "does this row exist".
public class UserPreference : AggregateRoot<Guid>
{
    public UserPreference(Guid id) : base(id)
    {
    }

    public UserPreference()
    {
    }

    // Which metric (weight or length) personal-best tiles show - every catch already
    // records both where available, this only controls which one the angler sees.
    public PersonalBestMetric PersonalBestMetric { get; set; }

    // Only used for values with no catch of their own to inherit a unit from (e.g. an
    // average across many catches) - a specific catch's own weight/length always
    // displays in whatever unit it was actually entered in, never converted.
    public WeightUnitPreference PreferredWeightUnit { get; set; }

    public LengthUnitPreference PreferredLengthUnit { get; set; }
}
