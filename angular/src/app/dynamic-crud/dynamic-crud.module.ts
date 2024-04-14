import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NgbCollapseModule, NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../shared/shared.module';
import { DynamicCrudRoutingModule } from './dynamic-crud-routing.module';
import { BeforeSaveArgs, DynamicCrudComponent } from './dynamic-crud.component';
import { DynamicSearchModule } from '../dynamic-search/dynamic-search.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [DynamicCrudComponent],
  imports: [
    CommonModule,
    SharedModule,
    DynamicCrudRoutingModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    PageModule,
    DynamicSearchModule,
    FormsModule,
    ReactiveFormsModule

  ],
  exports: [
    DynamicCrudComponent,
  ]
})
export class DynamicCrudModule {}
