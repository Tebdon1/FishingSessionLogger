import { PermissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DynamicCrudComponent } from './dynamic-crud.component';

const routes: Routes = [
  {
    path: '',
    children: [{ path: '', component: DynamicCrudComponent }],
    canActivate: [PermissionGuard],
    data: { requiredPolicy: 'Whitefox.Lookups.Search' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DynamicCrudRoutingModule {}
