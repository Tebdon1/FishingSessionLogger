import { InjectionToken } from '@angular/core';
import { CrudExtension, CrudListOptions } from './crud.types';

export const CRUD_ENDPOINT = new InjectionToken<string>('CRUD_ENDPOINT');
export const CRUD_EXTENSIONS = new InjectionToken<CrudExtension[]>('CRUD_EXTENSIONS');
export const CRUD_LIST_OPTIONS = new InjectionToken<CrudListOptions>('CRUD_LIST_OPTIONS');
