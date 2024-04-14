import { RestService } from '@abp/ng.core';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ErrorHandlerService } from './error-handler.service';
import { UtilsService } from './utils.service';

@Injectable({
  providedIn: 'root'
})
export class UploadService {
  apiName = 'Default';
  endpoint = 'upload';

  constructor(
    private restService: RestService,
    private errorHandler: ErrorHandlerService,
    private utils: UtilsService,

  ) {}

  uploadFile(rawFile: any, folder: string) : Observable<any> {
    var formData = new FormData();
    formData.append('file', rawFile);
    formData.append('folder', folder);

    return this.restService.request<any, any>(
      {
        method: 'POST',
        url: `/api/app/${this.endpoint}/upload-file`,
        body: formData
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  }

  getDownloadStream(path: string) : Observable<Blob> {
    return this.restService.request<any, Blob>(
      {
        method: 'GET',
        url: `/api/app/${this.endpoint}/download-stream?path=${encodeURIComponent(path)}`,
        responseType: 'blob'
      },
      { apiName: this.apiName,
        skipHandleError: true  }
    );
  } 
}
