import { Component, OnInit } from '@angular/core';
import { UserPreferenceService, PersonalBestMetric, WeightUnitPreference, LengthUnitPreference } from '@proxy/user-preferences';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-preferences',
  templateUrl: './preferences.component.html',
})
export class PreferencesComponent implements OnInit {
  PersonalBestMetric = PersonalBestMetric;
  WeightUnitPreference = WeightUnitPreference;
  LengthUnitPreference = LengthUnitPreference;

  personalBestMetric = PersonalBestMetric.Weight;
  preferredWeightUnit = WeightUnitPreference.Imperial;
  preferredLengthUnit = LengthUnitPreference.Metric;
  saved = false;

  constructor(private userPreferenceService: UserPreferenceService) {}

  ngOnInit() {
    this.userPreferenceService.get().subscribe((preference) => {
      this.personalBestMetric = preference.personalBestMetric;
      this.preferredWeightUnit = preference.preferredWeightUnit;
      this.preferredLengthUnit = preference.preferredLengthUnit;
    });
  }

  async setPersonalBestMetric(metric: PersonalBestMetric) {
    if (metric === this.personalBestMetric) {
      return;
    }

    this.personalBestMetric = metric;
    await this.save();
  }

  async setPreferredWeightUnit(unit: WeightUnitPreference) {
    if (unit === this.preferredWeightUnit) {
      return;
    }

    this.preferredWeightUnit = unit;
    await this.save();
  }

  async setPreferredLengthUnit(unit: LengthUnitPreference) {
    if (unit === this.preferredLengthUnit) {
      return;
    }

    this.preferredLengthUnit = unit;
    await this.save();
  }

  private async save() {
    await lastValueFrom(
      this.userPreferenceService.update({
        personalBestMetric: this.personalBestMetric,
        preferredWeightUnit: this.preferredWeightUnit,
        preferredLengthUnit: this.preferredLengthUnit,
      })
    );
    this.saved = true;
    setTimeout(() => (this.saved = false), 2000);
  }
}
