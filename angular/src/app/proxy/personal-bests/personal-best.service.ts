import type { PersonalBestDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PersonalBestService {
  apiName = 'Default';

  getList = () =>
    this.restService.request<any, PersonalBestDto[]>({
      method: 'GET',
      url: '/api/app/personal-best',
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
