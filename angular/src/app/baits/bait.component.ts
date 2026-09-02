import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { BaitDto, BaitType, SizeUnit } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { BaitService } from '@proxy/baits';
import { lastValueFrom } from 'rxjs';
import { lengthDisplay } from '../shared/unit-display';

@Component({
  selector: 'app-bait',
  templateUrl: './bait.component.html',
  styleUrls: ['./bait.component.scss'],
  providers: [
    ListService,
  ],
})
export class BaitComponent implements OnInit {

  baitItem: BaitDto;

  bait = { items: [], totalCount: 0 } as PagedResultDto<BaitDto>;

  // The bait page loads everything at once (no pager), so filtering/sorting is
  // just an in-memory transform of `bait.items` rather than a server round trip.
  displayedBait: BaitDto[] = [];

  BaitType = BaitType;
  nameFilter = '';
  baitTypeFilter: BaitType | 'all' = 'all';
  sortBy: 'name' | 'size' = 'name';

  view = '';

  constructor(
    public readonly list: ListService,
    private baitService: BaitService,
    private confirmation: ConfirmationService
  ){
    // A reference pick-list, not paginated data - raised well past ABP's default
    // of 10 so every bait actually shows up (same reasoning as the other lookup
    // pages - see species.component.ts).
    this.list.maxResultCount = 1000;
  }

  ngOnInit() {
    const baitStreamCreator = (query) => this.baitService.getList(query);
    this.list.hookToQuery(baitStreamCreator).subscribe((response) => {
      this.bait = response;
      this.applyFilters();
    })
  }

  applyFilters() {
    let items = [...this.bait.items];

    const term = this.nameFilter.trim().toLowerCase();
    if (term) {
      items = items.filter((b) =>
        b.name.toLowerCase().includes(term) || (b.brand ?? '').toLowerCase().includes(term)
      );
    }

    if (this.baitTypeFilter !== 'all') {
      items = items.filter((b) => b.baitType === this.baitTypeFilter);
    }

    items.sort((a, b) => {
      if (this.sortBy === 'size') {
        const aValue = a.sizeMm ?? -Infinity;
        const bValue = b.sizeMm ?? -Infinity;
        // Equality check first - two -Infinity values (neither has a size) would
        // otherwise subtract to NaN, which Array.sort treats unpredictably rather
        // than as "equal, fall through to the alphabetical tiebreaker".
        if (aValue !== bValue) {
          return bValue - aValue;
        }
      }
      return a.name.localeCompare(b.name);
    });

    this.displayedBait = items;
  }

  clearFilters() {
    this.nameFilter = '';
    this.baitTypeFilter = 'all';
    this.sortBy = 'name';
    this.applyFilters();
  }

  baitTypeLabel(baitType: BaitType): string {
    switch (baitType) {
      case BaitType.Lure: return 'Lure';
      case BaitType.Bait: return 'Bait';
      case BaitType.Natural: return 'Natural';
      default: return '';
    }
  }

  /** Row icon standing in for the type label - lets a list of baits be told apart at a glance. */
  baitTypeIcon(baitType: BaitType): string {
    switch (baitType) {
      case BaitType.Lure: return 'fa-magnet';
      case BaitType.Bait: return 'fa-circle';
      case BaitType.Natural: return 'fa-leaf';
      default: return 'fa-flask';
    }
  }

  /** Row accent colour by type: lures keep the section's terracotta, naturals
   * are green, manufactured baits use the brass/yellow shared with Rigs. */
  baitTypeRowClass(baitType: BaitType): string {
    switch (baitType) {
      case BaitType.Lure: return 'is-accent';
      case BaitType.Bait: return 'is-brass';
      case BaitType.Natural: return '';
      default: return 'is-accent';
    }
  }

  sizeDisplay(sizeMm?: number, sizeUnit?: SizeUnit): string {
    return lengthDisplay(sizeMm, sizeUnit);
  }

  createBait() {
    this.baitItem = null;
    this.view = 'baitForm';
  }

  async editBait(id) {
    this.baitItem = await lastValueFrom(this.baitService.get(id));
    this.view = 'baitForm';
  }

  onBaitSaved() {
    this.view = '';
    this.list.get();
  }

  closeBaitForm() {
    this.view = '';
  }

  deleteBait(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.baitService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

}
