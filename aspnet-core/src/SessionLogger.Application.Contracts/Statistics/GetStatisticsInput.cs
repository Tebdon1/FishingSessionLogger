using System;
using System.Collections.Generic;
using SessionLogger.Domain.Venues;

namespace SessionLogger.Statistics;

// Every filter is optional, and a list left null or empty means "no filter on this" - not
// "match nothing". Within one list the values are "or" (a session at any of the chosen
// venues); across filters they're "and".
//
// Two kinds, which behave differently:
//  - Session filters (dates, venues, water types, tickets) say which SESSIONS count. They
//    keep blank sessions, since a blank session still happened at a venue on a date.
//  - Catch filters (species, baits, methods, rigs) say which CATCHES count. A catch has to
//    satisfy all of the catch filters at once (the species AND the bait, on the same fish),
//    and a session only counts if it has at least one such catch - so while one is active,
//    blank sessions are left out (a session with no catches can't have a matching one), and
//    every figure on the page is about the matching catches only.
//    The one exception: when species is the only catch filter, blank sessions that
//    targeted one of the chosen species are counted as well (see Session.TargetSpecies) -
//    they have no catches, so they add to the session figures and the blank rate, not to
//    any catch figure.
public class GetStatisticsInput
{
    // Filters on Session.StartDateTime - both ends inclusive, either or both may be
    // omitted for an open-ended range.
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    // Session filters
    public List<int>? VenueIds { get; set; }

    // The water type of the session's venue.
    public List<WaterType>? WaterTypes { get; set; }

    // The ticket of the session's venue.
    public List<int>? TicketIds { get; set; }

    // Catch filters
    public List<int>? SpeciesIds { get; set; }

    public List<int>? BaitIds { get; set; }

    public List<int>? MethodIds { get; set; }

    public List<int>? RigIds { get; set; }
}
