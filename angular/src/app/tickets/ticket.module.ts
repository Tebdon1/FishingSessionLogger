import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TicketRoutingModule } from './ticket-routing.module';
import { TicketComponent } from './ticket.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    TicketComponent
  ],
  imports: [
    SharedModule,
    TicketRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive create/edit form - the two module styles coexist fine.
    FormsModule
  ]
})
export class TicketModule { }
