import type { MethodDto, MethodUpdateDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MethodService {
  apiName = 'Default';

  create = (input: MethodUpdateDto) =>
    this.restService.request<any, MethodDto>({
      method: 'POST',
      url: '/api/app/method',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/method/${id}`,
    },
    { apiName: this.apiName });

  get = (id: number) =>
    this.restService.request<any, MethodDto>({
      method: 'GET',
      url: `/api/app/method/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<MethodDto>>({
      method: 'GET',
      url: '/api/app/method',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });

  update = (id: number, input: MethodUpdateDto) =>
    this.restService.request<any, MethodDto>({
      method: 'PUT',
      url: `/api/app/method/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
