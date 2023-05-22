import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { SessionService, SessionDto, speciesTypeOptions, CreateUpdateSessionDto } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import * as _ from 'lodash'; 

@Component({
  selector: 'app-session',
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.scss'],
  providers: [
    ListService,
    { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }
  ],
})
export class SessionComponent implements OnInit {
  
  sessionItem: any;
  editSessionItem: any;
  catchSummaryItem: any;
  editCatchSummaryItem: any;

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;
  
  sessionForm: FormGroup;
  catchSummaryForm: FormGroup;

  speciesTypes = speciesTypeOptions;

  view = ''; 

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {}
  
    deleteSession(id: number) {
      this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.sessionService.delete(id).subscribe(() => this.list.get());
        }
      });
    }

  ngOnInit() {
    const sessionStreamCreator = (query) => this.sessionService.getList(query);
    this.list.hookToQuery(sessionStreamCreator).subscribe((response) => {
      this.session = response;
    })
  }

  // session level

  createSession() {
    this.sessionItem = null;

    this.editSessionItem = {
      sessionDate: new Date(),
      catchSummaries: []
    }
    
    this.buildSessionForm(this.editSessionItem);
    this.view = 'sessionForm';
  }

  async editSession(id) {
    this.sessionItem = await this.sessionService.get(id).toPromise();

    console.log('sessionItem', this.sessionItem);

    this.editSessionItem = JSON.parse(JSON.stringify(this.sessionItem));

    this.buildSessionForm(this.editSessionItem);
    this.view = 'sessionForm';
  }

  buildSessionForm(sessionItem: any) {
    this.sessionForm = this.fb.group({
      sessionDate: [sessionItem.sessionDate == null ? new Date() : new Date(sessionItem.sessionDate), Validators.required],
      venue: [sessionItem.venue || '', Validators.required],
      duration: [sessionItem.duration, Validators.required],
    });
  }

  async saveSession() {
    if (this.sessionForm.invalid) {
      return;
    }

    const formValue = this.sessionForm.value;
    
    formValue.catchSummaries = this.editSessionItem.catchSummaries;

    if (this.sessionItem) {
      await this.sessionService.update(this.sessionItem.id, formValue).toPromise();
    }
    else {
      await this.sessionService.create(formValue).toPromise();
    }

    this.view = '';
    this.sessionForm.reset();
    this.list.get();
}

  closeSessionForm() {
    this.view = '';
  }

  addCatchSummary() {
    this.catchSummaryItem = null;

    this.editCatchSummaryItem = {
      catchDetails: []
    };
    this.buildCatchSummaryForm(this.editCatchSummaryItem);
    this.view = 'catchSummaryForm';
  }

  // catchSummary level

  buildCatchSummaryForm(catchSummaryItem: any) {
    this.catchSummaryForm = this.fb.group({
      species: [catchSummaryItem.species || '', Validators.required],
      quantity: [catchSummaryItem.quantity || 1, Validators.required],
    });
  }

  closeCatchSummaryForm() {
    this.view = 'sessionForm';
  }

  saveCatchSummary() {
    if (this.catchSummaryForm.invalid) {
      return;
    }

    const formValue = this.catchSummaryForm.value;
    
    formValue.catchDetails = [];

    for (const item of this.editCatchSummaryItem.catchDetails) {
      let newDetail = {
        bait: item.bait,
        quantity: item.quantity,
        catchWeights: []
      }

      let weightString = item.weightString.trim(); 
      if (weightString != '') {
        for (const stringWeight of weightString.split(',')) {
          newDetail.catchWeights.push(
            {
              weight:parseFloat(stringWeight)
            });
        }
      } 

      formValue.catchDetails.push(newDetail);
    }

    if (this.catchSummaryItem) {
      this.catchSummaryItem = _.extend(this.catchSummaryItem, formValue);
    }
    else {
      console.log('pushing catchSummary', formValue);
      this.editSessionItem.catchSummaries.push(formValue);
    }

    this.view = 'sessionForm';

  }

  editCatchSummary(catchSummary) {
    console.log('editCatchSummary', catchSummary);

    if (catchSummary.catchDetails == null) {
      catchSummary.catchDetails = [];
    }

    this.catchSummaryItem = catchSummary;

    this.editCatchSummaryItem = JSON.parse(JSON.stringify(catchSummary));

    if (this.editCatchSummaryItem.catchDetails.length == 0) {
      this.editCatchSummaryItem.catchDetails.push({
        bait: '',
        quantity: 1,
        catchWeights: []
      });
    }

    for (const item of this.editCatchSummaryItem.catchDetails) {
      if (item.catchWeights) {
        let weights = [];
        for (const catchWeight of item.catchWeights){
          weights.push(catchWeight.weight)
        };
        item.weightString = weights.join(',');
      }
    }

    this.buildCatchSummaryForm(this.editCatchSummaryItem);

    this.view = 'catchSummaryForm';    
  }

  // catchDetails level

  addCatchDetail() {
    this.editCatchSummaryItem.catchDetails.push({
        bait: '',
        quantity: 1,
        weightString: '',
    });
  }

  deleteDetailRow(row) {
    this.editCatchSummaryItem.catchDetails.remove(row); // may need to amend  
  }

}
