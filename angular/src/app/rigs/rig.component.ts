import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { RigService, RigDto, HookWeightUnit } from '@proxy/rigs';
import { SizeUnit } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { lastValueFrom } from 'rxjs';
import { lengthDisplay as formatLength, hookWeightDisplay as formatHookWeight } from '../shared/unit-display';

@Component({
  selector: 'app-rig',
  templateUrl: './rig.component.html',
  styleUrls: ['./rig.component.scss'],
  providers: [
    ListService,
  ],
})
export class RigComponent implements OnInit {
  rigItem: RigDto;

  rig = { items: [], totalCount: 0 } as PagedResultDto<RigDto>;

  // The rig page loads everything at once (no pager), so filtering/sorting is
  // just an in-memory transform of `rig.items` rather than a server round trip.
  displayedRig: RigDto[] = [];

  nameFilter = '';
  sortBy: 'name' | 'length' = 'name';

  view = '';

  constructor(
    public readonly list: ListService,
    private rigService: RigService,
    private confirmation: ConfirmationService) {
      // A reference pick-list, not paginated data - raised well past the old 25-row
      // cap (which had no pager, so anything past it was just invisible) so every
      // rig actually shows up.
      this.list.maxResultCount = 1000;
    }

  ngOnInit() {
    const rigStreamCreator = (query) => this.rigService.getList(query);
    this.list.hookToQuery(rigStreamCreator).subscribe((response) => {
      this.rig = response;
      this.applyFilters();
    });
  }

  // Search also checks hookPattern and materials, since those are shown right on
  // the row and are just as likely to be what someone remembers about a rig as its name.
  applyFilters() {
    let items = [...this.rig.items];

    const term = this.nameFilter.trim().toLowerCase();
    if (term) {
      items = items.filter((r) =>
        r.name.toLowerCase().includes(term) ||
        (r.hookPattern ?? '').toLowerCase().includes(term) ||
        (r.materials ?? '').toLowerCase().includes(term)
      );
    }

    items.sort((a, b) => {
      if (this.sortBy === 'length') {
        const aValue = a.lengthMm ?? -Infinity;
        const bValue = b.lengthMm ?? -Infinity;
        // Equality check first - two -Infinity values (neither has a length) would
        // otherwise subtract to NaN, which Array.sort treats unpredictably rather
        // than as "equal, fall through to the alphabetical tiebreaker".
        if (aValue !== bValue) {
          return bValue - aValue;
        }
      }
      return a.name.localeCompare(b.name);
    });

    this.displayedRig = items;
  }

  clearFilters() {
    this.nameFilter = '';
    this.sortBy = 'name';
    this.applyFilters();
  }

  lengthDisplay(lengthMm?: number, lengthUnit?: SizeUnit): string {
    return formatLength(lengthMm, lengthUnit);
  }

  hookWeightDisplay(hookWeightG?: number, hookWeightUnit?: HookWeightUnit): string {
    return formatHookWeight(hookWeightG, hookWeightUnit);
  }

  createRig() {
    this.rigItem = null;
    this.view = 'rigForm';
  }

  async editRig(id) {
    this.rigItem = await lastValueFrom(this.rigService.get(id));
    this.view = 'rigForm';
  }

  onRigSaved() {
    this.view = '';
    this.list.get();
  }

  closeRigForm() {
    this.view = '';
  }

  deleteRig(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.rigService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
