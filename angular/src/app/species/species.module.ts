import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SpeciesRoutingModule } from './species-routing.module';
import { SpeciesComponent } from './species.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    SpeciesComponent
  ],
  imports: [
    SharedModule,
    SpeciesRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive create/edit form - the two module styles coexist fine.
    FormsModule
  ]
})
export class SpeciesModule { }
