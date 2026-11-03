using SessionLogger.Domain.Venues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SessionLogger.Venues;

// The label a venue is shown under wherever it appears by name (session cards, venue
// picker, statistics). A venue's Name is the place and its ticket is who controls it, so
// two venues can legitimately share a name on different tickets (e.g. a canal stretch
// split between two clubs) - in that case, and only then, the ticket is appended so the
// two stay distinguishable: "K&A Canal Midgham (RTAA)". A venue whose name is unique
// among the user's venues keeps its plain name.
//
// Built from ALL of a user's venues, not just the ones in the current result - otherwise
// a venue's label would change depending on whether its namesake happened to be in view.
public class VenueLabels
{
    private readonly Dictionary<int, string> _labels;

    private VenueLabels(Dictionary<int, string> labels)
    {
        _labels = labels;
    }

    // Venues must have their Ticket loaded.
    public static VenueLabels From(IEnumerable<Venue> venues)
    {
        var all = venues.ToList();

        var sharedNames = all
            .GroupBy(v => NormalizeName(v.Name))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        var labels = all.ToDictionary(
            v => v.Id,
            v => sharedNames.Contains(NormalizeName(v.Name))
                ? $"{v.Name} ({v.Ticket?.Name ?? "No ticket"})"
                : v.Name);

        return new VenueLabels(labels);
    }

    public string For(Venue venue)
    {
        if (venue == null)
        {
            return null;
        }

        return _labels.TryGetValue(venue.Id, out var label) ? label : venue.Name;
    }

    public string For(int venueId, string fallbackName)
    {
        return _labels.TryGetValue(venueId, out var label) ? label : fallbackName;
    }

    // Same comparison the duplicate check in VenueAppService uses.
    public static string NormalizeName(string name)
    {
        return (name ?? string.Empty).Trim().ToUpperInvariant();
    }
}
