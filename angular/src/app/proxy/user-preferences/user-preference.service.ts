import type { UpdateUserPreferenceDto, UserPreferenceDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserPreferenceService {
  apiName = 'Default';

  get = () =>
    this.restService.request<any, UserPreferenceDto>({
      method: 'GET',
      url: '/api/app/user-preference',
    },
    { apiName: this.apiName });

  update = (input: UpdateUserPreferenceDto) =>
    this.restService.request<any, UserPreferenceDto>({
      method: 'PUT',
      url: '/api/app/user-preference',
      body: input,
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
