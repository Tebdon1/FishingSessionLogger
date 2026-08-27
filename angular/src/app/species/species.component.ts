import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { SpeciesService, SpeciesDto } from '@proxy/species';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-species',
  templateUrl: './species.component.html',
  styleUrls: ['./species.component.scss'],
  providers: [
    ListService,
  ],
})
export class SpeciesComponent implements OnInit {
  speciesItem: SpeciesDto;
  editSpeciesItem: any;

  species = { items: [], totalCount: 0 } as PagedResultDto<SpeciesDto>;

  speciesForm: FormGroup;

  view = '';

  constructor(
    public readonly list: ListService,
    private speciesService: SpeciesService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const speciesStreamCreator = (query) => this.speciesService.getList(query);
    this.list.hookToQuery(speciesStreamCreator).subscribe((response) => {
      this.species = response;
    });
  }

  createSpecies() {
    this.speciesItem = null;
    this.editSpeciesItem = { name: '', isSaltwater: false };
    this.buildSpeciesForm(this.editSpeciesItem);
    this.view = 'speciesForm';
  }

  async editSpecies(id) {
    this.speciesItem = await lastValueFrom(this.speciesService.get(id));
    this.editSpeciesItem = JSON.parse(JSON.stringify(this.speciesItem));
    this.buildSpeciesForm(this.editSpeciesItem);
    this.view = 'speciesForm';
  }

  buildSpeciesForm(speciesItem: any) {
    this.speciesForm = this.fb.group({
      name: [speciesItem.name || '', [Validators.required, Validators.maxLength(255)]],
      isSaltwater: [speciesItem.isSaltwater ?? false],
    });
  }

  async saveSpecies() {
    if (this.speciesForm.invalid) {
      return;
    }

    const formValue = this.speciesForm.value;

    if (this.speciesItem) {
      await lastValueFrom(this.speciesService.update(this.speciesItem.id, formValue));
    }
    else {
      await lastValueFrom(this.speciesService.create(formValue));
    }

    this.view = '';
    this.speciesForm.reset();
    this.list.get();
  }

  closeSpeciesForm() {
    this.view = '';
  }

  deleteSpecies(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.speciesService.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
