import type { AuditedEntityDto } from '@abp/ng.core';

export interface SpeciesDto extends AuditedEntityDto<number> {
  name: string;
  isSaltwater: boolean;
}

export interface CreateUpdateSpeciesDto {
  name: string;
  isSaltwater: boolean;
}

