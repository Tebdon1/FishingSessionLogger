import type { AuditedEntityDto } from '@abp/ng.core';
import type { SpeciesType } from '../../home/enums/species-type.enum';

export interface CatchDetailDto extends AuditedEntityDto<number> {
  bait?: string;
  quantity: number;
  catchWeights: CatchWeightDto[];
}

export interface CatchSummaryDto extends AuditedEntityDto<number> {
  species: SpeciesType;
  quantity: number;
  catchDetails: CatchDetailDto[];
}

export interface CatchWeightDto extends AuditedEntityDto<number> {
  weight: number;
}

export interface CreateUpdateCatchDetailDto {
  bait?: string;
  quantity: number;
  catchWeights: CreateUpdateCatchWeightDto[];
}

export interface CreateUpdateCatchSummaryDto {
  quantity: number;
  species: SpeciesType;
  catchDetails: CreateUpdateCatchDetailDto[];
}

export interface CreateUpdateCatchWeightDto {
  weight: number;
}

export interface CreateUpdateSessionDto {
  sessionDate: string;
  venue: string;
  duration: number;
  catchSummaries: CreateUpdateCatchSummaryDto[];
}

export interface SessionDto extends AuditedEntityDto<number> {
  sessionDate?: string;
  venue?: string;
  duration: number;
  catchSummaries: CatchSummaryDto[];
}

export interface BaitDto extends AuditedEntityDto<number> {
  name: string;
}

export interface BaitUpdateDto {
  name: string;
}

export class Bait {
  id: number;
  name: string;
}