import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
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
    MethodFormComponent
  ]
})
export class MethodModule { }
