import type { AuditedEntityDto } from '@abp/ng.core';

export interface MethodDto extends AuditedEntityDto<number> {
  name: string;
}

export interface MethodUpdateDto {
  name: string;
}
