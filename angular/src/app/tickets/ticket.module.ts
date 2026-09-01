import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
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
    ReactiveFormsModule
  ]
})
export class TicketModule { }
