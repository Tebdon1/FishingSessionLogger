import type { AuditedEntityDto } from '@abp/ng.core';
import type { SizeUnit } from '../sessions/models';

export enum HookWeightUnit {
  Grams = 0,
  Ounces = 1,
}

export interface RigDto extends AuditedEntityDto<number> {
  name: string;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  hookSize?: string;
  hookWeightG?: number;
  hookWeightUnit?: HookWeightUnit;
  hookPattern?: string;
  materials?: string;
  notes?: string;
}

export interface RigUpdateDto {
  name: string;
  lengthValue?: number;
  lengthUnit: SizeUnit;
  hookSize?: string;
  hookWeightValue?: number;
  hookWeightUnit: HookWeightUnit;
  hookPattern?: string;
  materials?: string;
  notes?: string;
}
