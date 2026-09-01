import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
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
    ReactiveFormsModule
  ]
})
export class SpeciesModule { }
