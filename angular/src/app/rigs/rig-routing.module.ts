import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RigComponent } from './rig.component';

const routes: Routes = [{ path: '', component: RigComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RigRoutingModule { }
