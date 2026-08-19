import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { BaitDto } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { BaitService } from '../home/services/bait.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-bait',
  templateUrl: './bait.component.html',
  styleUrls: ['./bait.component.scss'],
  providers: [
    ListService,
  ],
})
export class BaitComponent implements OnInit {
  
  baitItem: any;
  editBaitItem: any;

  bait = { items: [], totalCount: 0 } as PagedResultDto<BaitDto>;
  
  baitForm: FormGroup;

  view = ''; 

  constructor(
    public readonly list: ListService,
    private baitService: BaitService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder
  ){}
  
  ngOnInit() {
    const baitStreamCreator = (query) => this.baitService.getList(query);
    this.list.hookToQuery(baitStreamCreator).subscribe((response) => {
      this.bait = response;
    })
  }

  createBait() {
    this.baitItem = null;
    this.editBaitItem = { name: '' };
    this.buildBaitForm(this.editBaitItem);
    this.view = 'baitForm';
  }

  async editBait(id) {
    this.baitItem = await lastValueFrom(this.baitService.get(id));
    this.editBaitItem = JSON.parse(JSON.stringify(this.baitItem));
    this.buildBaitForm(this.editBaitItem);
    this.view = 'baitForm';
  }

  buildBaitForm(baitItem: any) {
    this.baitForm = this.fb.group({
      name: [baitItem.name || '', [Validators.required, Validators.maxLength(255)]]
    });
  }

  async saveBait() {
    if (this.baitForm.invalid) {
      return;
    }

    const formValue = this.baitForm.value;

    if (this.baitItem) {
      await lastValueFrom(this.baitService.update(this.baitItem.id, formValue));
    }
    else {
      await lastValueFrom(this.baitService.create(formValue));
    }

    this.view = '';
    this.baitForm.reset();
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
