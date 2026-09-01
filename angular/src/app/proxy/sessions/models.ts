import type { AuditedEntityDto } from '@abp/ng.core';

export interface CatchDto extends AuditedEntityDto<number> {
  sessionId: number;
  speciesId: number;
  speciesName: string;
  baitId?: number;
  baitName?: string;
  weightG?: number;
  weightUnit?: WeightUnit;
  methodId?: number;
  methodName?: string;
  rigId?: number;
  rigName?: string;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  catchTime?: string;
  photoId?: number;
  photoFileName?: string;
  notes?: string;
}

export interface CreateUpdateCatchDto {
  id?: number;
  sessionId: number;
  speciesId: number;
  baitId?: number;
  weightUnit: WeightUnit;
  weightLbs?: number;
  weightOz?: number;
  weightValue?: number;
  methodId?: number;
  rigId?: number;
  lengthValue?: number;
  lengthUnit: SizeUnit;
  catchTime?: string;
  photoData?: string;
  photoFileName?: string;
  removePhoto?: boolean;
  notes?: string;
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
  isBlank: boolean;
  duration: number;
  catches: CatchDto[];
}

export enum BaitType {
  Lure = 0,
  Bait = 1,
  Natural = 2,
}

export enum SizeUnit {
  Millimetres = 0,
  Centimetres = 1,
  Inches = 2,
}

export enum WeightUnit {
  LbOz = 0,
  Kilograms = 1,
  Grams = 2,
}

export interface BaitDto extends AuditedEntityDto<number> {
  name: string;
  baitType: BaitType;
  brand?: string;
  range?: string;
  colour?: string;
  flavour?: string;
  sizeMm?: number;
  sizeUnit?: SizeUnit;
}

export interface BaitUpdateDto {
  baitType: BaitType;
  name?: string;
  brand?: string;
  range?: string;
  colour?: string;
  flavour?: string;
  sizeValue?: number;
  sizeUnit: SizeUnit;
}

export class Bait {
  id: number;
  name: string;
}
