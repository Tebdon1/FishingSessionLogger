import type { VenueDto, VenueUpdateDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class VenueService {
  apiName = 'Default';

  create = (input: VenueUpdateDto) =>
    this.restService.request<any, VenueDto>({
      method: 'POST',
      url: '/api/app/venue',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/venue/${id}`,
    },
    { apiName: this.apiName });

  get = (id: number) =>
    this.restService.request<any, VenueDto>({
      method: 'GET',
      url: `/api/app/venue/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<VenueDto>>({
      method: 'GET',
      url: '/api/app/venue',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });

  update = (id: number, input: VenueUpdateDto) =>
    this.restService.request<any, VenueDto>({
      method: 'PUT',
      url: `/api/app/venue/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
