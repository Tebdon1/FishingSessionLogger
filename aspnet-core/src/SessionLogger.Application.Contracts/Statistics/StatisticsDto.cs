using System;
using System.Collections.Generic;
using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;

namespace SessionLogger.Statistics;

public class StatisticsDto
{
    public StatisticsSummaryDto Summary { get; set; }

    public List<MonthlyCatchesDto> CatchesByMonth { get; set; }

    public List<MonthlySessionsDto> SessionsByMonth { get; set; }

    public List<NamedCountDto> CatchesBySpecies { get; set; }

    public List<NamedCountDto> CatchesByVenue { get; set; }

    // Catches with no bait recorded are left out rather than counted as an
    // "Unknown" bar - this is a breakdown of which baits produce fish, not of
    // logging completeness.
    public List<NamedCountDto> CatchesByBait { get; set; }

    public List<PersonalBestRowDto> PersonalBests { get; set; }

    public List<VenueStatsDto> TopVenues { get; set; }

    public List<SpeciesStatsDto> TopSpecies { get; set; }

    public List<NamedCountDto> CatchesByMethod { get; set; }

    // River/Lake/Reservoir/etc, taken from the session's venue - catches whose
    // venue has no water type set are left out (see CatchesByBait above for why).
    public List<NamedCountDto> CatchesByVenueWaterType { get; set; }

    // Always 12 entries (Jan-Dec), summed across every year in range - shows
    // seasonality (e.g. "pike season") rather than a plain timeline.
    public List<MonthOfYearCatchesDto> CatchesByMonthOfYear { get; set; }

    // Same idea as CatchesByMonthOfYear but counting sessions - how often the angler
    // actually fishes in each month, not how much they catch. Always 12 entries (Jan-Dec),
    // summed across every year in range.
    public List<MonthOfYearSessionsDto> SessionsByMonthOfYear { get; set; }

    public List<EffectivenessDto> TopBaits { get; set; }

    public List<EffectivenessDto> TopMethods { get; set; }

    // Always 7 entries, Monday first.
    public List<NamedCountDto> CatchesByDayOfWeek { get; set; }

    // Always 24 entries - only counts catches with a logged time of day (see
    // CatchesMissingTimeCount for how many were left out).
    public List<HourOfDayCatchesDto> CatchesByHourOfDay { get; set; }

    public int CatchesMissingTimeCount { get; set; }

    // One series per species with at least 2 weighed catches - a single point
    // isn't a "progression". The frontend picks which one to plot.
    public List<SpeciesProgressionDto> SpeciesProgressions { get; set; }
}

public class StatisticsSummaryDto
{
    public int TotalSessions { get; set; }

    public int TotalCatches { get; set; }

    public double TotalHoursFishing { get; set; }

    // Null rather than 0 when there's no time logged yet, so the frontend can show
    // "-" instead of a misleading "0 fish/hr".
    public double? CatchRatePerHour { get; set; }

    public double BlankRatePercent { get; set; }

    public int DistinctSpeciesCount { get; set; }
}

// One point per calendar month in the user's session history - months with
// sessions but no catches still appear, with CatchCount 0, so the chart doesn't
// show a misleading gap where nothing was actually missing.
public class MonthlyCatchesDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public int CatchCount { get; set; }

    public int CumulativeCatchCount { get; set; }
}

public class MonthlySessionsDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public int BlankSessionCount { get; set; }

    public int ProductiveSessionCount { get; set; }
}

// Generic "name -> count" row, shared by the species/venue/bait breakdown charts -
// sorted descending by Count. Grouping the long tail into "Other" is left to the
// frontend, since how many bars to show is a display concern, not a data one.
public class NamedCountDto
{
    public string Name { get; set; }

    public int Count { get; set; }
}

public class PersonalBestRowDto
{
    public string SpeciesName { get; set; }

    public decimal? WeightG { get; set; }

    public WeightUnit? WeightUnit { get; set; }

    public DateTime? WeightDate { get; set; }

    public string WeightVenueName { get; set; }

    // The heaviest and longest catch of a species needn't be the same fish, so
    // length gets its own date/venue rather than assuming they match the weight ones.
    public decimal? LengthMm { get; set; }

    public SizeUnit? LengthUnit { get; set; }

    public DateTime? LengthDate { get; set; }

    public string LengthVenueName { get; set; }
}

public class VenueStatsDto
{
    public string VenueName { get; set; }

    public int SessionCount { get; set; }

    public int CatchCount { get; set; }

    public double BlankRatePercent { get; set; }

    public double AvgCatchesPerSession { get; set; }
}

public class SpeciesStatsDto
{
    public string SpeciesName { get; set; }

    public int CatchCount { get; set; }

    public decimal? BestWeightG { get; set; }

    public WeightUnit? BestWeightUnit { get; set; }

    public decimal? AvgWeightG { get; set; }

    // Sample size behind AvgWeightG - not every catch is weighed, so the frontend
    // can show "(n=3)" rather than implying the average is as solid as the catch count.
    public int WeighedCount { get; set; }

    public decimal? BestLengthMm { get; set; }

    public SizeUnit? BestLengthUnit { get; set; }

    public decimal? AvgLengthMm { get; set; }

    // Weight and length are recorded independently per catch, so this can differ
    // from WeighedCount - sample size behind AvgLengthMm.
    public int MeasuredCount { get; set; }
}

public class MonthOfYearCatchesDto
{
    // 1-12 (January-December).
    public int Month { get; set; }

    public int CatchCount { get; set; }
}

public class MonthOfYearSessionsDto
{
    // 1-12 (January-December).
    public int Month { get; set; }

    public int ProductiveSessionCount { get; set; }

    public int BlankSessionCount { get; set; }
}

// Shared by the top-baits and top-methods tables - how many sessions used it
// alongside the raw catch count, so a bait fished twice can be told apart from
// one fished fifty times.
public class EffectivenessDto
{
    public string Name { get; set; }

    public int SessionsUsedCount { get; set; }

    public int CatchCount { get; set; }
}

public class HourOfDayCatchesDto
{
    // 0-23.
    public int Hour { get; set; }

    public int CatchCount { get; set; }
}

public class CatchPointDto
{
    public DateTime Date { get; set; }

    public decimal WeightG { get; set; }

    public WeightUnit? WeightUnit { get; set; }

    // True if this catch was a new all-time max for the species at the point it
    // was caught - lets the frontend highlight PB moments on the line.
    public bool IsPersonalBestAtTime { get; set; }

    public string VenueName { get; set; }
}

public class SpeciesProgressionDto
{
    public string SpeciesName { get; set; }

    public List<CatchPointDto> Points { get; set; }
}
