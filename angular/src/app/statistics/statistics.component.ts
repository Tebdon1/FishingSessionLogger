import { Component, OnDestroy, OnInit } from '@angular/core';
import { Chart, ChartConfiguration } from 'chart.js';
import { Subject, lastValueFrom, of } from 'rxjs';
import { catchError, debounceTime, switchMap, takeUntil, tap } from 'rxjs/operators';
import { StatisticsService, StatisticsDto, NamedCountDto, SpeciesProgressionDto, GetStatisticsInput } from '@proxy/statistics';
import { WeightUnit, SizeUnit } from '@proxy/sessions';
import { BaitService } from '@proxy/baits';
import { SpeciesService } from '@proxy/species';
import { VenueService, WaterType } from '@proxy/venues';
import { TicketService } from '@proxy/tickets';
import { MethodService } from '@proxy/methods';
import { RigService } from '@proxy/rigs';
import { rigDisplayLabel } from '../shared/rig-display';
import { UserPreferenceService, PersonalBestMetric, WeightUnitPreference, LengthUnitPreference } from '@proxy/user-preferences';
import {
  weightDisplay as formatWeight,
  lengthDisplay as formatLength,
  autoWeightDisplay,
  autoLengthDisplay,
  gramsToLbOz,
} from '../shared/unit-display';

const MONTH_OF_YEAR_LABELS = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
];

type RangePreset = 'all' | 'year' | 'last12' | 'custom';

// One entry in a filter dropdown. Every filter shares this shape, the water type enum
// included (its values are numbers).
interface FilterOption {
  id: number;
  name: string;
}

// How many bars a breakdown chart shows - anything past this is dropped rather than
// shown, since a horizontal bar chart with 30 species on it is just hard to read.
const MAX_BREAKDOWN_BARS = 10;

// How much of a breakdown chart's width its axis labels may take before they're cut short
// with an ellipsis. Chart.js lets a category axis grow to the widest label but caps it at
// half the chart, and clips (rather than shortens) anything wider than that - so this sits
// a little under half, leaving room for the axis's own padding.
const MAX_AXIS_LABEL_WIDTH_FRACTION = 0.45;

// Truncated by measured pixel width, not character count: a fixed character limit can't
// tell "iiiiiiiiii" from "MMMMMMMMMM", so a label that was short enough in characters could
// still be wide enough to get clipped by the axis (e.g. "K&A Canal Midgham (RDAA)" in a
// narrow panel). The full name still shows on hover, since the tooltip reads the
// untruncated label.
//
// A trailing bracketed part - the ticket on a venue that shares its name with another,
// "K&A Canal Midgham (RDAA)" - is kept intact and the name before it is shortened
// instead: it's the only thing telling two such labels apart, so cutting the end off
// would turn both into the same "K&A Canal Midgham (R…".
function truncateLabelToFit(label: string, chart: Chart): string {
  const maxWidth = chart.width * MAX_AXIS_LABEL_WIDTH_FRACTION;
  if (!(maxWidth > 0)) {
    return label;
  }

  const ctx = chart.ctx;
  ctx.save();
  ctx.font = `${Chart.defaults.font.size}px ${Chart.defaults.font.family}`;
  try {
    const fits = (text: string) => ctx.measureText(text).width <= maxWidth;
    if (fits(label)) {
      return label;
    }

    const shorten = (text: string, suffix: string): string | null => {
      let shortened = text;
      while (shortened.length > 1 && !fits(`${shortened}…${suffix}`)) {
        shortened = shortened.slice(0, -1);
      }
      const result = `${shortened.replace(/\s+$/, '')}…${suffix}`;
      return fits(result) ? result : null;
    };

    const suffix = /\s\([^()]*\)$/.exec(label)?.[0] ?? '';
    const keepingSuffix = suffix ? shorten(label.slice(0, -suffix.length), suffix) : null;

    // A suffix too long to keep (a very long ticket name) falls back to plain end-truncation.
    return keepingSuffix ?? shorten(label, '') ?? label;
  } finally {
    ctx.restore();
  }
}

@Component({
  selector: 'app-statistics',
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.scss'],
})
export class StatisticsComponent implements OnInit, OnDestroy {
  // True while a request is in flight. The page keeps showing the previous results
  // (dimmed) during a refresh, rather than blanking to a spinner on every filter change -
  // the spinner is only for the very first load, when there's nothing to show yet.
  loading = true;
  statistics: StatisticsDto | null = null;

  rangePreset: RangePreset = 'all';
  customFrom = '';
  customTo = '';

  // --- Filters --------------------------------------------------------------------
  // Empty = no filter on that. Within one filter the values are "or"; across filters
  // "and". Venue/water type/ticket pick which sessions count (blank sessions stay in);
  // species/bait/method/rig pick which catches count, and a session only counts if it has
  // a matching one - see GetStatisticsInput on the backend for the exact rules.
  venueFilter: number[] = [];
  waterTypeFilter: WaterType[] = [];
  ticketFilter: number[] = [];
  speciesFilter: number[] = [];
  baitFilter: number[] = [];
  methodFilter: number[] = [];
  rigFilter: number[] = [];

  // Every venue/species/bait/etc the user has, not just the ones that would match
  // something under the other filters - a choice that turns up nothing shows the "no
  // sessions match" state rather than silently vanishing from the list.
  venueOptions: FilterOption[] = [];
  ticketOptions: FilterOption[] = [];
  speciesOptions: FilterOption[] = [];
  baitOptions: FilterOption[] = [];
  methodOptions: FilterOption[] = [];
  rigOptions: FilterOption[] = [];
  // Static - a fixed enum, not something that grows as the user adds data.
  waterTypeOptions: FilterOption[] = [
    { id: WaterType.River, name: 'River' },
    { id: WaterType.Lake, name: 'Lake' },
    { id: WaterType.Reservoir, name: 'Reservoir' },
    { id: WaterType.Canal, name: 'Canal' },
    { id: WaterType.Pond, name: 'Pond' },
    { id: WaterType.Sea, name: 'Sea' },
    { id: WaterType.Other, name: 'Other' },
  ];

  private reload$ = new Subject<void>();
  private destroy$ = new Subject<void>();

  activityChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  activityChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    // Without this, Chart.js sizes the canvas from its own default aspect ratio
    // rather than the fixed-height wrapper div (.chart-canvas) - on a full-width
    // panel that produces a canvas thousands of pixels tall.
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: {
      y: { beginAtZero: true, title: { display: true, text: 'Catches' }, ticks: { precision: 0 } },
    },
  };

  sessionsChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  sessionsChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { stacked: true },
      y: { stacked: true, beginAtZero: true, title: { display: true, text: 'Sessions' }, ticks: { precision: 0 } },
    },
  };

  speciesChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  venueChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  baitChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  breakdownChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: { legend: { display: false } },
    scales: {
      x: { beginAtZero: true, ticks: { precision: 0 } },
      y: {
        ticks: {
          // Long names (bait names especially) get cut short with an ellipsis
          // rather than overflowing the chart - the full name still shows on
          // hover, since the tooltip reads the untruncated label.
          callback(value) {
            return truncateLabelToFit(this.getLabelForValue(value as number), this.chart);
          },
        },
      },
    },
  };

  methodChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  waterTypeChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  seasonalityChartData: ChartConfiguration<'bar'>['data'] = { labels: MONTH_OF_YEAR_LABELS, datasets: [] };
  // Drawn with sessionsChartOptions - the same stacked productive/blank bars as
  // sessions-over-time, just grouped by month of the year instead of along a timeline.
  sessionsSeasonalityChartData: ChartConfiguration<'bar'>['data'] = { labels: MONTH_OF_YEAR_LABELS, datasets: [] };
  dayOfWeekChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  hourOfDayChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  verticalCountChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
  };

  progressionSpeciesOptions: string[] = [];
  selectedProgressionSpecies = '';
  progressionChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  // Rebuilt with the chart data (see buildProgressionChart) rather than fixed up front,
  // since the axis unit, title and tick formatting all depend on the user's preferred
  // weight unit and on how heavy the plotted catches are.
  progressionChartOptions: ChartConfiguration<'line'>['options'] = this.buildProgressionOptions(
    this.progressionAxis(0)
  );
  private progressionPoints: SpeciesProgressionDto['points'] = [];

  // Exposed so the "showing the top N of M" notes under the breakdown charts always
  // match the limit the charts themselves are cut to.
  maxBreakdownBars = MAX_BREAKDOWN_BARS;

  PersonalBestMetric = PersonalBestMetric;
  // Which metric the species/PB-focused tables show a column for - mirrors the
  // Species tile preference, so a user who only cares about weight isn't shown
  // length columns (and vice versa) in every table on this page too.
  personalBestMetric = PersonalBestMetric.Weight;

  // Only used for averages that aren't any single catch's own weight/length -
  // "Best" values below always show in the unit that specific catch was entered in.
  private preferredWeightUnit = WeightUnitPreference.Imperial;
  private preferredLengthUnit = LengthUnitPreference.Metric;

  constructor(
    private statisticsService: StatisticsService,
    private userPreferenceService: UserPreferenceService,
    private venueService: VenueService,
    private ticketService: TicketService,
    private speciesService: SpeciesService,
    private baitService: BaitService,
    private methodService: MethodService,
    private rigService: RigService
  ) {}

  ngOnInit() {
    // Every filter change just asks for a reload; this turns a burst of them (picking
    // three venues in a row) into one request, and switchMap drops an in-flight request
    // whose answer is already out of date so a slow earlier response can never overwrite
    // a newer one.
    this.reload$
      .pipe(
        debounceTime(150),
        tap(() => (this.loading = true)),
        switchMap(() => this.statisticsService.get(this.buildInput()).pipe(catchError(() => of(null)))),
        takeUntil(this.destroy$)
      )
      .subscribe((result) => {
        if (result) {
          this.statistics = result;
          this.buildCharts(result);
        }
        this.loading = false;
      });

    this.loadFilterOptions();
    this.userPreferenceService.get().subscribe((preference) => {
      this.personalBestMetric = preference.personalBestMetric;
      this.preferredWeightUnit = preference.preferredWeightUnit;
      this.preferredLengthUnit = preference.preferredLengthUnit;
      // The preference and the statistics load in parallel - if the statistics won
      // the race, the progression chart was already built in the default unit.
      if (this.statistics) {
        this.buildProgressionChart(this.statistics, themeColors());
      }
    });
    this.loadStatistics();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setRangePreset(preset: RangePreset) {
    this.rangePreset = preset;
    if (preset !== 'custom') {
      this.loadStatistics();
    }
  }

  applyCustomRange() {
    this.loadStatistics();
  }

  // Called on every filter change (see the template).
  applyFilters() {
    this.loadStatistics();
  }

  clearFilters() {
    this.venueFilter = [];
    this.waterTypeFilter = [];
    this.ticketFilter = [];
    this.speciesFilter = [];
    this.baitFilter = [];
    this.methodFilter = [];
    this.rigFilter = [];
    this.rangePreset = 'all';
    this.customFrom = '';
    this.customTo = '';
    this.loadStatistics();
  }

  // Species/bait/method/rig filter individual catches (the others filter sessions) - the
  // page says so when one's on, since it changes what "blank" and the counts mean.
  get hasCatchFilter(): boolean {
    return (
      this.speciesFilter.length > 0 ||
      this.baitFilter.length > 0 ||
      this.methodFilter.length > 0 ||
      this.rigFilter.length > 0
    );
  }

  get hasActiveFilters(): boolean {
    return (
      this.hasCatchFilter ||
      this.venueFilter.length > 0 ||
      this.waterTypeFilter.length > 0 ||
      this.ticketFilter.length > 0 ||
      this.rangePreset !== 'all'
    );
  }

  // Shown in a closed multi-select instead of ng-select's per-item removable tags -
  // plain comma-separated text, the same as the filters on the sessions and venues pages.
  selectionLabel(options: FilterOption[], selected: (number | string)[]): string {
    return options
      .filter((o) => selected.includes(o.id))
      .map((o) => o.name)
      .join(', ');
  }

  private loadStatistics() {
    this.loading = true;
    this.reload$.next();
  }

  private buildInput(): GetStatisticsInput {
    const { dateFrom, dateTo } = this.resolveDateRange();
    // An empty list is sent as "not set" rather than as an empty filter.
    const given = <T>(values: T[]): T[] | undefined => (values.length ? values : undefined);

    return {
      dateFrom,
      dateTo,
      venueIds: given(this.venueFilter),
      waterTypes: given(this.waterTypeFilter),
      ticketIds: given(this.ticketFilter),
      speciesIds: given(this.speciesFilter),
      baitIds: given(this.baitFilter),
      methodIds: given(this.methodFilter),
      rigIds: given(this.rigFilter),
    };
  }

  private async loadFilterOptions() {
    const all = { maxResultCount: 1000 };
    const byName = (a: FilterOption, b: FilterOption) => a.name.localeCompare(b.name);

    const [venues, tickets, species, baits, methods, rigs] = await Promise.all([
      lastValueFrom(this.venueService.getList(all)),
      lastValueFrom(this.ticketService.getList(all)),
      lastValueFrom(this.speciesService.getList(all)),
      lastValueFrom(this.baitService.getList(all)),
      lastValueFrom(this.methodService.getList(all)),
      lastValueFrom(this.rigService.getList(all)),
    ]);

    // Venues by their display name, so two same-named venues on different tickets are
    // told apart in the list - see VenueLabels on the backend.
    this.venueOptions = venues.items.map((v) => ({ id: v.id, name: v.displayName || v.name })).sort(byName);
    this.ticketOptions = tickets.items.map((t) => ({ id: t.id, name: t.name })).sort(byName);
    this.speciesOptions = species.items.map((s) => ({ id: s.id, name: s.name })).sort(byName);
    this.baitOptions = baits.items.map((b) => ({ id: b.id, name: b.name })).sort(byName);
    this.methodOptions = methods.items.map((m) => ({ id: m.id, name: m.name })).sort(byName);
    this.rigOptions = rigs.items.map((r) => ({ id: r.id, name: rigDisplayLabel(r) })).sort(byName);
  }

  private resolveDateRange(): { dateFrom?: string; dateTo?: string } {
    const now = new Date();

    switch (this.rangePreset) {
      case 'year':
        return { dateFrom: new Date(now.getFullYear(), 0, 1).toISOString() };
      case 'last12': {
        const from = new Date(now.getFullYear(), now.getMonth() - 11, 1);
        return { dateFrom: from.toISOString() };
      }
      case 'custom':
        return {
          dateFrom: this.customFrom ? new Date(`${this.customFrom}T00:00:00`).toISOString() : undefined,
          // The end of the chosen day, not its start: the date input only gives a day, and
          // taking it as midnight left out every session on the last day of the range.
          dateTo: this.customTo ? new Date(`${this.customTo}T23:59:59.999`).toISOString() : undefined,
        };
      default:
        return {};
    }
  }

  private buildCharts(stats: StatisticsDto) {
    const monthLabels = stats.catchesByMonth.map((m) => monthLabel(m.year, m.month));
    const colors = themeColors();

    this.activityChartData = {
      labels: monthLabels,
      datasets: [{ label: 'Catches', data: stats.catchesByMonth.map((m) => m.catchCount), backgroundColor: colors[0] }],
    };

    this.sessionsChartData = {
      labels: stats.sessionsByMonth.map((m) => monthLabel(m.year, m.month)),
      datasets: [
        {
          label: 'Productive',
          data: stats.sessionsByMonth.map((m) => m.productiveSessionCount),
          backgroundColor: colors[0],
        },
        {
          label: 'Blank',
          data: stats.sessionsByMonth.map((m) => m.blankSessionCount),
          backgroundColor: colors[4],
        },
      ],
    };

    this.speciesChartData = breakdownChartData(stats.catchesBySpecies, colors);
    this.venueChartData = breakdownChartData(stats.catchesByVenue, colors);
    this.baitChartData = breakdownChartData(stats.catchesByBait, colors);
    this.methodChartData = breakdownChartData(stats.catchesByMethod, colors);
    this.waterTypeChartData = breakdownChartData(stats.catchesByVenueWaterType, colors);

    this.seasonalityChartData = {
      labels: MONTH_OF_YEAR_LABELS,
      datasets: [{ data: stats.catchesByMonthOfYear.map((m) => m.catchCount), backgroundColor: colors[0] }],
    };

    this.sessionsSeasonalityChartData = {
      labels: MONTH_OF_YEAR_LABELS,
      datasets: [
        {
          label: 'Productive',
          data: stats.sessionsByMonthOfYear.map((m) => m.productiveSessionCount),
          backgroundColor: colors[0],
        },
        {
          label: 'Blank',
          data: stats.sessionsByMonthOfYear.map((m) => m.blankSessionCount),
          backgroundColor: colors[4],
        },
      ],
    };

    this.dayOfWeekChartData = {
      labels: stats.catchesByDayOfWeek.map((d) => d.name),
      datasets: [{ data: stats.catchesByDayOfWeek.map((d) => d.count), backgroundColor: colors[0] }],
    };

    this.hourOfDayChartData = {
      labels: stats.catchesByHourOfDay.map((h) => `${h.hour}:00`),
      datasets: [{ data: stats.catchesByHourOfDay.map((h) => h.catchCount), backgroundColor: colors[0] }],
    };

    this.progressionSpeciesOptions = stats.speciesProgressions.map((p) => p.speciesName);
    if (!this.progressionSpeciesOptions.includes(this.selectedProgressionSpecies)) {
      this.selectedProgressionSpecies = this.progressionSpeciesOptions[0] ?? '';
    }
    this.buildProgressionChart(stats, colors);
  }

  selectProgressionSpecies(speciesName: string) {
    this.selectedProgressionSpecies = speciesName;
    if (this.statistics) {
      this.buildProgressionChart(this.statistics, themeColors());
    }
  }

  private buildProgressionChart(stats: StatisticsDto, colors: string[]) {
    const series = stats.speciesProgressions.find((p) => p.speciesName === this.selectedProgressionSpecies);
    this.progressionPoints = series?.points ?? [];

    const axis = this.progressionAxis(Math.max(0, ...this.progressionPoints.map((p) => p.weightG)));
    this.progressionChartOptions = this.buildProgressionOptions(axis);

    this.progressionChartData = {
      labels: this.progressionPoints.map((p) => new Date(p.date).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })),
      datasets: [
        {
          label: this.selectedProgressionSpecies,
          data: this.progressionPoints.map((p) => p.weightG / axis.gramsPerUnit),
          borderColor: colors[0],
          backgroundColor: colors[0],
          pointBackgroundColor: this.progressionPoints.map((p) => (p.isPersonalBestAtTime ? colors[2] : colors[0])),
          pointRadius: this.progressionPoints.map((p) => (p.isPersonalBestAtTime ? 6 : 3)),
          tension: 0.15,
        },
      ],
    };
  }

  // A chart axis is a single numeric scale, so unlike a specific catch's own weight
  // (which shows in whatever unit it was entered in) the progression chart has to pick
  // one unit for every point - the user's preferred one. Imperial plots decimal pounds
  // (with ticks at whole-ounce steps, formatted back as lb/oz); metric picks grams or kg
  // by how heavy the heaviest plotted catch is, so a chart of small fish isn't a column
  // of "0.05kg"-style ticks.
  private progressionAxis(maxWeightG: number): ProgressionAxis {
    if (this.preferredWeightUnit === WeightUnitPreference.Metric) {
      const useGrams = maxWeightG < 1000;
      return {
        gramsPerUnit: useGrams ? 1 : 1000,
        title: useGrams ? 'Weight (g)' : 'Weight (kg)',
        stepSize: undefined,
        formatTick: (value) => `${formatNumber(value)}${useGrams ? 'g' : 'kg'}`,
      };
    }

    const maxLb = maxWeightG / GRAMS_PER_LB;
    return {
      gramsPerUnit: GRAMS_PER_LB,
      title: 'Weight (lb / oz)',
      stepSize: maxLb <= 2 ? 0.25 : maxLb <= 6 ? 0.5 : maxLb <= 15 ? 1 : maxLb <= 40 ? 2 : 5,
      formatTick: (value) => compactLbOz(value * GRAMS_PER_LB),
    };
  }

  private buildProgressionOptions(axis: ProgressionAxis): ChartConfiguration<'line'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true,
          title: { display: true, text: axis.title },
          ticks: {
            stepSize: axis.stepSize,
            callback: (value) => axis.formatTick(value as number),
          },
        },
      },
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (item) => {
              const point = this.progressionPoints[item.dataIndex];
              return point ? `${item.dataset.label}: ${this.avgWeightDisplay(point.weightG)}` : '';
            },
            // Surfaces the venue and PB-moment flag on hover - not obvious from
            // the plotted weight alone.
            afterLabel: (item) => {
              const point = this.progressionPoints[item.dataIndex];
              if (!point) {
                return '';
              }
              return point.isPersonalBestAtTime ? `New PB - ${point.venueName ?? ''}`.trim() : point.venueName ?? '';
            },
          },
        },
      },
    };
  }

  weightDisplay(weightG?: number, unit?: WeightUnit): string {
    return formatWeight(weightG, unit);
  }

  lengthDisplay(lengthMm?: number, unit?: SizeUnit): string {
    return formatLength(lengthMm, unit);
  }

  // An average isn't "the unit a specific catch was entered in" - there's no
  // single original unit to preserve, so this uses the user's preferred unit family
  // rather than picking one arbitrarily. Metric auto-scales by magnitude instead of
  // pinning to one fixed unit, since an average can land anywhere on the scale.
  avgWeightDisplay(weightG?: number): string {
    return this.preferredWeightUnit === WeightUnitPreference.Metric
      ? autoWeightDisplay(weightG)
      : formatWeight(weightG, WeightUnit.LbOz);
  }

  avgLengthDisplay(lengthMm?: number): string {
    return this.preferredLengthUnit === LengthUnitPreference.Metric
      ? autoLengthDisplay(lengthMm)
      : formatLength(lengthMm, SizeUnit.Inches);
  }
}

interface ProgressionAxis {
  gramsPerUnit: number;
  title: string;
  stepSize?: number;
  formatTick: (value: number) => string;
}

const GRAMS_PER_LB = 453.59237;

function formatNumber(n: number): string {
  return Number(n.toFixed(2)).toString();
}

// "1lb 8oz", "8oz" or "2lb" - drops a zero part so an axis tick at exactly 1lb doesn't
// read "1lb 0oz" and one under a pound doesn't read "0lb 8oz".
function compactLbOz(weightG: number): string {
  const parts = gramsToLbOz(weightG);
  if (!parts) {
    return '';
  }
  if (parts.lbs === 0 && parts.oz === 0) {
    return '0';
  }
  const lb = parts.lbs ? `${parts.lbs}lb` : '';
  const oz = parts.oz ? `${parts.oz}oz` : '';
  return [lb, oz].filter((p) => p).join(' ');
}

function monthLabel(year: number, month: number): string {
  return new Date(year, month - 1, 1).toLocaleDateString(undefined, { month: 'short', year: 'numeric' });
}

// Reads the active theme's own palette straight from CSS custom properties, so
// chart colours always match the rest of the page in both light and dark mode
// without duplicating the palette here.
function themeColors(): string[] {
  const styles = getComputedStyle(document.documentElement);
  const read = (name: string) => styles.getPropertyValue(name).trim();
  return [
    read('--color-primary'),
    read('--color-accent'),
    read('--color-brass'),
    read('--color-slate'),
    read('--color-danger'),
  ];
}

// Keeps only the top MAX_BREAKDOWN_BARS entries (rows arrive sorted by count,
// descending) - a horizontal bar chart with 30 species on it is unreadable.
function breakdownChartData(rows: NamedCountDto[], colors: string[]): ChartConfiguration<'bar'>['data'] {
  const top = rows.slice(0, MAX_BREAKDOWN_BARS);

  const labels = top.map((r) => r.name);
  const data = top.map((r) => r.count);

  return {
    labels,
    datasets: [{ data, backgroundColor: labels.map((_, i) => colors[i % colors.length]) }],
  };
}
