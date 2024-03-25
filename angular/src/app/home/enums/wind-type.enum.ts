import { mapEnumToOptions } from '@abp/ng.core';

export enum SpeciesType {
  North = 0,
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
  Rudd = 16,
  Ruffe = 17,
  SeaBass = 18,
  SmoothHound = 19,
  Tench = 20,
  TroutBrown = 21,
  TroutRainbow = 22,
  TroutSea = 23,
  Wrasse = 24,
  Zander = 25,
}

export const speciesTypeOptions = mapEnumToOptions(SpeciesType);
