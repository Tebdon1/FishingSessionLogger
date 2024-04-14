import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UrlHelperService } from './url-helper.service';
import { APIResult } from "../models/apiresult";

@Injectable({
  providedIn: 'root'
})

export class LookupService {

  constructor(
    private http: HttpClient,
    private urlHelper: UrlHelperService
    ) { }

  getLookupData(url: string) {
    return this.http.get<APIResult>(this.urlHelper.lookupControllerUrl() + url);
  }
}
