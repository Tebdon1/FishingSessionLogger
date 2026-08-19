import type { AuditedEntityDto } from '@abp/ng.core';

export interface VenueDto extends AuditedEntityDto<number> {
  name: string;
  postcode?: string;
  ticketId?: number;
  ticketName?: string;
}

export interface VenueUpdateDto {
  name: string;
  postcode?: string;
  ticketId?: number;
}
