import { Injectable } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'sessionlogger-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private current: ThemeMode;

  constructor() {
    this.current = this.readStoredTheme();
    this.apply(this.current);
  }

  get mode(): ThemeMode {
    return this.current;
  }

  toggle(): void {
    this.setTheme(this.current === 'light' ? 'dark' : 'light');
  }

  setTheme(mode: ThemeMode): void {
    this.current = mode;
    localStorage.setItem(STORAGE_KEY, mode);
    this.apply(mode);
  }

  private apply(mode: ThemeMode): void {
    document.documentElement.setAttribute('data-theme', mode);
  }

  private readStoredTheme(): ThemeMode {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
