import { AccountConfigModule } from '@abp/ng.account/config';
import { AuthService, CoreModule } from '@abp/ng.core';
import { IdentityConfigModule } from '@abp/ng.identity/config';
import { AbpOAuthModule } from '@abp/ng.oauth';
import { SettingManagementConfigModule } from '@abp/ng.setting-management/config';
import { TenantManagementConfigModule } from '@abp/ng.tenant-management/config';
import { Component } from '@angular/core';

@Component({
  selector: 'page-topbar-nav',
  templateUrl: './page-topbar-nav.component.html',
  styleUrls: ['./page-topbar-nav.component.scss'],
  imports: [
    CoreModule,
    AbpOAuthModule,
    AccountConfigModule,
    IdentityConfigModule,
    TenantManagementConfigModule,
    SettingManagementConfigModule,
  ],
  standalone: true
})
export class PageTopbarNavComponent {

  constructor(
    private authService: AuthService) {}

    get hasLoggedIn(): boolean {
      return this.authService.isAuthenticated;
    }
  
    login() {
      this.authService.navigateToLogin();
    }
}
