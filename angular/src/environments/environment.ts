import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SessionLogger',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44374/',
    redirectUri: baseUrl,
    clientId: 'SessionLogger_App',
    responseType: 'code',
    scope: 'offline_access SessionLogger',
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44374',
      rootNamespace: 'SessionLogger',
    },
  },
} as Environment;
