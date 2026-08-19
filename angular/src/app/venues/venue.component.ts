import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { VenueService, VenueDto } from '@proxy/venues';
import { TicketService, TicketDto } from '@proxy/tickets';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
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
  editVenueItem: any;

  venue = { items: [], totalCount: 0 } as PagedResultDto<VenueDto>;

  venueForm: FormGroup;

  ticketList: TicketDto[] = [];

  view = '';

  constructor(
    public readonly list: ListService,
    private venueService: VenueService,
    private ticketService: TicketService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const venueStreamCreator = (query) => this.venueService.getList(query);
    this.list.hookToQuery(venueStreamCreator).subscribe((response) => {
      this.venue = response;
    });
    this.loadTickets();
  }

  async loadTickets() {
    const tickets = await lastValueFrom(this.ticketService.getList({ maxResultCount: 1000 }));
    this.ticketList = tickets.items;
  }

  createVenue() {
    this.venueItem = null;
    this.editVenueItem = { name: '', postcode: '', ticketId: null };
    this.buildVenueForm(this.editVenueItem);
    this.view = 'venueForm';
  }

  async editVenue(id) {
    this.venueItem = await lastValueFrom(this.venueService.get(id));
    this.editVenueItem = JSON.parse(JSON.stringify(this.venueItem));
    this.buildVenueForm(this.editVenueItem);
    this.view = 'venueForm';
  }

  buildVenueForm(venueItem: any) {
    this.venueForm = this.fb.group({
      name: [venueItem.name || '', [Validators.required, Validators.maxLength(255)]],
      postcode: [venueItem.postcode || '', Validators.maxLength(8)],
      ticketId: [venueItem.ticketId ?? null],
    });
  }

  async saveVenue() {
    if (this.venueForm.invalid) {
      return;
    }

    const formValue = this.venueForm.value;

    if (this.venueItem) {
      await lastValueFrom(this.venueService.update(this.venueItem.id, formValue));
    }
    else {
      await lastValueFrom(this.venueService.create(formValue));
    }

    this.view = '';
    this.venueForm.reset();
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
