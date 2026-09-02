import type { AuditedEntityDto } from '@abp/ng.core';

// Named SpeciesWaterType to avoid confusion with the unrelated WaterType used by
// venues (river/lake/reservoir/...) - see the backend enum of the same name.
export enum SpeciesWaterType {
  Freshwater = 0,
  Saltwater = 1,
  Both = 2,
}

export interface SpeciesDto extends AuditedEntityDto<number> {
  name: string;
  waterType: SpeciesWaterType;
  hasPhoto: boolean;
}

export interface CreateUpdateSpeciesDto {
  name: string;
  waterType: SpeciesWaterType;
  photoData?: string;
  photoFileName?: string;
  removePhoto?: boolean;
}

