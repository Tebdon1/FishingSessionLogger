import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { SessionService, SessionDto, CatchDto, CreateUpdateCatchDto, BaitDto } from '@proxy/sessions';
import { SpeciesService, SpeciesDto } from '@proxy/species';
import { VenueService, VenueDto } from '@proxy/venues';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';
import { BaitService } from '../home/services/bait.service';

@Component({
  selector: 'app-session',
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.scss'],
  providers: [
    ListService,
  ],
})
export class SessionComponent implements OnInit {
  sessionItem: SessionDto;
  editSessionItem: EditableSession;

  editCatchItem: EditableCatch;
  editingCatchIndex: number | null = null;

  bulkForm: FormGroup;

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;

  sessionForm: FormGroup;
  catchForm: FormGroup;

  speciesList: SpeciesDto[] = [];
  baitList: BaitDto[] = [];
  venueList: VenueDto[] = [];

  view = '';

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private speciesService: SpeciesService,
    private baitService: BaitService,
    private venueService: VenueService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder) {
      this.list.maxResultCount = 25;
    }

  ngOnInit() {
    const sessionStreamCreator = (query) => this.sessionService.getList(query);
    this.list.hookToQuery(sessionStreamCreator).subscribe((response) => {
      this.session = response;
    });
    this.loadLookups();
  }

  async loadLookups() {
    const [species, baits, venues] = await Promise.all([
      lastValueFrom(this.speciesService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.baitService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.venueService.getList({ maxResultCount: 1000 })),
    ]);
    this.speciesList = species.items;
    this.baitList = baits.items;
    this.venueList = venues.items;
  }

  speciesName(speciesId: number): string {
    return this.speciesList.find(s => s.id === speciesId)?.name ?? '';
  }

  baitName(baitId?: number): string {
    if (baitId == null) return '';
    return this.baitList.find(b => b.id === baitId)?.name ?? '';
  }

  deleteSession(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.sessionService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  // session level

  createSession() {
    this.sessionItem = null;

    this.editSessionItem = {
      startDateTime: toLocalDateTimeInput(new Date()),
      endDateTime: toLocalDateTimeInput(new Date()),
      venueId: null,
      notes: '',
      catches: [],
    };

    this.buildSessionForm(this.editSessionItem);
    this.view = 'sessionForm';
  }

  async editSession(id) {
    this.sessionItem = await lastValueFrom(this.sessionService.get(id));

    this.editSessionItem = {
      startDateTime: toLocalDateTimeInput(new Date(this.sessionItem.startDateTime)),
      endDateTime: toLocalDateTimeInput(new Date(this.sessionItem.endDateTime)),
      venueId: this.sessionItem.venueId,
      notes: this.sessionItem.notes ?? '',
      catches: this.sessionItem.catches.map(c => ({
        sessionId: c.sessionId,
        speciesId: c.speciesId,
        baitId: c.baitId,
        weight: c.weight,
        photoId: c.photoId,
        photoFileName: c.photoFileName,
      })),
    };

    this.buildSessionForm(this.editSessionItem);
    this.view = 'sessionForm';
  }

  buildSessionForm(sessionItem: EditableSession) {
    this.sessionForm = this.fb.group({
      startDateTime: [sessionItem.startDateTime, Validators.required],
      endDateTime: [sessionItem.endDateTime, Validators.required],
      venueId: [sessionItem.venueId, Validators.required],
      notes: [sessionItem.notes || ''],
    });
  }

  async saveSession() {
    if (this.sessionForm.invalid) {
      return;
    }

    const formValue = this.sessionForm.value;

    const payload = {
      startDateTime: new Date(formValue.startDateTime).toISOString(),
      endDateTime: new Date(formValue.endDateTime).toISOString(),
      venueId: formValue.venueId,
      notes: formValue.notes,
      catches: this.editSessionItem.catches,
    };

    if (this.sessionItem) {
      await lastValueFrom(this.sessionService.update(this.sessionItem.id, payload));
    }
    else {
      await lastValueFrom(this.sessionService.create(payload));
    }

    this.view = '';
    this.sessionForm.reset();
    this.list.get();
  }

  closeSessionForm() {
    this.view = '';
  }

  // grouping catches by species for display

  getSpeciesGroups(): SpeciesGroup[] {
    if (!this.editSessionItem) return [];

    const groups = new Map<number, SpeciesGroup>();
    this.editSessionItem.catches.forEach((c, index) => {
      let group = groups.get(c.speciesId);
      if (!group) {
        group = { speciesId: c.speciesId, speciesName: this.speciesName(c.speciesId), entries: [] };
        groups.set(c.speciesId, group);
      }
      group.entries.push({ index, catchItem: c });
    });

    return Array.from(groups.values()).sort((a, b) => a.speciesName.localeCompare(b.speciesName));
  }

  groupWeights(group: SpeciesGroup): string {
    const weights = group.entries.map(e => e.catchItem.weight).filter(w => w != null);
    return weights.length ? weights.join(', ') : '-';
  }

  groupBaits(group: SpeciesGroup): string {
    const baits = Array.from(new Set(group.entries
      .map(e => this.baitName(e.catchItem.baitId))
      .filter(b => b)));
    return baits.length ? baits.join(', ') : '-';
  }

  // single catch entry

  addCatch() {
    this.editingCatchIndex = null;
    this.editCatchItem = { speciesId: null, baitId: null, weight: null, photoData: null, photoFileName: null };
    this.buildCatchForm(this.editCatchItem);
    this.view = 'catchForm';
  }

  editCatch(index: number) {
    this.editingCatchIndex = index;
    const existing = this.editSessionItem.catches[index];
    this.editCatchItem = {
      speciesId: existing.speciesId,
      baitId: existing.baitId ?? null,
      weight: existing.weight ?? null,
      photoData: null,
      photoFileName: existing.photoFileName ?? null,
    };
    this.buildCatchForm(this.editCatchItem);
    this.view = 'catchForm';
  }

  buildCatchForm(catchItem: EditableCatch) {
    this.catchForm = this.fb.group({
      speciesId: [catchItem.speciesId, Validators.required],
      baitId: [catchItem.baitId],
      weight: [catchItem.weight],
    });
  }

  async onCatchPhotoSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.editCatchItem.photoData = await fileToBase64(file);
    this.editCatchItem.photoFileName = file.name;
    this.editCatchItem.photoMimeType = file.type || 'image/jpeg';
  }

  get catchPhotoPreviewUrl(): string | null {
    if (!this.editCatchItem?.photoData) return null;
    return `data:${this.editCatchItem.photoMimeType};base64,${this.editCatchItem.photoData}`;
  }

  photoUrl(photoId?: number): string | null {
    return photoId ? `/api/app/catch-photo/${photoId}` : null;
  }

  saveCatch() {
    if (this.catchForm.invalid) {
      return;
    }

    const formValue = this.catchForm.value;

    const catchEntry: CreateUpdateCatchDto = {
      sessionId: this.sessionItem?.id ?? 0,
      speciesId: formValue.speciesId,
      baitId: formValue.baitId || null,
      weight: formValue.weight === '' || formValue.weight == null ? null : formValue.weight,
      photoData: this.editCatchItem.photoData ?? undefined,
      photoFileName: this.editCatchItem.photoFileName ?? undefined,
    };

    if (this.editingCatchIndex != null) {
      this.editSessionItem.catches[this.editingCatchIndex] = catchEntry;
    }
    else {
      this.editSessionItem.catches.push(catchEntry);
    }

    this.view = 'sessionForm';
  }

  closeCatchForm() {
    this.view = 'sessionForm';
  }

  deleteCatch(index: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.editSessionItem.catches.splice(index, 1);
      }
    });
  }

  // bulk entry - fans out into N individual catch rows, no quantity stored

  addBulkCatch() {
    this.bulkForm = this.fb.group({
      speciesId: [null, Validators.required],
      baitId: [null],
      count: [1, [Validators.required, Validators.min(1), Validators.max(200)]],
      weightEach: [null],
    });
    this.view = 'bulkCatchForm';
  }

  saveBulkCatch() {
    if (this.bulkForm.invalid) {
      return;
    }

    const formValue = this.bulkForm.value;
    const weight = formValue.weightEach === '' || formValue.weightEach == null ? null : formValue.weightEach;

    for (let i = 0; i < formValue.count; i++) {
      this.editSessionItem.catches.push({
        sessionId: this.sessionItem?.id ?? 0,
        speciesId: formValue.speciesId,
        baitId: formValue.baitId || null,
        weight,
      });
    }

    this.view = 'sessionForm';
  }

  closeBulkCatchForm() {
    this.view = 'sessionForm';
  }
}

function toLocalDateTimeInput(date: Date): string {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

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

export interface EditableSession {
  startDateTime: string;
  endDateTime: string;
  venueId: number;
  notes: string;
  catches: CreateUpdateCatchDto[];
}

export interface EditableCatch {
  speciesId: number;
  baitId?: number;
  weight?: number;
  photoData?: string;
  photoFileName?: string;
  photoMimeType?: string;
}

export interface SpeciesGroup {
  speciesId: number;
  speciesName: string;
  entries: { index: number; catchItem: CreateUpdateCatchDto }[];
}
