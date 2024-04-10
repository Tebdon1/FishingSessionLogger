import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import { UrlHelperService } from '../services/url-helper.service';
import { APIResult } from '../models/apiresult';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class FoldersService {

  constructor(
    private http: HttpClient,
    private urlHelper: UrlHelperService
    ) { }

    getFolderInfo(folderId: any) : Observable<any> {


    return this.http.get<APIResult>(this.urlHelper.folderControllerUrl() + '/' + folderId + '/folder-info/' );
  }

}
