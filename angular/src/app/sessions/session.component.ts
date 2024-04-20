import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { CatchSummaryService, SessionService, SessionDto, speciesTypeOptions, SpeciesType } from '@proxy/sessions';
import { ElementRef } from '@angular/core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import * as _ from 'lodash'; 
import { lastValueFrom } from 'rxjs';
import { CatchDetails } from './session-models';

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
  expandedRow: ExpandedRowDetails[] = [];

  kendoGridData: CatchExpandedDetails[] = [];

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;
  
  sessionForm: FormGroup;
  catchSummaryForm: FormGroup;

  speciesTypes = speciesTypeOptions;

  view = '';

  public expandedDetailKeys: string[] = [];
  public expandDetailsBy = (dataItem: CatchExpandedDetails): string => {
    return dataItem.speciesName;
  };

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private catchSummaryService: CatchSummaryService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private elementRef: ElementRef<HTMLElement>) {
      this.list.maxResultCount = 25
    }

  ngOnInit() {
    const sessionStreamCreator = (query) => this.sessionService.getList(query);
    this.list.hookToQuery(sessionStreamCreator).subscribe((response) => {
      this.session = response;
    })
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

  validateCatchTotals() {
    var catchDetailQuantities: number = 0;
    this.editCatchSummaryItem.catchDetails.forEach(element => {
      catchDetailQuantities += element.quantity;
    });
    if(catchDetailQuantities != this.catchSummaryForm.value.quantity) {
      this.udpateBaitCatchDetailElements(true);
      return;
    }
    else if (catchDetailQuantities == this.catchSummaryForm.value.quantity) {
      this.udpateBaitCatchDetailElements(false);
      return;
    }
  }

  saveCatchSummary() {
    if (this.catchSummaryForm.invalid) {
      return;
    }

    const formValue = this.catchSummaryForm.value;
    
    formValue.catchDetails = [];

    var catchDetailQuantities: number = 0;

    if (this.editCatchSummaryItem.catchDetails.length > 0) {
      this.editCatchSummaryItem.catchDetails.forEach(element => {
        catchDetailQuantities += element.quantity;
      });

      if(catchDetailQuantities != formValue.quantity) {
        this.udpateBaitCatchDetailElements(true);
        return;
      }
    }
    
    for (const item of this.editCatchSummaryItem.catchDetails) {
      let newDetail: CatchDetails = {
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

  //I'm not thrilled about doing this like this. There is probably a clever angular method I don't know about?
  // Maybe making the inputs a form but that seems to break the dynamic adding of rows
  // Man I'm bad at angular now
  udpateBaitCatchDetailElements(error: boolean) {
    if(error) {
      for(let i = 0; i < this.editCatchSummaryItem.catchDetails.length; i++){
        document.getElementById(`baitQuantity-${i}`)
          .setAttribute('class', 'form-control is-invalid ng-dirty ng-invalid ng-touched');
      }
      document.getElementById('bait-details-error-message').setAttribute('style', '');
      return;
    }
    else if (!error){
      for(let i = 0; i < this.editCatchSummaryItem.catchDetails.length; i++){
        document.getElementById(`baitQuantity-${i}`)
          .setAttribute('class', 'form-control');
      }
      document.getElementById('bait-details-error-message').setAttribute('style', 'display: none;');
      return;
    }
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
    
    console.log(this.editCatchSummaryItem.catchDetails);
  }

  deleteCatchDetail(i: number) {
    console.log(i)
    if (i > -1){
      this.editCatchSummaryItem.catchDetails.splice(i, 1);
    }
    //this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      //if (status === Confirmation.Status.confirm) {
        //this.catchDetailService.delete(id).subscribe(() => this.list.get());
      //}
    //});
  }

  //TODO is this used? can't find a reference in the HTML
  deleteDetailRow(row) {
    this.editCatchSummaryItem.catchDetails.remove(row); // may need to amend  
  }

  // Overview level

  async expand(row) {
    this.sessionItem = await lastValueFrom(this.sessionService.get(row.id));
    this.createExpandedDetails(this.sessionItem);
    this.expandedRow = [];
    this.expandedRow.push({
      sessionDate: row.sessionDate.split("T")[0],
      venue: row.venue,
      duration: row.duration,
      maxWeight: this.maximumCatchWeight(),
      totalCaught: this.calculateSessionTotal(this.sessionItem)
    });
    console.log(this.expandedRow);
    this.view = 'overview';
  }

  calculateSessionTotal(session: { catchSummaries: any; }) {
    let totalQuantity = 0;
    for (const catchSummary of session.catchSummaries) {
      totalQuantity += catchSummary.quantity;
    }
    return totalQuantity;
    
  }

  createExpandedDetails(sessionItem: any) {
    let weightMax = 0;
    this.kendoGridData = [];
    //Order first. this will help with creating the array of the WeightBaitInfo. Can create a singular array,
    // then wipe it when moving onto a new 
  
    //Sort ascending
    sessionItem.catchSummaries.sort((a, b) => 
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
  
    for (const catchSummary of sessionItem.catchSummaries) {
  
      catchSummary.weightMax = 0;
      if (catchSummary.catchDetails.length == 0 && catchSummary.quantity > 0) {
        for (const _ of this.counter(catchSummary.quantity)) {
  
          let newWeightBaitInfo = {
            weight: "0",
            bait: "N/A"
          }
  
          this.addOrUpdateCatchExpandedDetailsArray(
            catchSummary.species,
            catchSummary.speciesName,
            newWeightBaitInfo
          );
        }
      }
      for (const catchDetail of catchSummary.catchDetails) {
        if (catchDetail.catchWeights?.length > 0) {
          for (const catchWeight of catchDetail.catchWeights) {
            console.log(catchDetail.bait);
            let newWeightBaitInfo = {
              weight: catchWeight.weight,
              bait: (catchDetail.bait && catchDetail.bait != "") ? catchDetail.bait : "N/A"
            }
  
            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              catchSummary.speciesName,
              newWeightBaitInfo
            );
  
            weightMax = catchWeight.weight > weightMax ? catchWeight.weight : weightMax;
  
            catchSummary.weightMax = catchWeight.weight > catchSummary.weightMax ? catchWeight.weight : catchSummary.weightMax;
          }
        }
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length != 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail.catchWeights?.length)) {
            let newWeightBaitInfo = {
              weight: "0",
              bait: "N/A"
            }
  
            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              catchSummary.speciesName,
              newWeightBaitInfo
            );
          }
        }
        if (catchDetail.catchWeights?.length < catchDetail.quantity && catchDetail.catchWeights.length == 0) {
          for (const noWeight of this.counter(catchDetail.quantity - catchDetail. catchWeights?.length)) {
            console.log(catchDetail.bait);
            let newWeightBaitInfo = {
              weight: "0",
              bait: catchDetail.bait ?? "N/A"
            }
  
            this.addOrUpdateCatchExpandedDetailsArray(
              catchSummary.species,
              catchSummary.speciesName,
              newWeightBaitInfo
            );
          }
        }
      }
    }
  }
  
  private addOrUpdateCatchExpandedDetailsArray(
    species: number,
    speciesName: string,
    catchInfoToMatch: { weight: string, bait: string },
   )
  {
    //If a catch entry already exists we simply add an element of { weight: number, bait: string } to the ArrayForTryingToGetThingsIntoMatTable.weightBaitDetails
    if (this.kendoGridData && this.kendoGridData.findIndex((catchEntry) => catchEntry.species === species) !== -1)
    {
      var catchDetails = this.kendoGridData.find((catchEntry) => catchEntry.species === species);
  
      if (catchDetails.weightBaitDetails && Array.isArray(catchDetails.weightBaitDetails))
      {
        catchDetails.weightBaitDetails.push(catchInfoToMatch);
        catchDetails.quantity++;
      }
      else 
      {
        catchDetails.weightBaitDetails.push(catchInfoToMatch);
        catchDetails.quantity++;
      }

      if (catchDetails.maxWeight < catchInfoToMatch.weight)
        catchDetails.maxWeight = parseFloat(catchInfoToMatch.weight).toFixed(2);
   }
   else 
   {
    speciesName = SpeciesType[species];

    this.kendoGridData.push({
        species: species,
        speciesName: SpeciesType[species],
        maxWeight: catchInfoToMatch.weight != "0" ? parseFloat(catchInfoToMatch.weight).toFixed(2) : "0",
        quantity: 1,
        weightBaitDetails: [catchInfoToMatch]
      });
   }
  }

  private maximumCatchWeight()
  {
    let maxWeight: string = "0.00";
    this.kendoGridData.forEach(element => {
      if (element.maxWeight > maxWeight)
        {
          maxWeight = element.maxWeight;
        }
    });
    return maxWeight
  }

  private counter(count) {
    return new Array(count);
  }  
}

/*
* Formatting will be as follows. This is for formatting data to use in the kendo table
*  var TableArray: CatchExpandedDetails[] = [
    {
      species: enum value here
      speciesName: 'Fishy name',
      weightBaitDetails: [
        {
          weight: 458,
          bait: 'An extra baity bait',
        },
      ],
    },
    {
      species: enum value here
      speciesName: 'Fishy name 2',
      weightBaitDetails: [
        {
          weight: 95000,
          bait: 'A baity bait',
        },
        {
          weight: 95000,
          bait: '',
        },
      ],
    },
*/

export interface CatchExpandedDetails
{
  species: number,
  speciesName: string,
  maxWeight: string;
  quantity: number;
  weightBaitDetails: WeightBaitDetails[];
}

export interface WeightBaitDetails
{
  weight: string;
  bait: string;
}

export interface ExpandedRowDetails
{
  sessionDate: string,
  venue: string,
  duration: number,
  totalCaught: number,
  maxWeight: string
}