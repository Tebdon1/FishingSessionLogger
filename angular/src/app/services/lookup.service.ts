import { RestService } from '@abp/ng.core';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LookupService {
  apiName = 'Default';
  endpoint = 'lookup';

  constructor(
    private restService: RestService,
  ) {}

  getPlants() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/plants`,

      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  getFieldMasters() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/field-masters`,

      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  getFieldMasterStatuses() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/field-master-statuses`,
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  } 

  getCtgItemTypes() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-types`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getGetCtgItemTypesHierarchical() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-types-hierarchical`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getCtgItemStatuses() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-statuses`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getCtgItemConditions() : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-conditions`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getCtgItemTypeInformation(id : number) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-type-information?typeId=${id.toString()}`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getCtgLocationsForItemType(id : number) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-locations-for-item-type?typeId=${id.toString()}`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  getCtgItemTypeInformationByItemId(id : number) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/ctg-item-type-information-by-item-id?Id=${id.toString()}`
      },
      {
        apiName: this.apiName,
        skipHandleError: true
      }
    )
  }

  ctgLocationSearch(typeId: number, keywords: string, skip: number = 0, take: number = 100) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${this.endpoint}/ctg-location-search`,
        body: {
          typeId: typeId,
          keywords: keywords,
          skip: skip,
          take: take
        },
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  ctgLocationSearchById(id : number): Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${this.endpoint}/ctg-location-search-by-id`,
        body: {
          id: id,
        },
    },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  

}
