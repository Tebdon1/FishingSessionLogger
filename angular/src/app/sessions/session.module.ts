import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { SessionRoutingModule } from './session-routing.module';
import { SessionComponent } from './session.component';
import { SharedModule } from '../shared/shared.module';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';
import { VenueFormComponent } from '../venues/venue-form/venue-form.component';
import { BaitFormComponent } from '../baits/bait-form/bait-form.component';
import { MethodFormComponent } from '../methods/method-form/method-form.component';
import { RigFormComponent } from '../rigs/rig-form/rig-form.component';


@NgModule({
  declarations: [
    SessionComponent
  ],
  imports: [
    SharedModule,
    SessionRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    NgSelectModule,
    PageTopbarNavComponent,
    VenueFormComponent,
    BaitFormComponent,
    MethodFormComponent,
    RigFormComponent
  ]
})
export class SessionModule { }
