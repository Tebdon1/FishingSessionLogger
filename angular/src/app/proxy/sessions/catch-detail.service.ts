import type { CatchDetailDto, CreateUpdateCatchDetailDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CatchDetailService {
  apiName = 'Default';
  

  create = (input: CreateUpdateCatchDetailDto) =>
    this.restService.request<any, CatchDetailDto>({
      method: 'POST',
      url: '/api/app/catch-detail',
      body: input,
    },
    { apiName: this.apiName });
  

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/catch-detail/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, CatchDetailDto>({
      method: 'GET',
      url: `/api/app/catch-detail/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<CatchDetailDto>>({
      method: 'GET',
      url: '/api/app/catch-detail',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: CreateUpdateCatchDetailDto) =>
    this.restService.request<any, CatchDetailDto>({
      method: 'PUT',
      url: `/api/app/catch-detail/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
