import { mapEnumToOptions } from '@abp/ng.core';

export enum SpeciesType {
  Barbel = 0,
  Bleak = 1,
  BreamCommon = 2,
  BreamSilver = 3,
  CarpCommon = 4,
  CarpCrucian = 5,
  CarpMirror = 6,
  Chub = 7,
  Dace = 8,
  Eel = 9,
  Grayling = 10,
  Gudgeon = 11,
  Minnow = 12,
  Perch = 13,
  Pike = 14,
  Roach = 15,
  RoachBreamHybrid = 16,
  Rudd = 17,
  Ruffe = 18,
  SeaBass = 19,
  SmoothHound = 20,
  Tench = 21,
  TroutBrown = 22,
  TroutRainbow = 23,
  TroutSea = 24,
  Wrasse = 25,
  Zander = 26,
}

export const speciesTypeOptions = mapEnumToOptions(SpeciesType);
