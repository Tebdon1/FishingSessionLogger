import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgModule } from '@angular/core';
import { NgxValidateCoreModule } from '@ngx-validate/core';

@NgModule({
  imports: [
    CoreModule,
    ThemeSharedModule,
    NgxValidateCoreModule,
  ],
  exports: [
    CoreModule,
    ThemeSharedModule,
    NgxValidateCoreModule,
  ],
  providers: [],
})
export class SharedModule {}
