import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MethodRoutingModule } from './method-routing.module';
import { MethodComponent } from './method.component';
import { SharedModule } from '../shared/shared.module';
import { MethodFormComponent } from './method-form/method-form.component';

@NgModule({
  declarations: [
    MethodComponent
  ],
  imports: [
    SharedModule,
    MethodRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    // The filter bar uses template-driven [(ngModel)] bindings, separate from the
    // reactive method-form component - the two module styles coexist fine.
    FormsModule,
    MethodFormComponent
  ]
})
export class MethodModule { }
