import { RestService } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ExportService {
  apiName = 'Default';
  endpoint = 'product';
  
  constructor(
    private restService: RestService,
  ) {}

  export(subBrandId: number, productGroupId: number, includeEOL: boolean) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${this.endpoint}/export-products`,
        body: {
          subBrandId: subBrandId,
          productGroupId: productGroupId,
          includeEOL: includeEOL
        },
        responseType: 'blob'
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }
}
