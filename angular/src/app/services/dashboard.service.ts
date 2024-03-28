import { RestService } from '@abp/ng.core';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  apiName = 'Default';
  endpoint = 'dashboard';

  constructor(
    private restService: RestService,
  ) {}

  getImportsByPlant(plant: number, startDate: string, endDate: string) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/imports-by-plant?plantId=${encodeURIComponent(plant)}&startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }
  
  getImportsByMonth(plant: number, startDate: string, endDate: string) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/imports-by-month?plantId=${encodeURIComponent(plant)}&startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  getPlantSummary(startDate: string, endDate: string) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/plant-summary?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }
}
