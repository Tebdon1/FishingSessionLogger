import type { AuditedEntityDto } from '@abp/ng.core';

export enum WaterType {
  River = 0,
  Lake = 1,
  Reservoir = 2,
  Canal = 3,
  Pond = 4,
  Sea = 5,
  Other = 6,
}

export interface VenueDto extends AuditedEntityDto<number> {
  name: string;
  postcode?: string;
  ticketId?: number;
  ticketName?: string;
  waterType?: WaterType;
}

export interface VenueUpdateDto {
  name: string;
  postcode?: string;
  ticketId?: number;
  waterType?: WaterType;
}
