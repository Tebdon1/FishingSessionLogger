import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private toastr: ToastrService) { }

  warning(message: string) {
    this.toastr.warning(message, 'Warning');
  }

  error(message: string) {
    this.toastr.error(message, 'Error');
  }

  info(message: string) {
    this.toastr.info(message, 'Information');
  }

  success(message: string) {
    this.toastr.success(message, 'Success');
  }

  handleError(response: any) {
    //console.log("handleError", response);
    if (response.error) {
      if (response.error.ExceptionMessage) {
        this.error(response.error.ExceptionMessage);
      }
      else if (response.error.error_message) {
        this.error(response.error.error_message);
      }
    }
    else if (response.message) {
      this.error(response.message);
    }
    else if (response.statusText) {
      this.error(response.statusText);
    }
    else {
      this.error(response);
    }
  }
}
