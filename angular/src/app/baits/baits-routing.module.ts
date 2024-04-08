import { PermissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CanDeactivateGuard } from '../services/canDeactivateGuard.service';
import { BaitsComponent } from './baits.component';

const routes: Routes = [
  {
    path: '',
    component: BaitsComponent,
    canActivate: [PermissionGuard],
    canDeactivate: [CanDeactivateGuard],
    data: { requiredPolicy: 'SessionLogger.Lookups.Search' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
  providers: [CanDeactivateGuard]
})
export class BaitsRoutingModule {}
