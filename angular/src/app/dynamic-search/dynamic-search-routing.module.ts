import { PermissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DynamicSearchComponent } from './dynamic-search.component';

const routes: Routes = [
  {
    path: '',
    children: [{ path: '', component: DynamicSearchComponent }],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DynamicSearchRoutingModule {}
