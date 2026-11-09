import { SpeciesDto, SpeciesWaterType } from '@proxy/species';
import { WaterType } from '@proxy/venues';

// Which species can sensibly be targeted at a venue. A sea venue offers saltwater
// species, a river/lake/reservoir/canal/pond offers freshwater ones, and species that
// turn up in both are offered either way. "Other" (or no venue picked yet) can't say,
// so it offers everything rather than hiding a species the angler might really want.
export function speciesForVenueWaterType(species: SpeciesDto[], venueWaterType?: WaterType | null): SpeciesDto[] {
  if (venueWaterType == null || venueWaterType === WaterType.Other) {
    return species;
  }

  const wanted = venueWaterType === WaterType.Sea ? SpeciesWaterType.Saltwater : SpeciesWaterType.Freshwater;
  return species.filter((s) => s.waterType === wanted || s.waterType === SpeciesWaterType.Both);
}
