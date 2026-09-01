import { Component } from '@angular/core';
import { ThemeService } from './services/theme.service';

// The app deliberately does NOT use <abp-dynamic-layout>. ABP's Lepton-X layout
// ships an admin-console shell (sidebar, breadcrumbs, topbar) plus CSS bundles
// that hard-code their own background colours - all of which fought this app's
// theme. We keep everything from ABP that does real work (auth, ListService,
// the generated API proxies, ConfirmationService) and render our own chrome.
//
// <abp-confirmation> and <abp-toast-container> are normally mounted by that
// layout, so they have to be mounted here instead or delete-confirmations and
// toasts silently stop appearing.
@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar></abp-loader-bar>

    <app-page-topbar-nav></app-page-topbar-nav>

    <main class="app-main">
      <router-outlet></router-outlet>
    </main>

    <footer class="app-footer">
      <span>Session Logger</span>
      <span class="app-footer-dot">&bull;</span>
      <span>Tight lines</span>
    </footer>

    <abp-confirmation></abp-confirmation>
    <abp-toast-container></abp-toast-container>
  `,
})
export class AppComponent {
  constructor(private themeService: ThemeService) {}
}
