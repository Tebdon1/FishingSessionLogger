import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BaitComponent } from './bait.component';

const routes: Routes = [{ path: '', component: BaitComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BaitRoutingModule { }
