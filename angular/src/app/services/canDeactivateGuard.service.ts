import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Observable, Subject } from 'rxjs';

export interface CanComponentDeactivate {
  canDeactivate: () => boolean;
}

@Injectable()
export class CanDeactivateGuard implements CanDeactivate<CanComponentDeactivate> {
  navigateAwaySelection$: Subject<boolean> = new Subject<boolean>();
  
  constructor (
    private confirmationService: ConfirmationService
  ) { 
  }

  canDeactivate(component: CanComponentDeactivate, 
  route: ActivatedRouteSnapshot, 
  state: RouterStateSnapshot) : boolean | Observable<boolean> | Promise<boolean>{
    var promptRequired = !(component?.canDeactivate ? component.canDeactivate() : true);

    if (promptRequired) {
      const options: Partial<Confirmation.Options> = {
        hideCancelBtn: false,
        hideYesBtn: false,
        dismissible: false,
        cancelText: 'Cancel',
        yesText: 'Yes - Lose Changes',
      };

      this.confirmationService
        .warn('Changes will be lost', 'Are you sure?', options)
        .subscribe((result) => {
          this.navigateAwaySelection$.next(result == 'confirm');
        });

      return this.navigateAwaySelection$;  
    }
    else {
      return true;
    }   
 }

}