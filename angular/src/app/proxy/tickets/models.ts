import type { AuditedEntityDto } from '@abp/ng.core';

export interface TicketDto extends AuditedEntityDto<number> {
  name: string;
}

export interface TicketUpdateDto {
  name: string;
}
