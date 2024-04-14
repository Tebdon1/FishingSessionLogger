import { NgModule } from '@angular/core';
import { BaitRoutingModule } from './bait-routing.module';
import { BaitComponent } from './bait.component';
import { SharedModule } from '../shared/shared.module';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';


@NgModule({
  declarations: [
    BaitComponent
  ],
  imports: [
    SharedModule,
    BaitRoutingModule,
    NgbDatepickerModule,
  ]
})
export class BaitModule { }
