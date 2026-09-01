import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BaitRoutingModule } from './bait-routing.module';
import { BaitComponent } from './bait.component';
import { SharedModule } from '../shared/shared.module';
import { BaitFormComponent } from './bait-form/bait-form.component';

@NgModule({
  declarations: [
    BaitComponent
  ],
  imports: [
    SharedModule,
    BaitRoutingModule,
    ReactiveFormsModule,
    BaitFormComponent
  ]
})
export class BaitModule { }
