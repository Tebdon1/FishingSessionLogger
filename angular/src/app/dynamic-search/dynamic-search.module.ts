import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NgbCollapseModule, NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../shared/shared.module';
import { DynamicSearchRoutingModule } from './dynamic-search-routing.module';
import { DynamicSearchComponent } from './dynamic-search.component';
import { ViewComponent } from './view//view.component';
import { QueryComponent } from './query/query.component';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { ExcelModule, GridModule } from '@progress/kendo-angular-grid';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { IconsModule } from '@progress/kendo-angular-icons';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { LabelModule } from '@progress/kendo-angular-label';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { ToolBarModule } from '@progress/kendo-angular-toolbar';
import { DialogModule } from '@progress//kendo-angular-dialog';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';

@NgModule({
  declarations: [
    DynamicSearchComponent,
    ViewComponent,
    QueryComponent],
  imports: [
    CommonModule,
    SharedModule,
    DynamicSearchRoutingModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    PageModule,
    FormsModule,
    ReactiveFormsModule,
    InputsModule,
    LabelModule,
    LayoutModule,
    ButtonsModule,
    GridModule,
    IconsModule,
    DropDownsModule,
    ToolBarModule,
    DialogModule,
    DragDropModule,
    DateInputsModule,
    ExcelModule,
  ],
  exports: [
    DynamicSearchComponent
  ]
})
export class DynamicSearchModule {}
