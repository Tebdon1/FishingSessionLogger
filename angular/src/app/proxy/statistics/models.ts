import type { WeightUnit, SizeUnit } from '../sessions/models';
import type { WaterType } from '../venues/models';

export interface GetStatisticsInput {
  dateFrom?: string;
  dateTo?: string;
  venueIds?: number[];
  waterTypes?: WaterType[];
  ticketIds?: number[];
  speciesIds?: number[];
  baitIds?: number[];
  methodIds?: number[];
  rigIds?: number[];
}

export interface StatisticsSummaryDto {
  totalSessions: number;
  totalCatches: number;
  totalHoursFishing: number;
  catchRatePerHour?: number;
  blankRatePercent: number;
  distinctSpeciesCount: number;
}

export interface MonthlyCatchesDto {
  year: number;
  month: number;
  catchCount: number;
  cumulativeCatchCount: number;
}

export interface MonthlySessionsDto {
  year: number;
  month: number;
  blankSessionCount: number;
  productiveSessionCount: number;
}

export interface NamedCountDto {
  name: string;
  count: number;
}

export interface PersonalBestRowDto {
  speciesName: string;
  weightG?: number;
  weightUnit?: WeightUnit;
  weightDate?: string;
  weightVenueName?: string;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  lengthDate?: string;
  lengthVenueName?: string;
}

export interface VenueStatsDto {
  venueName: string;
  sessionCount: number;
  catchCount: number;
  blankRatePercent: number;
  avgCatchesPerSession: number;
}

export interface SpeciesStatsDto {
  speciesName: string;
  catchCount: number;
  bestWeightG?: number;
  bestWeightUnit?: WeightUnit;
  avgWeightG?: number;
  weighedCount: number;
  bestLengthMm?: number;
  bestLengthUnit?: SizeUnit;
  avgLengthMm?: number;
  measuredCount: number;
}

export interface MonthOfYearCatchesDto {
  month: number;
  catchCount: number;
}

export interface MonthOfYearSessionsDto {
  month: number;
  productiveSessionCount: number;
  blankSessionCount: number;
}

export interface EffectivenessDto {
  name: string;
  sessionsUsedCount: number;
  catchCount: number;
}

export interface HourOfDayCatchesDto {
  hour: number;
  catchCount: number;
}

export interface CatchPointDto {
  date: string;
  weightG: number;
  weightUnit?: WeightUnit;
  isPersonalBestAtTime: boolean;
  venueName?: string;
}

export interface SpeciesProgressionDto {
  speciesName: string;
  points: CatchPointDto[];
}

export interface StatisticsDto {
  summary: StatisticsSummaryDto;
  catchesByMonth: MonthlyCatchesDto[];
  sessionsByMonth: MonthlySessionsDto[];
  catchesBySpecies: NamedCountDto[];
  catchesByVenue: NamedCountDto[];
  catchesByBait: NamedCountDto[];
  personalBests: PersonalBestRowDto[];
  topVenues: VenueStatsDto[];
  topSpecies: SpeciesStatsDto[];
  catchesByMethod: NamedCountDto[];
  catchesByVenueWaterType: NamedCountDto[];
  catchesByMonthOfYear: MonthOfYearCatchesDto[];
  sessionsByMonthOfYear: MonthOfYearSessionsDto[];
  topBaits: EffectivenessDto[];
  topMethods: EffectivenessDto[];
  catchesByDayOfWeek: NamedCountDto[];
  catchesByHourOfDay: HourOfDayCatchesDto[];
  catchesMissingTimeCount: number;
  speciesProgressions: SpeciesProgressionDto[];
}
