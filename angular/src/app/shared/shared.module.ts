import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgModule } from '@angular/core';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { CommercialUiModule } from '@volo/abp.commercial.ng.ui';
import { ThemeLeptonModule } from '@volo/abp.ng.theme.lepton';

@NgModule({
  imports: [
    CoreModule,
    ThemeSharedModule,
    ThemeLeptonModule,
    CommercialUiModule,
    NgbDropdownModule,
    NgxValidateCoreModule,
  ],
  exports: [
    CoreModule,
    ThemeSharedModule,
    ThemeLeptonModule,
    CommercialUiModule,
    NgbDropdownModule,
    NgxValidateCoreModule,
  ],
  providers: [],
})
export class SharedModule {}
