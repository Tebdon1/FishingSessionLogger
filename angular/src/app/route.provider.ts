import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/session-logger',
        name: '::Menu:SessionLogger',
        iconClass: 'fas fa-session',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/sessions',
        name: '::Menu:Sessions',
        parentName: '::Menu:SessionLogger',
        layout: eLayoutType.application,
      },
      {
        path: '/baits',
        name: 'Baits',
        parentName: '::Menu:SessionLogger',
        layout: eLayoutType.application,
      },
    ]);
  };
}
