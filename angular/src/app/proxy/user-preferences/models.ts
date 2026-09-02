export enum PersonalBestMetric {
  Weight = 0,
  Length = 1,
}

export interface UserPreferenceDto {
  personalBestMetric: PersonalBestMetric;
}

export interface UpdateUserPreferenceDto {
  personalBestMetric: PersonalBestMetric;
}
