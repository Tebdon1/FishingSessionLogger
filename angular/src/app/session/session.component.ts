import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit, QueryList, ViewChildren, ViewChild, ChangeDetectorRef } from '@angular/core';
import { SessionService, SessionDto, speciesTypeOptions, CatchSummaryService } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import * as _ from 'lodash'; 
import { lastValueFrom } from 'rxjs';
import { MatTableDataSource, MatTable } from '@angular/material/table' 
import { MatSort } from '@angular/material/sort';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { MatTableForSessions, CatchExpandedDetails, WeightBaitInfo } from '../mat-tables/mat-table.service';

@Component({
  selector: 'app-session',
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.scss'],
  providers: [
    ListService,
    { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }
  ],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition(
        'expanded <=> collapsed',
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')
      ),
    ]),
  ],
})
export class SessionComponent implements OnInit {
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild('matTable') table: MatTable<any>
  @ViewChildren('innerSort') innerSort: QueryList<MatSort>;
  @ViewChildren('innerTables') innerTables: QueryList<MatTable<WeightBaitInfo>>;

  sessionItem: any;
  editSessionItem: any;
  catchSummaryItem: any;
  editCatchSummaryItem: any;
  expandedRow: any;
  weightMax = 0;
  totalQuantity = 0;
  /* 
  * These variables are for creating the mat table in the expanded details section.
  * Longer term being able to potentially make a generic mat table component that could take in these
  * variables and create the table would be desirable for reusability.
  */ 
  columnsToDisplay = ['Species', 'Heaviest Weight Lbs.Oz'];
  innerDisplayedColumns = ['Weight (Lbs.Oz)', 'Bait Used'];
  dataSource = new MatTableDataSource<CatchExpandedDetails>;
  expandedElement: CatchExpandedDetails | null;
  

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;
  
  sessionForm: FormGroup;
  catchSummaryForm: FormGroup;

  speciesTypes = speciesTypeOptions;

  view = ''; 

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private catchSummaryService: CatchSummaryService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private cd: ChangeDetectorRef) {
      this.list.maxResultCount = 25
    }
  
    
  ngOnInit() {
    const sessionStreamCreator = (query) => this.sessionService.getList(query);
    this.list.hookToQuery(sessionStreamCreator).subscribe((response) => {
      this.session = response;
    })

    console.log(this.dataSource);
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
    this.sessionItem = await lastValueFrom(this.sessionService.get(row.id));
    this.createExpandedDetail();

    this.expandedRow = row;
    this.totalQuantity = this.calculateSessionTotal(this.sessionItem);

    this.view = 'overview';
  }
  
  counter(count) {
    return new Array(count);
  }

  createExpandedDetail() {
    this.weightMax = 0;
/*
    //Order first. this will help with creating the array of the WeightBaitInfo. Can create a singular array,
    // then wipe it when moving onto a new 

    //Sort ascending
    this.sessionItem.catchSummaries.sort((a, b) => 
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
    );

    for (const catchSummary of this.sessionItem.catchSummaries) {

      catchSummary.weightMax = 0;
      if (catchSummary.catchDetails.length == 0 && catchSummary.quantity > 0) {
        for (const _ of this.counter(catchSummary.quantity)) {

          let newWeightBaitInfo: WeightBaitInfo = {
            weight: 0,
            bait: "N/A"
          }

          this.addOrUpdateCatchExpandedDetailsArray(
            catchSummary.species,
            newWeightBaitInfo
          );
        }
      }
      for (const catchDetail of catchSummary.catchDetails) {
        if (catchDetail.catchWeights?.length > 0) {
          for (const catchWeight of catchDetail.catchWeights) {
            console.log(catchWeight.weight, catchDetail.bait);
            //Getting this info correctly is more involved than calling { weight: catchWeight.weight, bait: catchDetail.bait } I believe
            var newWeightBaitInfo: WeightBaitInfo = {
              weight: catchWeight.weight,
              bait: catchDetail.bait
            }

            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              newWeightBaitInfo
            );

            this.weightMax = catchWeight.weight > this.weightMax ? catchWeight.weight : this.weightMax;

            catchSummary.weightMax = catchWeight.weight > catchSummary.weightMax ? catchWeight.weight : catchSummary.weightMax;
          }
        }
        //I see there are two different checks here but I don't get why?
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length != 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail.catchWeights?.length)) {
            //Getting this info correctly is more involved than calling { bait: catchDetail.bait } I believe
            var newWeightBaitInfo: WeightBaitInfo = {
              weight: 0,
              bait: catchDetail.bait
            }
  
            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              newWeightBaitInfo
            );
          }
        }
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length == 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail. catchWeights?.length)) { 
            var newWeightBaitInfo: WeightBaitInfo = {
              weight: 0,
              bait: catchDetail.bait
            }
  
            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              newWeightBaitInfo
            );
          }
        }
      }
    }

    CatchDetailsArray.forEach((detail) => {
      if (detail.weightBaitDetails && Array.isArray(detail.weightBaitDetails) && detail.weightBaitDetails)
      {
        this.expandDetailsForMatTable = [...CatchDetailsArray, {...detail, weightBaitDetails: new MatTableDataSource(detail.weightBaitDetails)}];
      }
      else {
        this.expandDetailsForMatTable = [...CatchDetailsArray, detail]
      }      
    })
*/

    console.log(this.dataSource);
  }

  toggleRow(element: CatchExpandedDetails) {
    element.weightBaitDetails &&
    (element.weightBaitDetails as MatTableDataSource<WeightBaitInfo>).data.length
      ? (this.expandedElement =
          this.expandedElement === element ? null : element)
      : null;
    this.cd.detectChanges();
    this.innerTables.forEach(
      (table, index) =>
        ((table.dataSource as MatTableDataSource<WeightBaitInfo>).sort =
          this.innerSort.toArray()[index])
    );
  }

  applyFilter(filterValue: string) {
    this.innerTables.forEach(
      (table, index) =>
        ((table.dataSource as MatTableDataSource<WeightBaitInfo>).filter = filterValue
          .trim()
          .toLowerCase())
    );
  }

  calculateSessionTotal(session: { catchSummaries: any; }) {
    let totalQuantity = 0;
    for (const catchSummary of session.catchSummaries) {
      totalQuantity += catchSummary.quantity;
    }
    return totalQuantity;
    
  }
}