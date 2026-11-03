import type { SizeUnit } from '@proxy/sessions';
import type { HookWeightUnit } from '@proxy/rigs';
import { lengthDisplay, hookWeightDisplay } from './unit-display';

// Rig names aren't unique - fold the specs into the label so "Ronnie Rig" tied two
// different ways reads as two distinct options instead of an ambiguous duplicate.
export function rigDisplayLabel(rig: {
  name: string;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  hookSize?: string;
  hookWeightG?: number;
  hookWeightUnit?: HookWeightUnit;
  hookPattern?: string;
  materials?: string;
}): string {
  const parts = [
    rig.lengthMm != null ? lengthDisplay(rig.lengthMm, rig.lengthUnit) : null,
    rig.hookSize ? `Size ${rig.hookSize}` : null,
    rig.hookWeightG != null ? hookWeightDisplay(rig.hookWeightG, rig.hookWeightUnit) : null,
    rig.hookPattern || null,
    rig.materials || null,
  ].filter(p => p);

  return parts.length ? `${rig.name} (${parts.join(', ')})` : rig.name;
}
