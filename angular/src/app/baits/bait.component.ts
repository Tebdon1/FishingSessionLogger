import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { speciesTypeOptions, BaitDto } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import * as _ from 'lodash'; 
import { BaitService } from '../home/services/bait.service';
import { Bait } from '@proxy/sessions'
@Component({
  selector: 'app-bait',
  templateUrl: './bait.component.html',
  styleUrls: ['./bait.component.scss'],
  providers: [
    ListService,
    { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }
  ],
})
export class BaitComponent implements OnInit {
  
  sessionItem: any;
  editSessionItem: any;

  bait = { items: [], totalCount: 0 } as PagedResultDto<BaitDto>;
  
  speciesTypes = speciesTypeOptions;

  view = ''; 

  constructor(
    public readonly list: ListService,
    private baitService: BaitService,
    private confirmation: ConfirmationService,
  ){}
  
  deleteBait(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.baitService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  ngOnInit() {
  }

  buildForm(fb: FormBuilder, selected: Bait): FormGroup {
    return fb.group({
      name: [selected.name || '', [Validators.required, Validators.maxLength(255)]]
    });
  }

}
