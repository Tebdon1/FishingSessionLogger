import { RestService } from '@abp/ng.core';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private _state = {}; 
  
  constructor(
  ) {}

  state() {
    return this._state;
  }

}
