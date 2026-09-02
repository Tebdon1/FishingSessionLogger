import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive bait-form component - the two module styles coexist fine.
    FormsModule,
    BaitFormComponent
  ]
})
export class BaitModule { }
