import type { BaitUpdateDto, BaitDto } from '../../proxy/sessions/models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BaitService {
  apiName = 'Default';
  

  create = (input: BaitUpdateDto) =>
    this.restService.request<any, BaitDto>({
      method: 'POST',
      url: '/api/app/bait',
      body: input,
    },
    { apiName: this.apiName });
  

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/bait/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, BaitDto>({
      method: 'GET',
      url: `/api/app/bait/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<BaitDto>>({
      method: 'GET',
      url: '/api/app/bait',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: BaitUpdateDto) =>
    this.restService.request<any, BaitDto>({
      method: 'PUT',
      url: `/api/app/bait/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
