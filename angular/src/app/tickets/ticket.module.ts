import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { TicketRoutingModule } from './ticket-routing.module';
import { TicketComponent } from './ticket.component';
import { SharedModule } from '../shared/shared.module';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';

@NgModule({
  declarations: [
    TicketComponent
  ],
  imports: [
    SharedModule,
    TicketRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    PageTopbarNavComponent
  ]
})
export class TicketModule { }
