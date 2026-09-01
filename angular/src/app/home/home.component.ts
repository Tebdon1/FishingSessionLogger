import { AuthService } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { SessionService, SessionDto } from '@proxy/sessions';
import { sessionCatchCount, sessionSpeciesSummary, sessionBestCatch } from '../shared/session-summary';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  recentSessions: SessionDto[] = [];
  totalSessions = 0;
  loadingRecent = false;

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  constructor(
    private authService: AuthService,
    private sessionService: SessionService) {}

  async ngOnInit() {
    if (!this.hasLoggedIn) {
      return;
    }

    this.loadingRecent = true;
    try {
      const result = await lastValueFrom(this.sessionService.getList({
        sorting: 'startDateTime desc',
        skipCount: 0,
        maxResultCount: 3,
      }));
      this.recentSessions = result.items;
      this.totalSessions = result.totalCount;
    }
    finally {
      this.loadingRecent = false;
    }
  }

  login() {
    this.authService.navigateToLogin();
  }

  // Same read-model as the sessions list - see ../shared/session-summary.
  catchCount(session: SessionDto): number {
    return sessionCatchCount(session);
  }

  speciesSummary(session: SessionDto): { name: string; count: number }[] {
    return sessionSpeciesSummary(session);
  }

  bestCatch(session: SessionDto): string {
    return sessionBestCatch(session);
  }
}
