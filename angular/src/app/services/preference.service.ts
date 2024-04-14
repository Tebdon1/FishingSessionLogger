import { RestService } from '@abp/ng.core';
import { HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GridSearchResults } from '../dynamic-search/models/documentResults';
import { UserView } from '../dynamic-search/models/userView';
import { CRUD_ENDPOINT } from '../shared/crud.token';
import { ErrorHandlerService } from './error-handler.service';

@Injectable({
  providedIn: 'root'
})
export class PreferenceService {
  apiName = 'Default';
  endpoint = 'preference';

  constructor(
    private restService: RestService,
    private errorHandler: ErrorHandlerService
  ) {}

  getPreferenceEditNavInfo(preferenceId: number) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/edit-nav-info/?preferenceId=${preferenceId}`,

      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  quickSearch(
    keywords: string,
    take:number = 100,
    skip:number = 0,
    endpoint = this.endpoint
  ) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}/quick-search`,
        body: {
          keywords: keywords,
          take: take,
          skip: skip  
        },
      },
      { apiName: this.apiName, skipHandleError: true }
      )
      .pipe(map(res => {
        return res['data'];
      },
      error => {
        this.errorHandler.handleError(error);
      }
    )
    );
  }

  getPreferenceDescription(preferenceId: number) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/preference-description/?preferenceId=${preferenceId}`,

      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

}
