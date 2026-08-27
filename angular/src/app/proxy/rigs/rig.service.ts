import type { RigDto, RigUpdateDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RigService {
  apiName = 'Default';

  create = (input: RigUpdateDto) =>
    this.restService.request<any, RigDto>({
      method: 'POST',
      url: '/api/app/rig',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/rig/${id}`,
    },
    { apiName: this.apiName });

  get = (id: number) =>
    this.restService.request<any, RigDto>({
      method: 'GET',
      url: `/api/app/rig/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<RigDto>>({
      method: 'GET',
      url: '/api/app/rig',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });

  update = (id: number, input: RigUpdateDto) =>
    this.restService.request<any, RigDto>({
      method: 'PUT',
      url: `/api/app/rig/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
