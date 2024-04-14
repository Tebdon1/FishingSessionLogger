import type { CreateUpdateSessionDto, SessionDto } from '../../proxy/sessions/models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SessionService {
  apiName = 'Default';
  

  create = (input: CreateUpdateSessionDto) =>
    this.restService.request<any, SessionDto>({
      method: 'POST',
      url: '/api/app/session',
      body: input,
    },
    { apiName: this.apiName });
  

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/session/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, SessionDto>({
      method: 'GET',
      url: `/api/app/session/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<SessionDto>>({
      method: 'GET',
      url: '/api/app/session',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: CreateUpdateSessionDto) =>
    this.restService.request<any, SessionDto>({
      method: 'PUT',
      url: `/api/app/session/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
