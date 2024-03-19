import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { SessionService, SessionDto, speciesTypeOptions } from '@proxy/sessions';
import { CatchSummaryService } from '@proxy/sessions';
import { CatchDetailService } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import * as _ from 'lodash'; 
import { lastValueFrom } from 'rxjs';

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
  expandedRow: any;
  expandedDetails = [];
  weightMax = 0;
  totalQuantity = 0;

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;
  
  sessionForm: FormGroup;
  catchSummaryForm: FormGroup;

  speciesTypes = speciesTypeOptions;

  view = ''; 

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private catchSummaryService: CatchSummaryService,
    private catchDetailService: CatchDetailService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {
      this.list.maxResultCount = 25
    }
  
    deleteSession(id: number) {
      this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.sessionService.delete(id).subscribe(() => this.list.get());
        }
      });
    }

    deleteCatchSummary(catchSummary) {
      this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.catchSummaryService.delete(catchSummary).subscribe(() => this.list.get());
        }
      });
    }

    deleteCatchDetail(i) {
      console.log(i)
      if (i > -1){
        this.catchSummaryItem.catchDetails = this.catchSummaryItem.catchDetails.splice(i, 1)
      }
      console.log(this.catchSummaryItem.catchDetails)
      //this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
        //if (status === Confirmation.Status.confirm) {
          //this.catchDetailService.delete(id).subscribe(() => this.list.get());
        //}
      //});
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
    this.sessionItem = await lastValueFrom(this.sessionService.get(id));

    console.log('sessionItem', this.sessionItem);

    this.editSessionItem = JSON.parse(JSON.stringify(this.sessionItem));

    this.editSessionItem.catchSummaries.sort((a,b) => a.species - b.species); // b - a for reverse sort

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
      await lastValueFrom(this.sessionService.update(this.sessionItem.id, formValue));
    }
    else {
      await lastValueFrom(this.sessionService.create(formValue));
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
      species: [catchSummaryItem.species, Validators.required],
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

    //if (this.editCatchSummaryItem.catchDetails.length == 0) {
      //this.editCatchSummaryItem.catchDetails.push({
        //bait: '',
        //quantity: 1,
        //catchWeights: []
      //});
    //} do we need this? 

    for (const item of this.editCatchSummaryItem.catchDetails) {
      if (item.catchWeights) {
        let weights = [];
        for (const catchWeight of item.catchWeights){
          weights.push(catchWeight.weight)
          weights.sort((a,b) => b-a);
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

  // Overview level

  async expand(row) {
    this.expandedRow = row;
    this.sessionItem = await lastValueFrom(this.sessionService.get(row.id));
    this.createExpandedDetail();
    this.totalQuantity = this.calculateSessionTotal(this.sessionItem);

    // Table sorting
    // This seems to cause an issue. If there are x fish caught on y bait and x > y it will only show y amount 
    //  of fish instead of the actual amount of fish caught
    
    
    /*
    Commented out. Not sure if this does anything? Looks like the sorting is done inside 

    this.createExpandedDetail(); on line 331-343

    this.sessionItem.catchSummaries.sort((a,b) => a.species - b.species);

    for (const entry of this.sessionItem.catchSummaries) {
      entry.catchDetails.sort((a,b) => (a.bait > b.bait) ? 1 : ((b.bait > a.bait) ? -1 : 0));
      for (const detail of entry.catchDetails) {
        detail.catchWeights.sort((a,b) => b.weight - a.weight);
        
        // b - a for reverse sort
        //if ((a,b) => b.weight - a.weight == 0) {
          //detail.catchWeights.sort((a,b) => (a.bait > b.bait) ? 1 : ((b.bait > a.bait) ? -1 : 0));
        //}
      } 
    }
    */
    this.view = 'overview';
  }
  
  counter(count) {
    return new Array(count);
  }

  createExpandedDetail() {
    this.expandedDetails = [];
    this.weightMax = 0;

    for (const catchSummary of this.sessionItem.catchSummaries) {
      catchSummary.weightMax = 0;
      if (catchSummary.catchDetails.length == 0 && catchSummary.quantity > 0){
        for (const _ of this.counter(catchSummary.quantity)) {
          this.expandedDetails.push({
            speciesName: catchSummary.species,
            weightValue: 0,
            bait: 'N/A' });
        }
      }
      for (const catchDetail of catchSummary.catchDetails) {
        if (catchDetail.catchWeights?.length > 0) {
          for (const catchWeight of catchDetail.catchWeights) {
            this.expandedDetails.push({ 
              speciesName: catchSummary.species,
              weightValue: catchWeight.weight,
              bait: catchDetail.bait });

            this.weightMax = catchWeight.weight > this.weightMax ? catchWeight.weight : this.weightMax;

            catchSummary.weightMax = catchWeight.weight > catchSummary.weightMax ? catchWeight.weight : catchSummary.weightMax;
          }
        }
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length != 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail.catchWeights?.length)) {
            this.expandedDetails.push({ speciesName: catchSummary.species,
            weightValue: 0, bait: catchDetail.bait });
          }
        }
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length == 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail. catchWeights?.length)) {
            this.expandedDetails.push({ speciesName: catchSummary.species,
            weightValue: 0, bait: catchDetail.bait })
          }
        }
      }
    }

    this.expandedDetails = this.expandedDetails.sort((a,b) =>
      (a.speciesName > b.speciesName) ? 1 : (
        (a.speciesName < b.speciesName) ? -1 : (
          (b.weightValue > a.weightValue) ? 1 : (
            (b.weightValue < a.weightValue) ? -1 : (
              (a.bait > b.bait) ? 1 : (
                (a.bait < b.bait) ? -1 : 0
              )
            )
          )
        )
      )
    )

    const grouped = this.groupBy(this.expandedDetails, expandedDetail => expandedDetail.speciesName);

    console.log(grouped);
  }

  calculateSessionTotal(session) {
    let totalQuantity = 0;
    for (const catchSummary of session.catchSummaries) {
      totalQuantity += catchSummary.quantity;
    }
    return totalQuantity
    
  }

  groupBy(list, keyGetter) {
    const map = new Map();
    list.forEach((item) => {
         const key = keyGetter(item);
         const collection = map.get(key);
         if (!collection) {
             map.set(key, [item]);
         } else {
             collection.push(item);
         }
    });
    return map;
  }
}
