import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { lastValueFrom } from 'rxjs';
import { VenueService, VenueDto, VenueUpdateDto, WaterType } from '@proxy/venues';
import { TicketService, TicketDto } from '@proxy/tickets';

// Single source of truth for the venue create/edit fields - used by both the Venues
// page's own form and the session form's quick-add-venue modal, so a field can't
// drift out of sync between the two.
@Component({
  selector: 'app-venue-form',
  templateUrl: './venue-form.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, NgSelectModule],
})
export class VenueFormComponent implements OnInit, OnChanges {
  @Input() venue: VenueDto | null = null;
  @Output() saved = new EventEmitter<VenueDto>();
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;
  ticketList: TicketDto[] = [];
  WaterType = WaterType;

  constructor(
    private venueService: VenueService,
    private ticketService: TicketService,
    private fb: FormBuilder) {
    // Covers usage with no [venue] binding at all (e.g. the always-create quick-add
    // modal), where ngOnChanges below never fires since there's no bound input.
    this.buildForm();
  }

  ngOnInit() {
    this.loadTickets();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.venue) {
      this.buildForm();
    }
  }

  private async loadTickets() {
    const tickets = await lastValueFrom(this.ticketService.getList({ maxResultCount: 1000 }));
    this.ticketList = tickets.items;
  }

  private buildForm() {
    const v = this.venue;
    this.form = this.fb.group({
      name: [v?.name || '', [Validators.required, Validators.maxLength(255)]],
      postcode: [v?.postcode || '', Validators.maxLength(8)],
      ticketId: [v?.ticketId ?? null],
      waterType: [v?.waterType ?? null],
    });
  }

  async save() {
    if (this.form.invalid) {
      return;
    }

    const formValue: VenueUpdateDto = this.form.value;
    const result = this.venue
      ? await lastValueFrom(this.venueService.update(this.venue.id, formValue))
      : await lastValueFrom(this.venueService.create(formValue));

    this.saved.emit(result);
  }

  cancel() {
    this.cancelled.emit();
  }
}
