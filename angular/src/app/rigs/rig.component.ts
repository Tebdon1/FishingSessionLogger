import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { RigService, RigDto, HookWeightUnit } from '@proxy/rigs';
import { SizeUnit } from '@proxy/sessions';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { lastValueFrom } from 'rxjs';
import { lengthDisplay as formatLength, hookWeightDisplay as formatHookWeight } from '../shared/unit-display';

@Component({
  selector: 'app-rig',
  templateUrl: './rig.component.html',
  styleUrls: ['./rig.component.scss'],
  providers: [
    ListService,
  ],
})
export class RigComponent implements OnInit {
  rigItem: RigDto;

  rig = { items: [], totalCount: 0 } as PagedResultDto<RigDto>;

  view = '';

  constructor(
    public readonly list: ListService,
    private rigService: RigService,
    private confirmation: ConfirmationService) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const rigStreamCreator = (query) => this.rigService.getList(query);
    this.list.hookToQuery(rigStreamCreator).subscribe((response) => {
      this.rig = response;
    });
  }

  lengthDisplay(lengthMm?: number, lengthUnit?: SizeUnit): string {
    return formatLength(lengthMm, lengthUnit);
  }

  hookWeightDisplay(hookWeightG?: number, hookWeightUnit?: HookWeightUnit): string {
    return formatHookWeight(hookWeightG, hookWeightUnit);
  }

  createRig() {
    this.rigItem = null;
    this.view = 'rigForm';
  }

  async editRig(id) {
    this.rigItem = await lastValueFrom(this.rigService.get(id));
    this.view = 'rigForm';
  }

  onRigSaved() {
    this.view = '';
    this.list.get();
  }

  closeRigForm() {
    this.view = '';
  }

  deleteRig(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.rigService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
