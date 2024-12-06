import { NgModule } from '@angular/core';
import { SessionRoutingModule } from './session-routing.module';
import { SessionComponent } from './session.component';
import { SharedModule } from '../shared/shared.module';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { GridModule } from '@progress/kendo-angular-grid';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';


@NgModule({
  declarations: [
    SessionComponent
  ],
  imports: [
    SharedModule,
    SessionRoutingModule,
    NgbDatepickerModule,
    GridModule,
    PageTopbarNavComponent
  ]
})
export class SessionModule { }
