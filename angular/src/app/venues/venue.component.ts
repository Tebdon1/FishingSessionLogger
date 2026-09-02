import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { VenueService, VenueDto, WaterType } from '@proxy/venues';
import { TicketService } from '@proxy/tickets';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-venue',
  templateUrl: './venue.component.html',
  styleUrls: ['./venue.component.scss'],
  providers: [
    ListService,
  ],
})
export class VenueComponent implements OnInit {
  venueItem: VenueDto;

  venue = { items: [], totalCount: 0 } as PagedResultDto<VenueDto>;

  // The venue page loads everything at once (no pager), so filtering/sorting is
  // just an in-memory transform of `venue.items` rather than a server round trip.
  displayedVenue: VenueDto[] = [];

  // The full ticket list, not just tickets currently assigned to a venue - a
  // ticket the user just created should be filterable straight away, before
  // it's ever attached to anything. A synthetic "No ticket" entry is mixed in
  // so the ng-select can offer it alongside real tickets as one searchable list.
  ticketFilterItems: { id: number | 'none'; name: string }[] = [];

  WaterType = WaterType;

  // Static, unlike ticketFilterItems - the water types are a fixed enum, not
  // something that grows as the user adds data.
  waterTypeOptions: { id: WaterType; name: string }[] = [
    { id: WaterType.River, name: 'River' },
    { id: WaterType.Lake, name: 'Lake' },
    { id: WaterType.Reservoir, name: 'Reservoir' },
    { id: WaterType.Canal, name: 'Canal' },
    { id: WaterType.Pond, name: 'Pond' },
    { id: WaterType.Sea, name: 'Sea' },
    { id: WaterType.Other, name: 'Other' },
  ];

  nameFilter = '';
  // Empty = no water-type filter applied; otherwise a venue matches if its water
  // type is any one of the selected values (an "or", not an "and" - a venue only
  // ever has one water type, so requiring all selected values could never match).
  waterTypeFilter: WaterType[] = [];
  // null = no ticket filter applied (ng-select's own "clear" state doubles as "All tickets").
  ticketFilter: number | 'none' | null = null;
  sortBy: 'name' | 'name-desc' = 'name';

  view = '';

  constructor(
    public readonly list: ListService,
    private venueService: VenueService,
    private ticketService: TicketService,
    private confirmation: ConfirmationService) {
      // A reference pick-list, not paginated data - raised well past the old 25-row
      // cap (which had no pager, so anything past it was just invisible) so every
      // venue actually shows up.
      this.list.maxResultCount = 1000;
    }

  ngOnInit() {
    const venueStreamCreator = (query) => this.venueService.getList(query);
    this.list.hookToQuery(venueStreamCreator).subscribe((response) => {
      this.venue = response;
      this.applyFilters();
    });

    this.loadTicketOptions();
  }

  private loadTicketOptions() {
    this.ticketService.getList({ maxResultCount: 1000 }).subscribe((response) => {
      const tickets = response.items
        .map((t) => ({ id: t.id as number | 'none', name: t.name }))
        .sort((a, b) => a.name.localeCompare(b.name));
      this.ticketFilterItems = [{ id: 'none', name: 'No ticket' }, ...tickets];
    });
  }

  // Shown in the closed water-type control instead of ng-select's default per-item
  // removable tags - plain comma-separated text matches how meta chips/labels read
  // everywhere else on this page. Selections are still added/removed by toggling
  // them in the open dropdown, not from this label.
  get waterTypeFilterLabel(): string {
    return this.waterTypeOptions
      .filter((o) => this.waterTypeFilter.includes(o.id))
      .map((o) => o.name)
      .join(', ');
  }

  applyFilters() {
    let items = [...this.venue.items];

    const term = this.nameFilter.trim().toLowerCase();
    if (term) {
      items = items.filter((v) =>
        v.name.toLowerCase().includes(term) || (v.postcode ?? '').toLowerCase().includes(term)
      );
    }

    if (this.waterTypeFilter.length > 0) {
      items = items.filter((v) => v.waterType != null && this.waterTypeFilter.includes(v.waterType));
    }

    if (this.ticketFilter === 'none') {
      items = items.filter((v) => v.ticketId == null);
    } else if (this.ticketFilter != null) {
      items = items.filter((v) => v.ticketId === this.ticketFilter);
    }

    items.sort((a, b) => (this.sortBy === 'name-desc' ? b.name.localeCompare(a.name) : a.name.localeCompare(b.name)));

    this.displayedVenue = items;
  }

  clearFilters() {
    this.nameFilter = '';
    this.waterTypeFilter = [];
    this.ticketFilter = null;
    this.sortBy = 'name';
    this.applyFilters();
  }

  waterTypeLabel(waterType?: WaterType): string {
    switch (waterType) {
      case WaterType.River: return 'River';
      case WaterType.Lake: return 'Lake';
      case WaterType.Reservoir: return 'Reservoir';
      case WaterType.Canal: return 'Canal';
      case WaterType.Pond: return 'Pond';
      case WaterType.Sea: return 'Sea';
      case WaterType.Other: return 'Other';
      default: return '';
    }
  }

  /** Row icon standing in for the water type - lets a list of venues be told apart at a glance. */
  waterTypeIcon(waterType?: WaterType): string {
    switch (waterType) {
      case WaterType.River: return 'fa-water';
      case WaterType.Lake: return 'fa-tint';
      case WaterType.Reservoir: return 'fa-database';
      case WaterType.Canal: return 'fa-road';
      case WaterType.Pond: return 'fa-dot-circle-o';
      case WaterType.Sea: return 'fa-anchor';
      default: return 'fa-map-marker';
    }
  }

  createVenue() {
    this.venueItem = null;
    this.view = 'venueForm';
  }

  async editVenue(id) {
    this.venueItem = await lastValueFrom(this.venueService.get(id));
    this.view = 'venueForm';
  }

  onVenueSaved() {
    this.view = '';
    this.list.get();
  }

  closeVenueForm() {
    this.view = '';
  }

  deleteVenue(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.venueService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
