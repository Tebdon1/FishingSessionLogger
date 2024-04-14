import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SessionComponent } from './session.component';
import { SessionLoggerModule } from '../material-module'; 

const routes: Routes = [{ path: '', component: SessionComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), SessionLoggerModule],
  exports: [RouterModule]
})
export class SessionRoutingModule { }
