using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Domain.Venues;
using SessionLogger.Venues;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Statistics;

// Computed on read from Session/Catch, scoped to the current user - same
// "materialize once, derive many things" approach as PersonalBestAppService,
// since a personal fishing log's catch history is small enough that this is
// simpler and cheaper than a dozen separate SQL aggregate queries.
public class StatisticsAppService : SessionLoggerAppService, IStatisticsAppService
{
    private readonly IRepository<Session, int> _sessionRepository;
    private readonly IRepository<Venue, int> _venueRepository;
    private readonly IRepository<Species, int> _speciesRepository;

    public StatisticsAppService(
        IRepository<Session, int> sessionRepository,
        IRepository<Venue, int> venueRepository,
        IRepository<Species, int> speciesRepository)
    {
        _sessionRepository = sessionRepository;
        _venueRepository = venueRepository;
        _speciesRepository = speciesRepository;
    }

    // Venues, species, baits and methods are soft-deleted, not removed, so past sessions
    // and catches still count towards the numbers. With the soft-delete filter left on,
    // EF would drop every session at a deleted venue and every catch of a deleted species.
    public async Task<StatisticsDto> GetAsync(GetStatisticsInput input)
    {
        using (DataFilter.Disable<ISoftDelete>())
        {
            return await BuildStatisticsAsync(input);
        }
    }

    private async Task<StatisticsDto> BuildStatisticsAsync(GetStatisticsInput input)
    {
        var query = await _sessionRepository.GetQueryableAsync();
        // Species is deliberately not Included: that loads the whole species row, photo
        // blob and all, and in a join it's repeated once per catch - hundreds of copies of
        // the same few images read from SQL Server on every load of this page. Only the
        // name is used, so it's attached below from an Id/Name-only query instead.
        // AsNoTracking since nothing here is ever saved - and so attaching a detached
        // species to each catch can't be mistaken by EF for a new row to insert.
        // Session filters are applied in SQL; the catch filters just below, in memory, once
        // the sessions' catches are loaded (see GetStatisticsInput for what each means).
        var venueIds = input.VenueIds ?? new List<int>();
        var waterTypes = input.WaterTypes ?? new List<WaterType>();
        var ticketIds = input.TicketIds ?? new List<int>();

        var sessionQuery = query
            .AsNoTracking()
            .Include(s => s.Venue)
            .Include(s => s.Catches).ThenInclude(c => c.Bait)
            .Include(s => s.Catches).ThenInclude(c => c.Method)
            .Where(s => s.CreatorId == CurrentUser.Id)
            .Where(s => input.DateFrom == null || s.StartDateTime >= input.DateFrom.Value)
            .Where(s => input.DateTo == null || s.StartDateTime <= input.DateTo.Value)
            .Where(s => venueIds.Count == 0 || venueIds.Contains(s.VenueId))
            .Where(s => waterTypes.Count == 0
                || (s.Venue.WaterType != null && waterTypes.Contains(s.Venue.WaterType.Value)))
            .Where(s => ticketIds.Count == 0
                || (s.Venue.TicketId != null && ticketIds.Contains(s.Venue.TicketId.Value)));

        var sessions = sessionQuery.ToList();
        sessions = ApplyCatchFilters(sessions, input);

        // A species filter alone also counts the blank sessions where that species was the
        // target - a blank has no catch to match on, but "I went for carp and blanked" is
        // part of the carp story. Any bait/method/rig filter rules them out again, since a
        // blank has no bait, method or rig to match.
        var speciesIds = input.SpeciesIds ?? new List<int>();
        var onlySpeciesFilter = speciesIds.Count > 0
            && (input.BaitIds ?? new List<int>()).Count == 0
            && (input.MethodIds ?? new List<int>()).Count == 0
            && (input.RigIds ?? new List<int>()).Count == 0;
        if (onlySpeciesFilter)
        {
            var targetedBlanks = sessionQuery
                .Where(s => s.IsBlank && s.TargetSpecies.Any(t => speciesIds.Contains(t.Id)))
                .ToList();
            sessions = sessions.Concat(targetedBlanks).OrderBy(s => s.Id).ToList();
        }

        var speciesQuery = await _speciesRepository.GetQueryableAsync();
        var speciesById = speciesQuery
            .AsNoTracking()
            .Select(s => new { s.Id, s.Name })
            .ToList()
            .ToDictionary(s => s.Id, s => new Species(s.Id) { Name = s.Name });
        foreach (var session in sessions)
        {
            foreach (var c in session.Catches)
            {
                c.Species = speciesById.GetValueOrDefault(c.SpeciesId);
            }
        }

        // (Session, Catch) pairs rather than a plain catch list - most of the
        // breakdowns below need the parent session's date or venue, and a catch
        // doesn't carry its own date (see Catch.CatchTime - time of day only).
        var catches = sessions
            .SelectMany(s => s.Catches.Select(c => (Session: s, Catch: c)))
            .ToList();

        // Built from all of the user's venues rather than just the ones in range, so a
        // venue's label doesn't change with the date filter - see VenueLabels.
        var venueQuery = await _venueRepository.GetQueryableAsync();
        var venueLabels = VenueLabels.From(
            venueQuery.Include(v => v.Ticket).Where(v => v.CreatorId == CurrentUser.Id).ToList());

        return new StatisticsDto
        {
            Summary = BuildSummary(sessions, catches),
            CatchesByMonth = BuildCatchesByMonth(sessions, catches),
            SessionsByMonth = BuildSessionsByMonth(sessions),
            CatchesBySpecies = BuildNamedCounts(catches, x => x.Catch.Species?.Name),
            CatchesByVenue = BuildCatchesByVenue(catches, venueLabels),
            CatchesByBait = BuildNamedCounts(catches, x => x.Catch.Bait?.Name),
            PersonalBests = BuildPersonalBests(catches, venueLabels),
            TopVenues = BuildTopVenues(sessions, venueLabels),
            TopSpecies = BuildTopSpecies(catches),
            CatchesByMethod = BuildNamedCounts(catches, x => x.Catch.Method?.Name),
            CatchesByVenueWaterType = BuildNamedCounts(catches, x => x.Session.Venue?.WaterType?.ToString()),
            CatchesByMonthOfYear = BuildCatchesByMonthOfYear(catches),
            SessionsByMonthOfYear = BuildSessionsByMonthOfYear(sessions),
            TopBaits = BuildEffectiveness(catches, x => x.Catch.BaitId, x => x.Catch.Bait?.Name),
            TopMethods = BuildEffectiveness(catches, x => x.Catch.MethodId, x => x.Catch.Method?.Name),
            CatchesByDayOfWeek = BuildCatchesByDayOfWeek(catches),
            CatchesByHourOfDay = BuildCatchesByHourOfDay(catches),
            CatchesMissingTimeCount = catches.Count(x => x.Catch.CatchTime == null),
            SpeciesProgressions = BuildSpeciesProgressions(catches, venueLabels)
        };
    }

    // Narrows each session's catches to the ones matching every active catch filter, and
    // drops the sessions left with none. Done in memory, on sessions loaded without
    // tracking, so every figure built from them afterwards (counts, rates, per-venue and
    // per-species breakdowns, personal bests) is automatically about the matching catches
    // only, with no second copy of the filtering to keep in step.
    //
    // A catch with no bait/method/rig recorded can't match a filter on that - it has
    // nothing to match against.
    private static List<Session> ApplyCatchFilters(List<Session> sessions, GetStatisticsInput input)
    {
        var speciesIds = input.SpeciesIds ?? new List<int>();
        var baitIds = input.BaitIds ?? new List<int>();
        var methodIds = input.MethodIds ?? new List<int>();
        var rigIds = input.RigIds ?? new List<int>();

        if (speciesIds.Count == 0 && baitIds.Count == 0 && methodIds.Count == 0 && rigIds.Count == 0)
        {
            return sessions;
        }

        bool Matches(Catch c) =>
            (speciesIds.Count == 0 || speciesIds.Contains(c.SpeciesId))
            && (baitIds.Count == 0 || (c.BaitId != null && baitIds.Contains(c.BaitId.Value)))
            && (methodIds.Count == 0 || (c.MethodId != null && methodIds.Contains(c.MethodId.Value)))
            && (rigIds.Count == 0 || (c.RigId != null && rigIds.Contains(c.RigId.Value)));

        foreach (var session in sessions)
        {
            session.Catches = session.Catches.Where(Matches).ToList();
        }

        return sessions.Where(s => s.Catches.Count > 0).ToList();
    }

    private static StatisticsSummaryDto BuildSummary(
        List<Session> sessions,
        List<(Session Session, Catch Catch)> catches)
    {
        var totalHours = sessions.Sum(s => (double)s.Duration);
        var blankCount = sessions.Count(s => s.IsBlank);

        return new StatisticsSummaryDto
        {
            TotalSessions = sessions.Count,
            TotalCatches = catches.Count,
            TotalHoursFishing = totalHours,
            CatchRatePerHour = totalHours > 0 ? catches.Count / totalHours : null,
            BlankRatePercent = sessions.Count > 0 ? blankCount * 100.0 / sessions.Count : 0,
            DistinctSpeciesCount = catches.Select(x => x.Catch.SpeciesId).Distinct().Count()
        };
    }

    // Walks every month from the first to the last session (not just months that
    // had catches), so a quiet month shows as a real zero instead of a gap that
    // looks like missing data.
    private static List<MonthlyCatchesDto> BuildCatchesByMonth(
        List<Session> sessions,
        List<(Session Session, Catch Catch)> catches)
    {
        if (sessions.Count == 0)
        {
            return new List<MonthlyCatchesDto>();
        }

        var countsByMonth = catches
            .GroupBy(x => FirstOfMonth(x.Session.StartDateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<MonthlyCatchesDto>();
        var running = 0;
        foreach (var month in MonthRange(sessions))
        {
            var count = countsByMonth.TryGetValue(month, out var c) ? c : 0;
            running += count;
            result.Add(new MonthlyCatchesDto
            {
                Year = month.Year,
                Month = month.Month,
                CatchCount = count,
                CumulativeCatchCount = running
            });
        }

        return result;
    }

    private static List<MonthlySessionsDto> BuildSessionsByMonth(List<Session> sessions)
    {
        if (sessions.Count == 0)
        {
            return new List<MonthlySessionsDto>();
        }

        var byMonth = sessions
            .GroupBy(s => FirstOfMonth(s.StartDateTime))
            .ToDictionary(g => g.Key, g => (Blank: g.Count(s => s.IsBlank), Productive: g.Count(s => !s.IsBlank)));

        return MonthRange(sessions)
            .Select(month =>
            {
                var (blank, productive) = byMonth.TryGetValue(month, out var v) ? v : (0, 0);
                return new MonthlySessionsDto
                {
                    Year = month.Year,
                    Month = month.Month,
                    BlankSessionCount = blank,
                    ProductiveSessionCount = productive
                };
            })
            .ToList();
    }

    private static List<NamedCountDto> BuildNamedCounts(
        List<(Session Session, Catch Catch)> catches,
        Func<(Session Session, Catch Catch), string> nameSelector)
    {
        return catches
            .Select(nameSelector)
            .Where(name => name != null)
            .GroupBy(name => name)
            .Select(g => new NamedCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    // Grouped by venue id rather than by name (as BuildNamedCounts does) - two venues can
    // share a name on different tickets, and grouping by name would add their catches
    // together into one bar. The label (see VenueLabels) is what tells them apart.
    private static List<NamedCountDto> BuildCatchesByVenue(
        List<(Session Session, Catch Catch)> catches,
        VenueLabels venueLabels)
    {
        return catches
            .GroupBy(x => x.Session.VenueId)
            .Select(g => new NamedCountDto
            {
                Name = venueLabels.For(g.First().Session.Venue) ?? "Unknown",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    private static List<PersonalBestRowDto> BuildPersonalBests(
        List<(Session Session, Catch Catch)> catches,
        VenueLabels venueLabels)
    {
        return catches
            .GroupBy(x => x.Catch.SpeciesId)
            .Select(g =>
            {
                var bestWeight = g.Where(x => x.Catch.WeightG != null).OrderByDescending(x => x.Catch.WeightG).FirstOrDefault();
                var bestLength = g.Where(x => x.Catch.LengthMm != null).OrderByDescending(x => x.Catch.LengthMm).FirstOrDefault();

                return new PersonalBestRowDto
                {
                    SpeciesName = g.First().Catch.Species?.Name ?? "Unknown",
                    WeightG = bestWeight.Catch?.WeightG,
                    WeightUnit = bestWeight.Catch?.WeightUnit,
                    WeightDate = bestWeight.Session?.StartDateTime,
                    WeightVenueName = venueLabels.For(bestWeight.Session?.Venue),
                    LengthMm = bestLength.Catch?.LengthMm,
                    LengthUnit = bestLength.Catch?.LengthUnit,
                    LengthDate = bestLength.Session?.StartDateTime,
                    LengthVenueName = venueLabels.For(bestLength.Session?.Venue)
                };
            })
            .Where(x => x.WeightG != null || x.LengthMm != null)
            .OrderBy(x => x.SpeciesName)
            .ToList();
    }

    private static List<VenueStatsDto> BuildTopVenues(List<Session> sessions, VenueLabels venueLabels)
    {
        return sessions
            .GroupBy(s => s.VenueId)
            .Select(g =>
            {
                var sessionCount = g.Count();
                var catchCount = g.Sum(s => s.Catches.Count);
                var blankCount = g.Count(s => s.IsBlank);
                return new VenueStatsDto
                {
                    VenueName = venueLabels.For(g.First().Venue) ?? "Unknown",
                    SessionCount = sessionCount,
                    CatchCount = catchCount,
                    BlankRatePercent = blankCount * 100.0 / sessionCount,
                    AvgCatchesPerSession = (double)catchCount / sessionCount
                };
            })
            .OrderByDescending(x => x.CatchCount)
            .ToList();
    }

    private static List<SpeciesStatsDto> BuildTopSpecies(List<(Session Session, Catch Catch)> catches)
    {
        return catches
            .GroupBy(x => x.Catch.SpeciesId)
            .Select(g =>
            {
                var weighed = g.Where(x => x.Catch.WeightG != null).ToList();
                var bestWeight = weighed.OrderByDescending(x => x.Catch.WeightG).FirstOrDefault();
                var measured = g.Where(x => x.Catch.LengthMm != null).ToList();
                var bestLength = measured.OrderByDescending(x => x.Catch.LengthMm).FirstOrDefault();
                return new SpeciesStatsDto
                {
                    SpeciesName = g.First().Catch.Species?.Name ?? "Unknown",
                    CatchCount = g.Count(),
                    BestWeightG = bestWeight.Catch?.WeightG,
                    BestWeightUnit = bestWeight.Catch?.WeightUnit,
                    AvgWeightG = weighed.Count > 0 ? weighed.Average(x => x.Catch.WeightG) : null,
                    WeighedCount = weighed.Count,
                    BestLengthMm = bestLength.Catch?.LengthMm,
                    BestLengthUnit = bestLength.Catch?.LengthUnit,
                    AvgLengthMm = measured.Count > 0 ? measured.Average(x => x.Catch.LengthMm) : null,
                    MeasuredCount = measured.Count
                };
            })
            .OrderByDescending(x => x.CatchCount)
            .ToList();
    }

    // Summed across every year in range, not one bar per year-month - this is
    // about seasonality ("when do I catch the most"), not a timeline.
    private static List<MonthOfYearCatchesDto> BuildCatchesByMonthOfYear(List<(Session Session, Catch Catch)> catches)
    {
        var countsByMonth = catches
            .GroupBy(x => x.Session.StartDateTime.Month)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(1, 12)
            .Select(month => new MonthOfYearCatchesDto
            {
                Month = month,
                CatchCount = countsByMonth.TryGetValue(month, out var c) ? c : 0
            })
            .ToList();
    }

    // The sessions counterpart of BuildCatchesByMonthOfYear - summed across every year in
    // range, with all 12 months always present so a month never fished shows as a real
    // zero rather than a gap. Split productive/blank, matching the sessions-over-time chart.
    private static List<MonthOfYearSessionsDto> BuildSessionsByMonthOfYear(List<Session> sessions)
    {
        var byMonth = sessions
            .GroupBy(s => s.StartDateTime.Month)
            .ToDictionary(g => g.Key, g => (Blank: g.Count(s => s.IsBlank), Productive: g.Count(s => !s.IsBlank)));

        return Enumerable.Range(1, 12)
            .Select(month =>
            {
                var (blank, productive) = byMonth.TryGetValue(month, out var v) ? v : (0, 0);
                return new MonthOfYearSessionsDto
                {
                    Month = month,
                    ProductiveSessionCount = productive,
                    BlankSessionCount = blank
                };
            })
            .ToList();
    }

    // Generic over bait/method: groups catches by whichever FK isn't null, then
    // counts the distinct sessions each one appeared in alongside the raw catch
    // count.
    private static List<EffectivenessDto> BuildEffectiveness(
        List<(Session Session, Catch Catch)> catches,
        Func<(Session Session, Catch Catch), int?> idSelector,
        Func<(Session Session, Catch Catch), string> nameSelector)
    {
        return catches
            .Where(x => idSelector(x) != null)
            .GroupBy(idSelector)
            .Select(g => new EffectivenessDto
            {
                Name = nameSelector(g.First()) ?? "Unknown",
                SessionsUsedCount = g.Select(x => x.Session.Id).Distinct().Count(),
                CatchCount = g.Count()
            })
            .OrderByDescending(x => x.CatchCount)
            .ToList();
    }

    // Monday first, all 7 days always present regardless of whether every day of
    // the week has been fished yet.
    private static List<NamedCountDto> BuildCatchesByDayOfWeek(List<(Session Session, Catch Catch)> catches)
    {
        var countsByDay = catches
            .GroupBy(x => x.Session.StartDateTime.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(0, 7)
            .Select(offset => (DayOfWeek)(((int)DayOfWeek.Monday + offset) % 7))
            .Select(day => new NamedCountDto
            {
                Name = day.ToString(),
                Count = countsByDay.TryGetValue(day, out var c) ? c : 0
            })
            .ToList();
    }

    // Only catches with a logged time of day - see StatisticsDto.CatchesMissingTimeCount
    // for how many were excluded, since plenty of bulk-logged catches skip this.
    private static List<HourOfDayCatchesDto> BuildCatchesByHourOfDay(List<(Session Session, Catch Catch)> catches)
    {
        var countsByHour = catches
            .Where(x => x.Catch.CatchTime != null)
            .GroupBy(x => x.Catch.CatchTime.Value.Hours)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(0, 24)
            .Select(hour => new HourOfDayCatchesDto
            {
                Hour = hour,
                CatchCount = countsByHour.TryGetValue(hour, out var c) ? c : 0
            })
            .ToList();
    }

    // Only species with at least 2 weighed catches - a single point has nothing
    // to show "progression" against.
    private static List<SpeciesProgressionDto> BuildSpeciesProgressions(
        List<(Session Session, Catch Catch)> catches,
        VenueLabels venueLabels)
    {
        return catches
            .Where(x => x.Catch.WeightG != null)
            .GroupBy(x => x.Catch.SpeciesId)
            .Where(g => g.Count() >= 2)
            .Select(g =>
            {
                var points = new List<CatchPointDto>();
                var runningBest = 0m;
                foreach (var x in g.OrderBy(x => x.Session.StartDateTime))
                {
                    var isPb = x.Catch.WeightG.Value > runningBest;
                    if (isPb)
                    {
                        runningBest = x.Catch.WeightG.Value;
                    }

                    points.Add(new CatchPointDto
                    {
                        Date = x.Session.StartDateTime,
                        WeightG = x.Catch.WeightG.Value,
                        WeightUnit = x.Catch.WeightUnit,
                        IsPersonalBestAtTime = isPb,
                        VenueName = venueLabels.For(x.Session.Venue)
                    });
                }

                return new SpeciesProgressionDto
                {
                    SpeciesName = g.First().Catch.Species?.Name ?? "Unknown",
                    Points = points
                };
            })
            .OrderBy(x => x.SpeciesName)
            .ToList();
    }

    private static DateTime FirstOfMonth(DateTime date) => new DateTime(date.Year, date.Month, 1);

    private static IEnumerable<DateTime> MonthRange(List<Session> sessions)
    {
        var start = FirstOfMonth(sessions.Min(s => s.StartDateTime));
        var end = FirstOfMonth(sessions.Max(s => s.StartDateTime));

        for (var month = start; month <= end; month = month.AddMonths(1))
        {
            yield return month;
        }
    }
}
