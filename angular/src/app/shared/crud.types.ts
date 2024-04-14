import { ABP, ListService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export type AdvancedFilters<Filters extends ABP.PageQueryParams> = Omit<
  Filters,
  keyof ABP.PageQueryParams
>;

export type CrudOptions = Record<string, Observable<ABP.Option<any>[]>>;

export interface CrudExtension {
  endpoint: string;
  keyResolver?: (item: ABP.BasicItem) => string;
  valueResolver?: (item: ABP.BasicItem) => any;
}

export type CrudListOptions = Pick<
  ListService,
  'filter' | 'maxResultCount' | 'page' | 'sortKey' | 'sortOrder'
>;
