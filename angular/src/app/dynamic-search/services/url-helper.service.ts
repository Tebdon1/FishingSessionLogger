import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UrlHelperService {

  constructor() {
  }

  public serverUrl() {
    return environment.apis.default.url;
  }

  public fileDownloadControllerUrl() {
    return this.serverUrl() + '/api/app/file';
  }

  public fileUploadControllerUrl() {
    return this.serverUrl() + '/api/app/file';
  }

  public searchControllerUrl() {
    return this.serverUrl() + '/api/app';
  }

  public lookupControllerUrl() {
    return this.serverUrl() + '/api/app/lookup';
  }

  public folderControllerUrl() {
    return this.serverUrl() + '/api/app';
  }

}
