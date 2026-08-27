import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MethodComponent } from './method.component';

const routes: Routes = [{ path: '', component: MethodComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MethodRoutingModule { }
