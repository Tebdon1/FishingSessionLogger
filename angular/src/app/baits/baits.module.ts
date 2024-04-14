import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NgbCollapseModule, NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { PageModule } from '@abp/ng.components/page';
import { SharedModule } from '../shared/shared.module';
import { DynamicCrudModule } from '../dynamic-crud/dynamic-crud.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EditorModule, TINYMCE_SCRIPT_SRC } from '@tinymce/tinymce-angular';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { DateInputModule, DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { BaitsRoutingModule } from './baits-routing.module';
import { BaitsComponent } from './baits.component';


@NgModule({
  declarations: [BaitsComponent],
  imports: [
    CommonModule,
    SharedModule,
    BaitsRoutingModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    PageModule,
    DynamicCrudModule,
    FormsModule,
    ReactiveFormsModule,
    EditorModule,
    DropDownsModule,
    DateInputsModule,
    DateInputModule,
  ],
  providers: [
    { provide: TINYMCE_SCRIPT_SRC, useValue: 'tinymce/tinymce.min.js' }
  ]
})
export class BaitsModule {}
