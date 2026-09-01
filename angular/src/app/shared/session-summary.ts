import type { SessionDto } from '@proxy/sessions';
import { weightDisplay } from './unit-display';

// Shared read-model helpers for turning a SessionDto's raw catches into the
// summary bits shown on both the sessions list and the home page's recent-
// activity preview. speciesName is optional because catches loaded from the
// API already carry speciesName - it only matters for an in-progress edit
// where a catch's speciesId was just picked and hasn't round-tripped yet.

export function sessionCatchCount(session: SessionDto): number {
  return session.catches?.length ?? 0;
}

/** Distinct species in a session with their counts, commonest first. */
export function sessionSpeciesSummary(
  session: SessionDto,
  speciesName: (id: number) => string = () => '',
): { name: string; count: number }[] {
  const counts = new Map<string, number>();
  for (const c of session.catches ?? []) {
    const name = c.speciesName || speciesName(c.speciesId) || 'Unknown';
    counts.set(name, (counts.get(name) ?? 0) + 1);
  }
  return [...counts.entries()]
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count);
}

/** Heaviest recorded fish of the session, already formatted, or '' if none weighed. */
export function sessionBestCatch(
  session: SessionDto,
  speciesName: (id: number) => string = () => '',
): string {
  const weighed = (session.catches ?? []).filter(c => c.weightG != null);
  if (weighed.length === 0) {
    return '';
  }
  const best = weighed.reduce((a, b) => (b.weightG! > a.weightG! ? b : a));
  const name = best.speciesName || speciesName(best.speciesId);
  return `${name} ${weightDisplay(best.weightG, best.weightUnit)}`.trim();
}
