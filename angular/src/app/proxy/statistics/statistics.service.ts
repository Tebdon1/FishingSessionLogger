import type { GetStatisticsInput, StatisticsDto } from './models';
import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StatisticsService {
  apiName = 'Default';

  get = (input: GetStatisticsInput) =>
    this.restService.request<any, StatisticsDto>({
      method: 'GET',
      url: '/api/app/statistics',
      params: {
        dateFrom: input.dateFrom,
        dateTo: input.dateTo,
        venueIds: input.venueIds,
        waterTypes: input.waterTypes,
        ticketIds: input.ticketIds,
        speciesIds: input.speciesIds,
        baitIds: input.baitIds,
        methodIds: input.methodIds,
        rigIds: input.rigIds,
      },
    },
    { apiName: this.apiName });

  constructor(private restService: RestService) {}
}
