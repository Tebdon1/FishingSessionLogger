import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RigRoutingModule } from './rig-routing.module';
import { RigComponent } from './rig.component';
import { SharedModule } from '../shared/shared.module';
import { RigFormComponent } from './rig-form/rig-form.component';

@NgModule({
  declarations: [
    RigComponent
  ],
  imports: [
    SharedModule,
    RigRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive rig-form component - the two module styles coexist fine.
    FormsModule,
    RigFormComponent
  ]
})
export class RigModule { }
