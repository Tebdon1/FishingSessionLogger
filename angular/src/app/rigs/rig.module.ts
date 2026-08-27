import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { RigRoutingModule } from './rig-routing.module';
import { RigComponent } from './rig.component';
import { SharedModule } from '../shared/shared.module';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';
import { RigFormComponent } from './rig-form/rig-form.component';

@NgModule({
  declarations: [
    RigComponent
  ],
  imports: [
    SharedModule,
    RigRoutingModule,
    RouterModule,
    ReactiveFormsModule,
    PageTopbarNavComponent,
    RigFormComponent
  ]
})
export class RigModule { }
