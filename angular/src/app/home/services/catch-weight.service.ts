import type { CatchWeightDto, CreateUpdateCatchWeightDto } from '../../proxy/sessions/models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CatchWeightService {
  apiName = 'Default';
  

  create = (input: CreateUpdateCatchWeightDto) =>
    this.restService.request<any, CatchWeightDto>({
      method: 'POST',
      url: '/api/app/catch-weight',
      body: input,
    },
    { apiName: this.apiName });
  

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/catch-weight/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, CatchWeightDto>({
      method: 'GET',
      url: `/api/app/catch-weight/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<CatchWeightDto>>({
      method: 'GET',
      url: '/api/app/catch-weight',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: CreateUpdateCatchWeightDto) =>
    this.restService.request<any, CatchWeightDto>({
      method: 'PUT',
      url: `/api/app/catch-weight/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
