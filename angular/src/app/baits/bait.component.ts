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

  view = '';

  constructor(
    public readonly list: ListService,
    private baitService: BaitService,
    private confirmation: ConfirmationService
  ){}

  ngOnInit() {
    const baitStreamCreator = (query) => this.baitService.getList(query);
    this.list.hookToQuery(baitStreamCreator).subscribe((response) => {
      this.bait = response;
    })
  }

  baitTypeLabel(baitType: BaitType): string {
    switch (baitType) {
      case BaitType.Lure: return 'Lure';
      case BaitType.Bait: return 'Bait';
      case BaitType.Natural: return 'Natural';
      default: return '';
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
