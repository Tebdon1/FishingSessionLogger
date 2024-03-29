import { Injectable } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Observable, of } from 'rxjs';

@Injectable()
export class MatTableForSessions {

  sortMatTableData(sessionItem: any): [Observable<CatchExpandedDetails[]>, number] {
    let weightMax = 0;
    let expandDetailsForMatTable: CatchExpandedDetails[] = [];
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

            weightMax = catchWeight.weight > weightMax ? catchWeight.weight : weightMax;

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
        expandDetailsForMatTable = [...CatchDetailsArray, {...detail, weightBaitDetails: new MatTableDataSource(detail.weightBaitDetails)}];
      }
      else {
        expandDetailsForMatTable = [...CatchDetailsArray, detail]
      }      
    })

    return [of(expandDetailsForMatTable), weightMax];
  }

  private addOrUpdateCatchExpandedDetailsArray(
    species: number,
    catchInfoToMatch: WeightBaitInfo,
   )
  {
    //If a catch entry already exists we simply add an element of { weight: number, bait: string } to the ArrayForTryingToGetThingsIntoMatTable.weightBaitDetails
    if (CatchDetailsArray && CatchDetailsArray.findIndex((catchEntry) => catchEntry.species === species) !== -1)
    {
      var catchDetails = CatchDetailsArray.find((catchEntry) => catchEntry.species === species);

      if (catchDetails.weightBaitDetails && Array.isArray(catchDetails.weightBaitDetails))
      {
        catchDetails.weightBaitDetails.push(catchInfoToMatch);
      }
      else 
      {
        catchDetails.weightBaitDetails = [catchInfoToMatch];
      }
   }
   else 
   {
      CatchDetailsArray.push({
        species: species,
        maxWeight: 0,
        quantity: 0,
        weightBaitDetails: [catchInfoToMatch]
      });
   }
  }

  private counter(count) {
    return new Array(count);
  }

}

// I'm not super happy about making these here... 
// Needed some more solidly typed interfaces for the mat table however which maybe can be abstracted somehow eventually

/*
* Formatting will be as follows 
*  var ArrayForTryingToGetThingsIntoMatTable: CatchExpandedDetailsForMatTable[] = [
    {
      speciesName: 'Fishy name',
      weightBaitDetails: [
        {
          weight: 458,
          bait: 'An extra baity bait',
        },
      ],
    },
    {
      speciesName: 'Fishy name 2',weightBaitDetails: [
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

    I don't know why really I was required to make this variable to chuck everything in but it stopped 
    line 397 this.expandDetailsForMatTable = [...ArrayForTryingToGetThingsIntoMatTable, {...detail, weightBaitDetails: new MatTableDataSource(expandedDetail.weightBaitDetails)}];
    complaining about type issues between weightBaitDetails being a WeightBaitInfo[] | MatTableDataSource<WeightBaitInfo>; 
    and new MatTableDataSource(expandedDetail.weightBaitDetails being a WeightBaitInfo[]) 
*/
var CatchDetailsArray: CatchExpandedDetails[] = []

export interface CatchExpandedDetails
{
  species: number;
  maxWeight: number;
  quantity: number;
  weightBaitDetails?: WeightBaitInfo[] | MatTableDataSource<WeightBaitInfo>;
}

export interface WeightBaitInfo
{
  weight: number;
  bait: string;
}