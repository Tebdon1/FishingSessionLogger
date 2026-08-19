import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BaitRoutingModule } from './bait-routing.module';
import { BaitComponent } from './bait.component';
import { SharedModule } from '../shared/shared.module';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';

@NgModule({
  declarations: [
    BaitComponent
  ],
  imports: [
    SharedModule,
    BaitRoutingModule,
    ReactiveFormsModule,
    PageTopbarNavComponent
  ]
})
export class BaitModule { }
