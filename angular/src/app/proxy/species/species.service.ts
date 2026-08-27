import type { SpeciesDto, CreateUpdateSpeciesDto } from './models';
import { RestService } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SpeciesService {
  apiName = 'Default';

  create = (input: CreateUpdateSpeciesDto) =>
    this.restService.request<any, SpeciesDto>({
      method: 'POST',
      url: '/api/app/species',
      body: input,
    },
    { apiName: this.apiName });


  delete = (id: number) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/species/${id}`,
    },
    { apiName: this.apiName });
  

  get = (id: number) =>
    this.restService.request<any, SpeciesDto>({
      method: 'GET',
      url: `/api/app/species/${id}`,
    },
    { apiName: this.apiName });
  

  getList = (input: PagedAndSortedResultRequestDto) =>
    this.restService.request<any, PagedResultDto<SpeciesDto>>({
      method: 'GET',
      url: '/api/app/species',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName });
  

  update = (id: number, input: CreateUpdateSpeciesDto) =>
    this.restService.request<any, SpeciesDto>({
      method: 'PUT',
      url: `/api/app/species/${id}`,
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}

