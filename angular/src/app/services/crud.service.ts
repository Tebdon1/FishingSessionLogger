import { RestService } from '@abp/ng.core';
import { HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GridSearchResults } from '../dynamic-search/models/documentResults';
import { UserView } from '../dynamic-search/models/userView';
import { CRUD_ENDPOINT } from '../shared/crud.token';
import { ErrorHandlerService } from './error-handler.service';
import { SaveResult } from '../shared/saveResult';

@Injectable(
  )
export class CrudService<
  ItemType extends { id: number } = any,
  ItemDto = Omit<ItemType, 'id'>
> {
  apiName = 'Default';

  constructor(
    private restService: RestService,
    @Inject(CRUD_ENDPOINT) public readonly endpoint: string,
    private errorHandler: ErrorHandlerService
  ) {}

  getFolderInfo(endpoint = this.endpoint) : Observable<any> {
    return this.restService.request<void, any>(
      {
        method: 'GET',
        url: `/api/app/${endpoint}/folder-info`,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  search(
    folderId: string,
    userView: UserView,
    skip: number,
    take: number,
    endpoint = this.endpoint
  ) : Observable<any> {

    userView.query.searchExpression = userView.query.searchExpression ?? '';
    userView.query.keywords = userView.query.keywords ?? '';
    userView.query.filterSQL = userView.query.filterSQL ?? '';
    userView.query.filterLines = userView.query.filterLines ?? [];

    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}/search`,
        body: {
          folderId: folderId,
          userView: userView,
          skip: skip,
          take: take
        },
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  create(createAccountInput: ItemDto, endpoint = this.endpoint): Observable<SaveResult<ItemType>> {
    return this.restService.request<ItemDto, SaveResult<ItemType>>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}`,
        body: createAccountInput,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  getById(id: number, endpoint = this.endpoint): Observable<ItemType> {
    return this.restService.request<void, ItemType>(
      {
        method: 'GET',
        url: `/api/app/${endpoint}/${id}`,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  update(
    createAccountInput: ItemDto,
    id: number,
    endpoint = this.endpoint,
    params?: Record<string, any>
  ): Observable<SaveResult<ItemType>> {
    return this.restService.request<ItemDto, SaveResult<ItemType>> ( 
      {
        method: 'PUT',
        url: `/api/app/${endpoint}/${id}`,
        body: createAccountInput,
        params
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  delete(id: number, endpoint = this.endpoint): Observable<void> {
    return this.restService.request<void, void>(
      {
        method: 'DELETE',
        url: `/api/app/${endpoint}/${id}`,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  saveUserView(userViewId: number, userView: UserView, endpoint = this.endpoint) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}/save-user-view`,
        body: { 
          userViewId: userViewId, 
          json: JSON.stringify(userView)
        },
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  saveUserViewAs(userViewName: string, userView: UserView, isPublic: boolean, endpoint = this.endpoint) : Observable<any> {
    userView.name = userViewName;
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}/save-user-view-as`,
        body: { 
          json: JSON.stringify(userView),
          isPublic: isPublic
        },
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  deleteUserView(userViewId: number, endpoint = this.endpoint) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${endpoint}/delete-user-view`,
        body: {userViewId: userViewId}
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }
  
  getAllUserViews(endpoint = this.endpoint) : Observable<any> {
    return this.restService.request<void, ItemType>(
      {
        method: 'GET',
        url: `/api/app/${endpoint}/user-views`,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }

  getUserView(userViewId: number, endpoint = this.endpoint) : Observable<any> {
    return this.restService.request<any, any>(
      {
        method: 'GET',
        url: `/api/app/${endpoint}/user-view/?userViewId=${userViewId}`,
      },
      { apiName: this.apiName, skipHandleError: true }
    );
  }
}
