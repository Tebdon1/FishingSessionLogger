import type { TicketDto, TicketUpdateDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TicketService {
  apiName = 'Default';

  create = (input: TicketUpdateDto) =>
    this.restService.request<any, TicketDto>({
      method: 'POST',
      url: '/api/app/ticket',
      body: input,
    },
    { apiName: this.apiName });

  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/ticket/${id}`,
    },
    { apiName: this.apiName });

  get = (id: number) =>
    this.restService.request<any, TicketDto>({
      method: 'GET',
      url: `/api/app/ticket/${id}`,
    },
    { apiName: this.apiName });

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<TicketDto>>({
      method: 'GET',
      url: '/api/app/ticket',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });

  update = (id: number, input: TicketUpdateDto) =>
    this.restService.request<any, TicketDto>({
      method: 'PUT',
      url: `/api/app/ticket/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
