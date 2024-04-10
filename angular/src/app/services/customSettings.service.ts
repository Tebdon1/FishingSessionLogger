import { RestService } from '@abp/ng.core';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CustomSettingsService {
  apiName = 'Default';
  endpoint = 'custom-settings';

  constructor(
    private restService: RestService,
  ) {}

  getSettings() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/settings`,

      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  saveSettings(settings: any) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${this.endpoint}/save-settings`,
        body: settings,
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  } 

}
