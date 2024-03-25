import { mapEnumToOptions } from '@abp/ng.core';

export enum WindDirection {
  North = 0,
  South = 1,
  East = 2,
  West = 3,
  NorthEast = 4,
  NorthWest = 5,
  SouthEast = 6,
  SouthWest = 7,
}

export const windDirections = mapEnumToOptions(WindDirection);
