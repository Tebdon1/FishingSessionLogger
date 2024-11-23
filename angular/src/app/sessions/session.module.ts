import { NgModule } from '@angular/core';
import { SessionRoutingModule } from './session-routing.module';
import { SessionComponent } from './session.component';
import { SharedModule } from '../shared/shared.module';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { GridModule } from '@progress/kendo-angular-grid';


@NgModule({
  declarations: [
    SessionComponent
  ],
  imports: [
    SharedModule,
    SessionRoutingModule,
    NgbDatepickerModule,
    GridModule
  ]
})
export class SessionModule { }
