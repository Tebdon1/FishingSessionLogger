import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';
import { PageTopbarNavComponent } from '../components/page-topbar-nav/page-topbar-nav.component';

@NgModule({
  declarations: [HomeComponent],
  imports: [SharedModule, HomeRoutingModule, PageTopbarNavComponent],
})
export class HomeModule {}
