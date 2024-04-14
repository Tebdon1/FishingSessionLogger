import { Injectable } from '@angular/core';
import { NotificationService } from "./notification.service";

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {

  constructor(
    private notificationService: NotificationService

  ) {

   }

   public handleError(error: any) {
    this.notificationService.handleError(error);
   }

}
