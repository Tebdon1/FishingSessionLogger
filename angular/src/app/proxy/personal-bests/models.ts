import type { SizeUnit, WeightUnit } from '../sessions/models';

export interface PersonalBestDto {
  speciesId: number;
  weightG?: number;
  weightUnit?: WeightUnit;
  weightPhotoId?: number;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  lengthPhotoId?: number;
}
