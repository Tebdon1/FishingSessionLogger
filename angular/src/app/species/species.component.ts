import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SpeciesService, SpeciesDto, SpeciesWaterType } from '@proxy/species';
import { PersonalBestService, PersonalBestDto } from '@proxy/personal-bests';
import { UserPreferenceService, PersonalBestMetric } from '@proxy/user-preferences';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { combineLatest, lastValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { weightDisplay, lengthDisplay } from '../shared/unit-display';
import { UrlService } from '../services/url.service';

// Matches SpeciesUpdateDto.MaxPhotoBytes on the backend.
const MAX_PHOTO_BYTES = 8 * 1024 * 1024;

// Shown when a species has neither a personal-best photo nor an admin-set one.
const SPECIES_IMAGE_FALLBACK = '/assets/images/species-placeholder.svg';

function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      resolve(result.substring(result.indexOf(',') + 1));
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

@Component({
  selector: 'app-species',
  templateUrl: './species.component.html',
  styleUrls: ['./species.component.scss'],
  providers: [
    ListService,
  ],
})
export class SpeciesComponent implements OnInit {
  SpeciesWaterType = SpeciesWaterType;

  speciesItem: SpeciesDto;
  editSpeciesItem: any;

  species = { items: [], totalCount: 0 } as PagedResultDto<SpeciesDto>;

  // The species page loads everything at once (no pager), so filtering/sorting is
  // just an in-memory transform of `species.items` rather than a server round trip.
  displayedSpecies: SpeciesDto[] = [];

  nameFilter = '';
  waterTypeFilter: SpeciesWaterType | 'all' = 'all';
  sortBy: 'name' | 'pb' = 'name';

  speciesForm: FormGroup;

  view = '';

  personalBests: Record<number, PersonalBestDto> = {};
  personalBestMetric = PersonalBestMetric.Weight;

  // Object URLs for the record catch's photo, keyed by species id - populated only
  // for species whose current-metric record catch actually has a photo attached.
  photoUrls: Record<number, string> = {};

  // Object URLs for each species' admin-set default photo, keyed by species id -
  // shown on a tile only when there's no personal-best photo taking priority.
  speciesPhotoUrls: Record<number, string> = {};

  constructor(
    public readonly list: ListService,
    private speciesService: SpeciesService,
    private personalBestService: PersonalBestService,
    private userPreferenceService: UserPreferenceService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private http: HttpClient,
    private urlService: UrlService) {
      // A reference pick-list, not paginated data - raised well past the old 25-row
      // cap (which had no pager, so anything past it was just invisible) so every
      // species actually shows up.
      this.list.maxResultCount = 1000;
    }

  ngOnInit() {
    const speciesStreamCreator = (query) => this.speciesService.getList(query);
    this.list.hookToQuery(speciesStreamCreator).subscribe((response) => {
      this.species = response;
      this.loadSpeciesPhotos(response.items);
      this.applyFilters();
    });

    // Which photo to show depends on the preference, so both are loaded together
    // before deciding which record catches' photos are worth fetching.
    combineLatest([
      this.personalBestService.getList(),
      this.userPreferenceService.get(),
    ]).subscribe(([personalBests, preference]) => {
      this.personalBestMetric = preference.personalBestMetric;
      this.personalBests = personalBests.reduce((map, pb) => {
        map[pb.speciesId] = pb;
        return map;
      }, {} as Record<number, PersonalBestDto>);
      this.loadPersonalBestPhotos();
      // Re-sort if "Largest PB first" is selected - this data arrives after the
      // species list itself, so the initial sort pass wouldn't have had it yet.
      this.applyFilters();
    });
  }

  applyFilters() {
    let items = [...this.species.items];

    const term = this.nameFilter.trim().toLowerCase();
    if (term) {
      items = items.filter((s) => s.name.toLowerCase().includes(term));
    }

    if (this.waterTypeFilter !== 'all') {
      items = items.filter((s) => {
        // "Both" is its own exact bucket - selecting it shows only dual-habitat
        // species. Selecting Freshwater or Saltwater specifically also includes
        // "Both" species, since they genuinely do turn up there too.
        if (this.waterTypeFilter === SpeciesWaterType.Both) {
          return s.waterType === SpeciesWaterType.Both;
        }
        return s.waterType === this.waterTypeFilter || s.waterType === SpeciesWaterType.Both;
      });
    }

    items.sort((a, b) => {
      if (this.sortBy === 'pb') {
        const aValue = this.personalBestSortValue(a.id);
        const bValue = this.personalBestSortValue(b.id);
        // Equality check first - two -Infinity values (neither has a PB) would
        // otherwise subtract to NaN, which Array.sort treats unpredictably rather
        // than as "equal, fall through to the alphabetical tiebreaker".
        if (aValue !== bValue) {
          return bValue - aValue;
        }
      }
      // Alphabetical is always the tiebreaker, and the sole criterion in "name" mode.
      return a.name.localeCompare(b.name);
    });

    this.displayedSpecies = items;
  }

  clearFilters() {
    this.nameFilter = '';
    this.waterTypeFilter = 'all';
    this.sortBy = 'name';
    this.applyFilters();
  }

  // Species with no personal best for the active metric sort to the bottom rather
  // than being excluded - "Largest PB first" is a ranking, not a filter.
  private personalBestSortValue(speciesId: number): number {
    const pb = this.personalBests[speciesId];
    if (!pb) {
      return -Infinity;
    }

    const value = this.personalBestMetric === PersonalBestMetric.Length ? pb.lengthMm : pb.weightG;
    return value ?? -Infinity;
  }

  // Only one of weight/length is shown per tile, per the user's preference - a
  // species the user hasn't caught yet (or hasn't recorded that metric for) shows
  // nothing rather than a misleading "0" or dash.
  personalBestLabel(speciesId: number): string {
    const pb = this.personalBests[speciesId];
    if (!pb) {
      return '';
    }

    if (this.personalBestMetric === PersonalBestMetric.Length) {
      return pb.lengthMm != null ? lengthDisplay(pb.lengthMm, pb.lengthUnit) : '';
    }

    return pb.weightG != null ? weightDisplay(pb.weightG, pb.weightUnit) : '';
  }

  // Priority: the user's own personal-best catch photo, then the admin-set species
  // default, then a generic placeholder.
  tileImage(row: SpeciesDto): string {
    return this.photoUrls[row.id] || this.speciesPhotoUrls[row.id] || SPECIES_IMAGE_FALLBACK;
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    if (!img.src.endsWith('species-placeholder.svg')) {
      img.src = SPECIES_IMAGE_FALLBACK;
    }
  }

  private loadPersonalBestPhotos() {
    for (const pb of Object.values(this.personalBests)) {
      const photoId = this.personalBestMetric === PersonalBestMetric.Length ? pb.lengthPhotoId : pb.weightPhotoId;
      if (photoId == null) {
        continue;
      }

      // Same approach as session.component.ts's loadPhotoBlobUrl - the endpoint
      // requires the app's Bearer token, which a plain <img src> can't attach.
      this.http
        .get(this.urlService.apiUrl(`/catch-photo/${photoId}`), { responseType: 'blob' })
        .pipe(map((blob) => URL.createObjectURL(blob)))
        .subscribe((url) => (this.photoUrls[pb.speciesId] = url));
    }
  }

  private loadSpeciesPhotos(rows: SpeciesDto[]) {
    for (const row of rows) {
      if (!row.hasPhoto) {
        continue;
      }

      this.http
        .get(this.urlService.apiUrl(`/species-photo/${row.id}`), { responseType: 'blob' })
        .pipe(map((blob) => URL.createObjectURL(blob)))
        .subscribe((url) => (this.speciesPhotoUrls[row.id] = url));
    }
  }

  createSpecies() {
    this.speciesItem = null;
    this.editSpeciesItem = { name: '', waterType: SpeciesWaterType.Freshwater };
    this.buildSpeciesForm(this.editSpeciesItem);
    this.view = 'speciesForm';
  }

  async editSpecies(id) {
    this.speciesItem = await lastValueFrom(this.speciesService.get(id));
    this.editSpeciesItem = JSON.parse(JSON.stringify(this.speciesItem));
    this.buildSpeciesForm(this.editSpeciesItem);
    this.view = 'speciesForm';

    if (this.speciesItem.hasPhoto) {
      this.http
        .get(this.urlService.apiUrl(`/species-photo/${this.speciesItem.id}`), { responseType: 'blob' })
        .pipe(map((blob) => URL.createObjectURL(blob)))
        .subscribe((url) => (this.editSpeciesItem.existingPhotoUrl = url));
    }
  }

  async onSpeciesPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (file.size > MAX_PHOTO_BYTES) {
      this.toaster.error(`Photo must be smaller than ${MAX_PHOTO_BYTES / (1024 * 1024)}MB.`, 'Photo too large');
      input.value = '';
      return;
    }

    this.editSpeciesItem.photoData = await fileToBase64(file);
    this.editSpeciesItem.photoFileName = file.name;
    this.editSpeciesItem.photoMimeType = file.type || 'image/jpeg';
    // A fresh selection supersedes an earlier removal in the same edit.
    this.editSpeciesItem.removePhoto = false;
  }

  removeSpeciesPhoto(photoInput: HTMLInputElement) {
    this.editSpeciesItem.photoData = null;
    this.editSpeciesItem.photoFileName = null;
    this.editSpeciesItem.photoMimeType = null;
    this.editSpeciesItem.existingPhotoUrl = null;
    this.editSpeciesItem.removePhoto = true;
    photoInput.value = '';
  }

  get speciesPhotoPreviewUrl(): string | null {
    if (!this.editSpeciesItem?.photoData) {
      return null;
    }
    return `data:${this.editSpeciesItem.photoMimeType};base64,${this.editSpeciesItem.photoData}`;
  }

  get hasSpeciesPhoto(): boolean {
    return !!(this.editSpeciesItem?.photoData || this.editSpeciesItem?.existingPhotoUrl);
  }

  buildSpeciesForm(speciesItem: any) {
    this.speciesForm = this.fb.group({
      name: [speciesItem.name || '', [Validators.required, Validators.maxLength(255)]],
      waterType: [speciesItem.waterType ?? SpeciesWaterType.Freshwater],
    });
  }

  async saveSpecies() {
    if (this.speciesForm.invalid) {
      return;
    }

    const formValue = {
      ...this.speciesForm.value,
      photoData: this.editSpeciesItem.photoData ?? undefined,
      photoFileName: this.editSpeciesItem.photoFileName ?? undefined,
      removePhoto: this.editSpeciesItem.removePhoto ?? false,
    };

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
