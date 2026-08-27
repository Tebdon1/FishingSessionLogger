import { SizeUnit, WeightUnit } from '../proxy/sessions/models';
import { HookWeightUnit } from '../proxy/rigs/models';

function formatNumber(n: number): string {
  return Number(n.toFixed(2)).toString();
}

// Rounded to kill floating-point noise from the division (e.g. 76.2 / 25.4 giving
// 3.0000000000000004 instead of 3) - 6dp preserves far more precision than any real
// fishing measurement needs, so nothing legitimate is lost.
export function mmToUnitValue(mm: number, unit: SizeUnit): number {
  const raw = (() => {
    switch (unit) {
      case SizeUnit.Centimetres:
        return mm / 10;
      case SizeUnit.Inches:
        return mm / 25.4;
      default:
        return mm;
    }
  })();
  return Math.round(raw * 1e6) / 1e6;
}

// Legacy rows (created before LengthUnit/SizeUnit existed) have no stored unit -
// falls back to mm to match the display behaviour that was already in place for them.
export function lengthDisplay(lengthMm?: number, unit?: SizeUnit): string {
  if (lengthMm == null) {
    return '';
  }
  const u = unit ?? SizeUnit.Millimetres;
  const suffix = u === SizeUnit.Centimetres ? 'cm' : u === SizeUnit.Inches ? 'in' : 'mm';
  return `${formatNumber(mmToUnitValue(lengthMm, u))}${suffix}`;
}

export function gramsToLbOz(weightG?: number | null): { lbs: number; oz: number } | null {
  if (weightG == null) {
    return null;
  }
  const totalOz = weightG / 28.349523125;
  let lbs = Math.floor(totalOz / 16);
  let oz = Math.round(totalOz - lbs * 16);
  if (oz === 16) {
    lbs += 1;
    oz = 0;
  }
  return { lbs, oz };
}

// Legacy rows (created before WeightUnit existed) have no stored unit - falls back to
// lb/oz to match the display behaviour that was already in place for them.
export function weightDisplay(weightG?: number, unit?: WeightUnit): string {
  if (weightG == null) {
    return '';
  }
  const u = unit ?? WeightUnit.LbOz;
  if (u === WeightUnit.Kilograms) {
    return `${formatNumber(weightG / 1000)}kg`;
  }
  if (u === WeightUnit.Grams) {
    return `${formatNumber(weightG)}g`;
  }
  const parts = gramsToLbOz(weightG);
  return parts ? `${parts.lbs}lb ${parts.oz}oz` : '';
}

// For resending a value the user isn't actively editing (e.g. an untouched catch in a
// session save) - reconstructs the value+unit pair that round-trips back to the exact
// same canonical mm, so the stored unit isn't silently overwritten with a default.
export function lengthMmToInput(
  lengthMm?: number | null,
  unit?: SizeUnit
): { lengthValue?: number; lengthUnit: SizeUnit } {
  const u = unit ?? SizeUnit.Millimetres;
  return {
    lengthValue: lengthMm == null ? undefined : mmToUnitValue(lengthMm, u),
    lengthUnit: u,
  };
}

export function weightGToInput(
  weightG?: number | null,
  unit?: WeightUnit
): { weightUnit: WeightUnit; weightLbs?: number; weightOz?: number; weightValue?: number } {
  const u = unit ?? WeightUnit.Grams;
  if (weightG == null) {
    return { weightUnit: u };
  }
  if (u === WeightUnit.Kilograms) {
    // Rounded to kill floating-point noise from the division, same as mmToUnitValue.
    return { weightUnit: u, weightValue: Math.round((weightG / 1000) * 1e6) / 1e6 };
  }
  if (u === WeightUnit.Grams) {
    return { weightUnit: u, weightValue: weightG };
  }
  const parts = gramsToLbOz(weightG);
  return { weightUnit: u, weightLbs: parts?.lbs, weightOz: parts?.oz };
}

// Mirrors SessionAppService.ToMillimetres/ToGrams on the backend, so the session-edit
// form can show a just-saved catch's weight/length locally without waiting for a round
// trip to the server (which only happens when the whole session is saved).
export function lengthInputToMm(lengthValue?: number | null, unit?: SizeUnit): number | null {
  if (lengthValue == null) {
    return null;
  }
  switch (unit) {
    case SizeUnit.Centimetres:
      return lengthValue * 10;
    case SizeUnit.Inches:
      return lengthValue * 25.4;
    default:
      return lengthValue;
  }
}

export function weightInputToGrams(input: {
  weightUnit?: WeightUnit;
  weightLbs?: number;
  weightOz?: number;
  weightValue?: number;
}): number | null {
  switch (input.weightUnit) {
    case WeightUnit.Kilograms:
      return input.weightValue == null ? null : input.weightValue * 1000;
    case WeightUnit.Grams:
      return input.weightValue ?? null;
    default: // LbOz
      if (input.weightLbs == null && input.weightOz == null) {
        return null;
      }
      return (input.weightLbs ?? 0) * 453.59237 + (input.weightOz ?? 0) * 28.349523125;
  }
}

// Jighead weight - a simpler two-unit (g/oz) scale, separate from the lb+oz/kg/g
// WeightUnit used for catch weight, since jighead weight is virtually always quoted
// in grams or ounces alone.
export function hookWeightGToUnitValue(g: number, unit: HookWeightUnit): number {
  const raw = unit === HookWeightUnit.Ounces ? g / 28.349523125 : g;
  return Math.round(raw * 1e6) / 1e6;
}

export function hookWeightDisplay(hookWeightG?: number, unit?: HookWeightUnit): string {
  if (hookWeightG == null) {
    return '';
  }
  const u = unit ?? HookWeightUnit.Grams;
  const suffix = u === HookWeightUnit.Ounces ? 'oz' : 'g';
  return `${formatNumber(hookWeightGToUnitValue(hookWeightG, u))}${suffix}`;
}

export function hookWeightGToInput(
  hookWeightG?: number | null,
  unit?: HookWeightUnit
): { hookWeightValue?: number; hookWeightUnit: HookWeightUnit } {
  const u = unit ?? HookWeightUnit.Grams;
  return {
    hookWeightValue: hookWeightG == null ? undefined : hookWeightGToUnitValue(hookWeightG, u),
    hookWeightUnit: u,
  };
}
