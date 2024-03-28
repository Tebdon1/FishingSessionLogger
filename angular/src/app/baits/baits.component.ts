import { ListService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { DateAdapter } from '@abp/ng.theme.shared/extensions';
import { Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import { DynamicCrudComponent } from '../dynamic-crud/dynamic-crud.component';
import { CanComponentDeactivate, CanDeactivateGuard } from '../services/canDeactivateGuard.service';
import { CrudService } from '../services/crud.service';
import { LookupService } from '../services/lookup.service';
import {
  CRUD_ENDPOINT,
} from '../shared';
import { Bait } from '../models/baits'

@Component({
  selector: 'app-bait',
  providers: [
    { provide: CRUD_ENDPOINT, useValue: 'bait' },
    CrudService,
    { provide: NgbDateAdapter, useClass: DateAdapter },
  ],
  templateUrl: './bait.component.html',
  styleUrls: ['./bait.component.scss'],
})
export class BaitsComponent implements CanComponentDeactivate  {
  @ViewChild('crud') crud: DynamicCrudComponent;

  statuses = [];

  public dateFormat = {
    displayFormat: "dd/MMM/yy",
    inputFormat: "dd/MMM/yy",
  };

  constructor(
    private confirmationService: ConfirmationService,
    private lookupService: LookupService,
  )
  {}

  ngAfterViewInit() {
    this.loadLookups();
  }

  loadLookups() {

  }

  buildForm(fb: FormBuilder, selected: Bait): FormGroup {
    return fb.group({
      name: [selected.name || '', [Validators.required, Validators.maxLength(255)]]
    });
  }

  canDeactivate(): boolean {
    return !(this.crud.displayMode == 'edit' && this.crud.form.dirty);
  }

  async canNavigate() : Promise<boolean> {
    return new Promise((resolve, reject) => {
      if (this.canDeactivate()) {
        resolve(true);
      }
      else {
        const options: Partial<Confirmation.Options> = {
          hideCancelBtn: false,
          hideYesBtn: false,
          dismissible: false,
          cancelText: 'Cancel',
          yesText: 'Yes - Lose Changes',
        };
  
        this.confirmationService
          .warn('Changes will be lost', 'Are you sure?', options)
          .subscribe(e => {
            if (e == 'confirm') {
              resolve(true);
            }
            else {
              resolve(false);
            }
          });
      }
    });
  }
}
