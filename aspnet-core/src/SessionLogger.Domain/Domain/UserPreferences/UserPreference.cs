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
}
