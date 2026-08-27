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

  view = '';

  constructor(
    public readonly list: ListService,
    private methodService: MethodService,
    private confirmation: ConfirmationService) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const methodStreamCreator = (query) => this.methodService.getList(query);
    this.list.hookToQuery(methodStreamCreator).subscribe((response) => {
      this.method = response;
    });
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
