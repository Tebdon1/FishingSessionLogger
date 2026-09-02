import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { MethodService, MethodDto } from '@proxy/methods';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-method',
  templateUrl: './method.component.html',
  styleUrls: ['./method.component.scss'],
  providers: [
    ListService,
  ],
})
export class MethodComponent implements OnInit {
  methodItem: MethodDto;

  method = { items: [], totalCount: 0 } as PagedResultDto<MethodDto>;

  // The method page loads everything at once (no pager), so filtering/sorting is
  // just an in-memory transform of `method.items` rather than a server round trip.
  displayedMethod: MethodDto[] = [];

  nameFilter = '';
  sortBy: 'name' | 'name-desc' = 'name';

  view = '';

  constructor(
    public readonly list: ListService,
    private methodService: MethodService,
    private confirmation: ConfirmationService) {
      // A reference pick-list, not paginated data - raised well past the old 25-row
      // cap (which had no pager, so anything past it was just invisible) so every
      // method actually shows up.
      this.list.maxResultCount = 1000;
    }

  ngOnInit() {
    const methodStreamCreator = (query) => this.methodService.getList(query);
    this.list.hookToQuery(methodStreamCreator).subscribe((response) => {
      this.method = response;
      this.applyFilters();
    });
  }

  // There's nothing on the Method DTO besides a name, so search-by-name plus an
  // A-Z/Z-A toggle is all that's meaningful here.
  applyFilters() {
    let items = [...this.method.items];

    const term = this.nameFilter.trim().toLowerCase();
    if (term) {
      items = items.filter((m) => m.name.toLowerCase().includes(term));
    }

    items.sort((a, b) => (this.sortBy === 'name-desc' ? b.name.localeCompare(a.name) : a.name.localeCompare(b.name)));

    this.displayedMethod = items;
  }

  clearFilters() {
    this.nameFilter = '';
    this.sortBy = 'name';
    this.applyFilters();
  }

  createMethod() {
    this.methodItem = null;
    this.view = 'methodForm';
  }

  async editMethod(id) {
    this.methodItem = await lastValueFrom(this.methodService.get(id));
    this.view = 'methodForm';
  }

  onMethodSaved() {
    this.view = '';
    this.list.get();
  }

  closeMethodForm() {
    this.view = '';
  }

  deleteMethod(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.methodService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
