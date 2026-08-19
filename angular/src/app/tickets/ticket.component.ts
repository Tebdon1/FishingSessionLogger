import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { TicketService, TicketDto } from '@proxy/tickets';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-ticket',
  templateUrl: './ticket.component.html',
  styleUrls: ['./ticket.component.scss'],
  providers: [
    ListService,
  ],
})
export class TicketComponent implements OnInit {
  ticketItem: TicketDto;
  editTicketItem: any;

  ticket = { items: [], totalCount: 0 } as PagedResultDto<TicketDto>;

  ticketForm: FormGroup;

  view = '';

  constructor(
    public readonly list: ListService,
    private ticketService: TicketService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const ticketStreamCreator = (query) => this.ticketService.getList(query);
    this.list.hookToQuery(ticketStreamCreator).subscribe((response) => {
      this.ticket = response;
    });
  }

  createTicket() {
    this.ticketItem = null;
    this.editTicketItem = { name: '' };
    this.buildTicketForm(this.editTicketItem);
    this.view = 'ticketForm';
  }

  async editTicket(id) {
    this.ticketItem = await lastValueFrom(this.ticketService.get(id));
    this.editTicketItem = JSON.parse(JSON.stringify(this.ticketItem));
    this.buildTicketForm(this.editTicketItem);
    this.view = 'ticketForm';
  }

  buildTicketForm(ticketItem: any) {
    this.ticketForm = this.fb.group({
      name: [ticketItem.name || '', [Validators.required, Validators.maxLength(255)]],
    });
  }

  async saveTicket() {
    if (this.ticketForm.invalid) {
      return;
    }

    const formValue = this.ticketForm.value;

    if (this.ticketItem) {
      await lastValueFrom(this.ticketService.update(this.ticketItem.id, formValue));
    }
    else {
      await lastValueFrom(this.ticketService.create(formValue));
    }

    this.view = '';
    this.ticketForm.reset();
    this.list.get();
  }

  closeTicketForm() {
    this.view = '';
  }

  deleteTicket(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.ticketService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
