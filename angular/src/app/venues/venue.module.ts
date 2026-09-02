import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { VenueRoutingModule } from './venue-routing.module';
import { VenueComponent } from './venue.component';
import { SharedModule } from '../shared/shared.module';
import { VenueFormComponent } from './venue-form/venue-form.component';

@NgModule({
  declarations: [
    VenueComponent
  ],
  imports: [
    SharedModule,
    VenueRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive venue-form component - the two module styles coexist fine.
    FormsModule,
    // The ticket filter is a searchable ng-select rather than a plain <select>,
    // same component already used for the ticket picker in venue-form - scales
    // to a long ticket list without becoming an unusable native dropdown.
    NgSelectModule,
    VenueFormComponent
  ]
})
export class VenueModule { }
