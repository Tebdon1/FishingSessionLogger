import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgChartsModule } from 'ng2-charts';
import { StatisticsRoutingModule } from './statistics-routing.module';
import { StatisticsComponent } from './statistics.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    StatisticsComponent
  ],
  imports: [
    SharedModule,
    StatisticsRoutingModule,
    RouterModule,
    FormsModule,
    NgSelectModule,
    NgChartsModule
  ]
})
export class StatisticsModule { }
