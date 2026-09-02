import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PreferencesRoutingModule } from './preferences-routing.module';
import { PreferencesComponent } from './preferences.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    PreferencesComponent
  ],
  imports: [
    SharedModule,
    PreferencesRoutingModule,
    RouterModule
  ]
})
export class PreferencesModule { }
