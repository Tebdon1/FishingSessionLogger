import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})

export class UrlService {
    constructor() {} 

    apiUrl(path) {
        return environment.apis.default.url + '/api/app' + path;
    }   

}
