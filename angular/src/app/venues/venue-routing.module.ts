import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VenueComponent } from './venue.component';

const routes: Routes = [{ path: '', component: VenueComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class VenueRoutingModule { }
