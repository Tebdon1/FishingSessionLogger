import type { AuditedEntityDto } from '@abp/ng.core';

export interface CatchDto extends AuditedEntityDto<number> {
  sessionId: number;
  speciesId: number;
  speciesName: string;
  baitId?: number;
  baitName?: string;
  weight?: number;
  photoId?: number;
  photoFileName?: string;
}

export interface CreateUpdateCatchDto {
  sessionId: number;
  speciesId: number;
  baitId?: number;
  weight?: number;
  photoData?: string;
  photoFileName?: string;
}

export interface CreateUpdateSessionDto {
  startDateTime: string;
  endDateTime: string;
  venueId: number;
  notes?: string;
  catches: CreateUpdateCatchDto[];
}

export interface SessionDto extends AuditedEntityDto<number> {
  startDateTime: string;
  endDateTime: string;
  venueId: number;
  venueName?: string;
  notes?: string;
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
