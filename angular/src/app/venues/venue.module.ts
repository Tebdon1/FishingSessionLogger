import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { VenueRoutingModule } from './venue-routing.module';
import { VenueComponent } from './venue.component';
import { SharedModule } from '../shared/shared.module';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';

@NgModule({
  declarations: [
    VenueComponent
  ],
  imports: [
    SharedModule,
    VenueRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    PageTopbarNavComponent
  ]
})
export class VenueModule { }
