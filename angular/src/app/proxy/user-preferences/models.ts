export enum PersonalBestMetric {
  Weight = 0,
  Length = 1,
}

export enum WeightUnitPreference {
  Imperial = 0,
  Metric = 1,
}

export enum LengthUnitPreference {
  Imperial = 0,
  Metric = 1,
}

export interface UserPreferenceDto {
  personalBestMetric: PersonalBestMetric;
  preferredWeightUnit: WeightUnitPreference;
  preferredLengthUnit: LengthUnitPreference;
}

export interface UpdateUserPreferenceDto {
  personalBestMetric: PersonalBestMetric;
  preferredWeightUnit: WeightUnitPreference;
  preferredLengthUnit: LengthUnitPreference;
}
