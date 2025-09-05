import type { AuditedEntityDto } from '@abp/ng.core';
import type { SpeciesType } from '../../home/enums/species-type.enum';

export interface CatchDto extends AuditedEntityDto<number> {
  species: SpeciesType;
  weight: number;
  bait?: string;
  quantity: number;
  sessionId: number;
}

export interface CreateUpdateCatchDto {
  species: SpeciesType;
  weight: number;
  bait?: string;
  quantity: number;
  sessionId: number;
}

export interface CreateUpdateSessionDto {
  sessionDate: string;
  venue: string;
  duration: number;
  catches: CreateUpdateCatchDto[];
}

export interface SessionDto extends AuditedEntityDto<number> {
  sessionDate?: string;
  venue?: string;
  duration: number;
  catches: CatchDto[];
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