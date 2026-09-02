import { Component, OnInit } from '@angular/core';
import { UserPreferenceService, PersonalBestMetric } from '@proxy/user-preferences';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-preferences',
  templateUrl: './preferences.component.html',
})
export class PreferencesComponent implements OnInit {
  PersonalBestMetric = PersonalBestMetric;

  personalBestMetric = PersonalBestMetric.Weight;
  saved = false;

  constructor(private userPreferenceService: UserPreferenceService) {}

  ngOnInit() {
    this.userPreferenceService.get().subscribe((preference) => {
      this.personalBestMetric = preference.personalBestMetric;
    });
  }

  async setPersonalBestMetric(metric: PersonalBestMetric) {
    if (metric === this.personalBestMetric) {
      return;
    }

    this.personalBestMetric = metric;
    await lastValueFrom(this.userPreferenceService.update({ personalBestMetric: metric }));
    this.saved = true;
    setTimeout(() => (this.saved = false), 2000);
  }
}
