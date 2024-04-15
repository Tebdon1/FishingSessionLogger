import type { CatchSummaryDto, CreateUpdateCatchSummaryDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CatchSummaryService {
  apiName = 'Default';
  

  create = (input: CreateUpdateCatchSummaryDto) =>
    this.restService.request<any, CatchSummaryDto>({
      method: 'POST',
      url: '/api/app/catch-summary',
      body: input,
    },
    { apiName: this.apiName });
  

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/catch-summary/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, CatchSummaryDto>({
      method: 'GET',
      url: `/api/app/catch-summary/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<CatchSummaryDto>>({
      method: 'GET',
      url: '/api/app/catch-summary',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: CreateUpdateCatchSummaryDto) =>
    this.restService.request<any, CatchSummaryDto>({
      method: 'PUT',
      url: `/api/app/catch-summary/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
