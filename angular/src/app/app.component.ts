import { Component } from '@angular/core';
import { ThemeService } from './services/theme.service';

// The app deliberately does NOT use <abp-dynamic-layout>. ABP's Lepton-X layout
// ships an admin-console shell (sidebar, breadcrumbs, topbar) plus CSS bundles
// that hard-code their own background colours - all of which fought this app's
// theme. We keep everything from ABP that does real work (auth, ListService,
// the generated API proxies, ConfirmationService) and render our own chrome.
//
// <abp-confirmation> and <abp-toast-container> are deliberately NOT mounted here, even
// though ABP's layout markup includes them. ConfirmationService and ToasterService each
// append their own container to the body the first time something is shown, and wire it
// to their own stream when they do. A tag placed in a template is a second, unwired
// instance: for the toast container that means `toasts$` is undefined, so its ngOnInit
// threw "Cannot read properties of undefined (reading 'subscribe')" into the console on
// every page load - and it never displayed anything anyway.
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
  `,
})
export class AppComponent {
  constructor(private themeService: ThemeService) {}
}
