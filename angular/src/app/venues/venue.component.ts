import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { VenueService, VenueDto, WaterType } from '@proxy/venues';
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

  WaterType = WaterType;

  view = '';

  constructor(
    public readonly list: ListService,
    private venueService: VenueService,
    private confirmation: ConfirmationService) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const venueStreamCreator = (query) => this.venueService.getList(query);
    this.list.hookToQuery(venueStreamCreator).subscribe((response) => {
      this.venue = response;
    });
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
