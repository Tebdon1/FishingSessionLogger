import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SessionService, SessionDto, CatchDto, CreateUpdateCatchDto, BaitDto, SizeUnit, WeightUnit } from '@proxy/sessions';
import { SpeciesService, SpeciesDto } from '@proxy/species';
import { VenueService, VenueDto } from '@proxy/venues';
import { TicketService, TicketDto } from '@proxy/tickets';
import { MethodService, MethodDto } from '@proxy/methods';
import { RigService, RigDto, HookWeightUnit } from '@proxy/rigs';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { lastValueFrom, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BaitService } from '@proxy/baits';
import { UrlService } from '../services/url.service';
import { lengthDisplay as formatLength, weightDisplay as formatWeight, hookWeightDisplay as formatHookWeight, lengthMmToInput, weightGToInput, lengthInputToMm, weightInputToGrams } from '../shared/unit-display';
import { sessionCatchCount, sessionSpeciesSummary as sharedSpeciesSummary, sessionBestCatch as sharedBestCatch } from '../shared/session-summary';

@Component({
  selector: 'app-session',
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.scss'],
  providers: [
    ListService,
  ],
})
export class SessionComponent implements OnInit {
  @ViewChild('catchModal') catchModalTpl: TemplateRef<any>;
  @ViewChild('bulkModal') bulkModalTpl: TemplateRef<any>;
  @ViewChild('venueModal') venueModalTpl: TemplateRef<any>;
  @ViewChild('photoModal') photoModalTpl: TemplateRef<any>;
  @ViewChild('baitModal') baitModalTpl: TemplateRef<any>;
  @ViewChild('methodModal') methodModalTpl: TemplateRef<any>;
  @ViewChild('rigModal') rigModalTpl: TemplateRef<any>;

  sessionItem: SessionDto;
  editSessionItem: EditableSession;

  editCatchItem: EditableCatch;
  editingCatchIndex: number | null = null;

  viewingPhotoUrl: string | null = null;

  bulkForm: FormGroup;

  session = { items: [], totalCount: 0 } as PagedResultDto<SessionDto>;

  sessionForm: FormGroup;
  catchForm: FormGroup;

  speciesList: SpeciesDto[] = [];
  baitList: BaitDto[] = [];
  venueList: VenueDto[] = [];
  ticketList: TicketDto[] = [];
  methodList: MethodDto[] = [];
  rigList: RigDto[] = [];

  SizeUnit = SizeUnit;
  WeightUnit = WeightUnit;

  view = '';
  expandedSpeciesId: number | null = null;

  constructor(
    public readonly list: ListService,
    private sessionService: SessionService,
    private speciesService: SpeciesService,
    private baitService: BaitService,
    private venueService: VenueService,
    private ticketService: TicketService,
    private methodService: MethodService,
    private rigService: RigService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private modalService: NgbModal,
    private fb: FormBuilder,
    private urlService: UrlService,
    private http: HttpClient) {
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
    const [species, baits, venues, tickets, methods, rigs] = await Promise.all([
      lastValueFrom(this.speciesService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.baitService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.venueService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.ticketService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.methodService.getList({ maxResultCount: 1000 })),
      lastValueFrom(this.rigService.getList({ maxResultCount: 1000 })),
    ]);
    this.speciesList = species.items;
    this.baitList = baits.items;
    this.venueList = venues.items;
    this.ticketList = tickets.items;
    this.methodList = methods.items;
    this.rigList = rigs.items.map(r => ({ ...r, displayLabel: rigDisplayLabel(r) }));
  }

  speciesName(speciesId: number): string {
    return this.speciesList.find(s => s.id === speciesId)?.name ?? '';
  }

  baitName(baitId?: number): string {
    if (baitId == null) return '';
    return this.baitList.find(b => b.id === baitId)?.name ?? '';
  }

  methodName(methodId?: number): string {
    if (methodId == null) return '';
    return this.methodList.find(m => m.id === methodId)?.name ?? '';
  }

  rigName(rigId?: number): string {
    if (rigId == null) return '';
    const rig = this.rigList.find(r => r.id === rigId) as any;
    return rig?.displayLabel ?? rig?.name ?? '';
  }

  lengthDisplay(lengthMm?: number, unit?: SizeUnit): string {
    return formatLength(lengthMm, unit);
  }

  weightDisplay(weightG?: number, unit?: WeightUnit): string {
    return formatWeight(weightG, unit);
  }

  // --- Summary helpers for the session list cards -------------------------
  // The list response embeds each session's catches, so a card can show what
  // was actually caught rather than just a start time and a duration. Shared
  // with the home page's recent-activity preview - see ../shared/session-summary.

  catchCount(session: SessionDto): number {
    return sessionCatchCount(session);
  }

  sessionSpeciesSummary(session: SessionDto): { name: string; count: number }[] {
    return sharedSpeciesSummary(session, id => this.speciesName(id));
  }

  sessionBestCatch(session: SessionDto): string {
    return sharedBestCatch(session, id => this.speciesName(id));
  }

  get catchWeightIsLbOz(): boolean {
    return this.catchForm?.value.weightUnit === WeightUnit.LbOz;
  }

  get bulkWeightIsLbOz(): boolean {
    return this.bulkForm?.value.weightUnitEach === WeightUnit.LbOz;
  }

  deleteSession(id: number) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.sessionService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

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
      // Catches not reopened via editCatch() in this session-edit pass are resent
      // as-is on save - reconstructing the value in the catch's own already-stored
      // unit (rather than a hardcoded one) means the backend's conversion round-trips
      // losslessly for them, instead of silently zeroing WeightG/LengthMm because
      // WeightLbs/WeightOz/LengthValue were never populated for an untouched row, and
      // without overwriting the real stored WeightUnit/LengthUnit with a default.
      catches: this.sessionItem.catches.map(c => {
        const weightInput = weightGToInput(c.weightG, c.weightUnit);
        const lengthInput = lengthMmToInput(c.lengthMm, c.lengthUnit);
        return {
          id: c.id,
          sessionId: c.sessionId,
          speciesId: c.speciesId,
          baitId: c.baitId,
          ...weightInput,
          weightG: c.weightG,
          methodId: c.methodId,
          rigId: c.rigId,
          ...lengthInput,
          lengthMm: c.lengthMm,
          catchTime: c.catchTime,
          photoId: c.photoId,
          photoFileName: c.photoFileName,
          notes: c.notes,
        };
      }),
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
      // Sent as the local wall-clock value the user entered, with no UTC conversion -
      // see toDateTimeInputIso for why (this was previously .toISOString(), which
      // shifted the time by the browser's UTC offset on every single save).
      startDateTime: toDateTimeInputIso(formValue.startDateTime),
      endDateTime: toDateTimeInputIso(formValue.endDateTime),
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
    const weights = group.entries
      .map(e => this.weightDisplay((e.catchItem as any).weightG, (e.catchItem as any).weightUnit))
      .filter(w => w);
    return weights.length ? weights.join(', ') : '-';
  }

  groupBaits(group: SpeciesGroup): string {
    const baits = Array.from(new Set(group.entries
      .map(e => this.baitName(e.catchItem.baitId))
      .filter(b => b)));
    return baits.length ? baits.join(', ') : '-';
  }

  groupHasPhotos(group: SpeciesGroup): boolean {
    return group.entries.some(e => this.hasPhoto(e.catchItem));
  }

  groupHasNotes(group: SpeciesGroup): boolean {
    return group.entries.some(e => !!(e.catchItem as any).notes);
  }

  // single catch entry - opens in a modal so the session form stays visible behind it

  addCatch() {
    this.editingCatchIndex = null;
    this.editCatchItem = {
      speciesId: null, baitId: null,
      weightUnit: WeightUnit.LbOz, weightLbs: null, weightOz: null, weightValue: null,
      methodId: null, rigId: null,
      lengthValue: null, lengthUnit: SizeUnit.Centimetres, catchTime: null,
      photoData: null, photoFileName: null, notes: null,
    };
    this.buildCatchForm(this.editCatchItem);
    this.modalService.open(this.catchModalTpl, { size: 'md' });
  }

  editCatch(index: number) {
    this.editingCatchIndex = index;
    const existing = this.editSessionItem.catches[index] as any;
    // Re-editing starts from the unit it was originally entered in, so the form
    // shows the value the way the angler entered it rather than a reformatted one.
    // Rows with no stored unit (created before WeightUnit/LengthUnit existed) fall
    // back to the same defaults addCatch() uses for a brand new catch.
    const weightInput = weightGToInput(existing.weightG, existing.weightUnit ?? WeightUnit.LbOz);
    const lengthInput = lengthMmToInput(existing.lengthMm, existing.lengthUnit ?? SizeUnit.Centimetres);
    this.editCatchItem = {
      id: existing.id,
      speciesId: existing.speciesId,
      baitId: existing.baitId ?? null,
      weightUnit: weightInput.weightUnit,
      weightLbs: weightInput.weightLbs ?? null,
      weightOz: weightInput.weightOz ?? null,
      weightValue: weightInput.weightValue ?? null,
      methodId: existing.methodId ?? null,
      rigId: existing.rigId ?? null,
      lengthValue: lengthInput.lengthValue ?? null,
      lengthUnit: lengthInput.lengthUnit,
      catchTime: timeSpanToTimeInput(existing.catchTime),
      // A pending upload (attached but not yet saved to the server, so it has no
      // photoId to fetch by) is carried forward as-is so it keeps previewing on
      // re-edit; only fall back to fetching by id once it round-trips and gets one.
      photoData: existing.photoId ? null : existing.photoData ?? null,
      photoFileName: existing.photoFileName ?? null,
      photoMimeType: existing.photoId ? null : existing.photoMimeType ?? null,
      existingPhotoId: existing.photoId ?? null,
      existingPhotoUrl: null,
      notes: existing.notes ?? null,
    };
    this.buildCatchForm(this.editCatchItem);
    this.modalService.open(this.catchModalTpl, { size: 'md' });

    if (existing.photoId) {
      const catchItem = this.editCatchItem;
      this.loadPhotoBlobUrl(existing.photoId).subscribe(url => catchItem.existingPhotoUrl = url);
    }
  }

  buildCatchForm(catchItem: EditableCatch) {
    this.catchForm = this.fb.group({
      speciesId: [catchItem.speciesId, Validators.required],
      baitId: [catchItem.baitId],
      weightUnit: [catchItem.weightUnit ?? WeightUnit.LbOz],
      weightLbs: [catchItem.weightLbs],
      weightOz: [catchItem.weightOz, [Validators.min(0), Validators.max(15)]],
      weightValue: [catchItem.weightValue],
      methodId: [catchItem.methodId],
      rigId: [catchItem.rigId],
      lengthValue: [catchItem.lengthValue],
      lengthUnit: [catchItem.lengthUnit ?? SizeUnit.Centimetres],
      catchTime: [catchItem.catchTime],
      notes: [catchItem.notes || ''],
    });
  }

  async onCatchPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (file.size > MAX_PHOTO_BYTES) {
      this.toaster.error(`Photo must be smaller than ${MAX_PHOTO_BYTES / (1024 * 1024)}MB.`, 'Photo too large');
      input.value = '';
      return;
    }

    this.editCatchItem.photoData = await fileToBase64(file);
    this.editCatchItem.photoFileName = file.name;
    this.editCatchItem.photoMimeType = file.type || 'image/jpeg';
    // A fresh selection supersedes an earlier removal in the same edit.
    this.editCatchItem.removePhoto = false;
  }

  removeCatchPhoto(photoInput: HTMLInputElement) {
    this.editCatchItem.photoData = null;
    this.editCatchItem.photoFileName = null;
    this.editCatchItem.photoMimeType = null;
    this.editCatchItem.existingPhotoId = null;
    this.editCatchItem.existingPhotoUrl = null;
    this.editCatchItem.removePhoto = true;
    photoInput.value = '';
  }

  get catchPhotoPreviewUrl(): string | null {
    if (!this.editCatchItem?.photoData) return null;
    return `data:${this.editCatchItem.photoMimeType};base64,${this.editCatchItem.photoData}`;
  }

  get hasCatchPhoto(): boolean {
    return !!(this.editCatchItem?.photoData || this.editCatchItem?.existingPhotoId);
  }

  // The photo endpoint requires the app's Bearer token, which a plain <img src> can't
  // attach - fetched via HttpClient (whose interceptor adds the token) and turned into
  // an object URL instead, same trick for both the edit-form preview and the viewer.
  private loadPhotoBlobUrl(photoId: number): Observable<string> {
    return this.http
      .get(this.urlService.apiUrl(`/catch-photo/${photoId}`), { responseType: 'blob' })
      .pipe(map(blob => URL.createObjectURL(blob)));
  }

  // catchItem may be a catch with a server-confirmed photoId (fetched by id), or one
  // with only a pending upload not yet saved to the server - see hasPhoto().
  viewPhoto(catchItem: any) {
    this.viewingPhotoUrl = null;
    const modalRef = this.modalService.open(this.photoModalTpl, { size: 'lg' });

    if (catchItem.photoId) {
      this.loadPhotoBlobUrl(catchItem.photoId).subscribe(url => this.viewingPhotoUrl = url);
      modalRef.result.finally(() => {
        if (this.viewingPhotoUrl) {
          URL.revokeObjectURL(this.viewingPhotoUrl);
          this.viewingPhotoUrl = null;
        }
      });
    }
    else if (catchItem.photoData) {
      // A pending upload has no photoId to fetch by yet - preview straight from the
      // same local base64 data the edit-form preview already uses for it.
      this.viewingPhotoUrl = `data:${catchItem.photoMimeType ?? 'image/jpeg'};base64,${catchItem.photoData}`;
    }
  }

  hasPhoto(catchItem: any): boolean {
    return !!(catchItem.photoId || catchItem.photoData);
  }

  saveCatch(modal: NgbActiveModal) {
    if (this.catchForm.invalid) {
      return;
    }

    const formValue = this.catchForm.value;

    const catchEntry: CreateUpdateCatchDto = {
      id: this.editCatchItem.id,
      sessionId: this.sessionItem?.id ?? 0,
      speciesId: formValue.speciesId,
      baitId: formValue.baitId || null,
      weightUnit: formValue.weightUnit ?? WeightUnit.LbOz,
      weightLbs: formValue.weightLbs === '' || formValue.weightLbs == null ? null : formValue.weightLbs,
      weightOz: formValue.weightOz === '' || formValue.weightOz == null ? null : formValue.weightOz,
      weightValue: formValue.weightValue === '' || formValue.weightValue == null ? null : formValue.weightValue,
      methodId: formValue.methodId || null,
      rigId: formValue.rigId || null,
      lengthValue: formValue.lengthValue === '' || formValue.lengthValue == null ? null : formValue.lengthValue,
      lengthUnit: formValue.lengthUnit ?? SizeUnit.Centimetres,
      catchTime: timeInputToTimeSpan(formValue.catchTime),
      photoData: this.editCatchItem.photoData ?? undefined,
      photoFileName: this.editCatchItem.photoFileName ?? undefined,
      removePhoto: this.editCatchItem.removePhoto ?? false,
      notes: formValue.notes || undefined,
    };

    // weightG/lengthMm are display-only fields computed server-side and don't exist on
    // CreateUpdateCatchDto - recomputed here (mirroring the backend's own conversion) so
    // the session-edit table and a later re-edit of this same catch see the correct
    // values immediately, instead of going blank until the whole session round-trips to
    // the server. photoMimeType is similarly carried forward so a just-attached photo
    // (not yet saved server-side, so it has no photoId to fetch by) still previews on
    // re-edit instead of appearing to have been forgotten.
    const displayFields = {
      weightG: weightInputToGrams(catchEntry),
      lengthMm: lengthInputToMm(catchEntry.lengthValue, catchEntry.lengthUnit),
      photoMimeType: catchEntry.removePhoto ? undefined : this.editCatchItem.photoMimeType,
      // preserve photoId for display until the session is reloaded from the server;
      // undefined (not just falsy) when removed/pending so hasPhoto() reads it as gone
      photoId: catchEntry.removePhoto || this.editCatchItem.photoData ? undefined : this.editCatchItem.existingPhotoId,
    };

    if (this.editingCatchIndex != null) {
      this.editSessionItem.catches[this.editingCatchIndex] = { ...catchEntry, ...displayFields } as any;
    }
    else {
      this.editSessionItem.catches.push({ ...catchEntry, ...displayFields } as any);
    }

    modal.close();
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
      methodId: [null],
      rigId: [null],
      count: [1, [Validators.required, Validators.min(1), Validators.max(200)]],
      weightUnitEach: [WeightUnit.LbOz],
      weightLbsEach: [null],
      weightOzEach: [null, [Validators.min(0), Validators.max(15)]],
      weightValueEach: [null],
    });
    this.modalService.open(this.bulkModalTpl, { size: 'md' });
  }

  saveBulkCatch(modal: NgbActiveModal) {
    if (this.bulkForm.invalid) {
      return;
    }

    const formValue = this.bulkForm.value;
    const weightUnit = formValue.weightUnitEach ?? WeightUnit.LbOz;
    const weightLbs = formValue.weightLbsEach === '' || formValue.weightLbsEach == null ? null : formValue.weightLbsEach;
    const weightOz = formValue.weightOzEach === '' || formValue.weightOzEach == null ? null : formValue.weightOzEach;
    const weightValue = formValue.weightValueEach === '' || formValue.weightValueEach == null ? null : formValue.weightValueEach;

    for (let i = 0; i < formValue.count; i++) {
      this.editSessionItem.catches.push({
        sessionId: this.sessionItem?.id ?? 0,
        speciesId: formValue.speciesId,
        baitId: formValue.baitId || null,
        methodId: formValue.methodId || null,
        rigId: formValue.rigId || null,
        lengthUnit: SizeUnit.Centimetres,
        weightUnit,
        weightLbs,
        weightOz,
        weightValue,
      });
    }

    modal.close();
  }

  // venue quick-add - lets the user create a venue without losing session form progress

  addVenueQuickAdd() {
    this.modalService.open(this.venueModalTpl, { size: 'md' });
  }

  onVenueQuickAddSaved(newVenue: VenueDto, modal: NgbActiveModal) {
    this.venueList = [...this.venueList, newVenue].sort((a, b) => a.name.localeCompare(b.name));
    this.sessionForm.patchValue({ venueId: newVenue.id });

    modal.close();
  }

  // bait/method/rig quick-add - same pattern as venue quick-add, but patch into the
  // catch form (these are fields on a catch, not the session itself)

  addBaitQuickAdd() {
    this.modalService.open(this.baitModalTpl, { size: 'md' });
  }

  onBaitQuickAddSaved(newBait: BaitDto, modal: NgbActiveModal) {
    this.baitList = [...this.baitList, newBait].sort((a, b) => a.name.localeCompare(b.name));
    this.catchForm.patchValue({ baitId: newBait.id });

    modal.close();
  }

  addMethodQuickAdd() {
    this.modalService.open(this.methodModalTpl, { size: 'md' });
  }

  onMethodQuickAddSaved(newMethod: MethodDto, modal: NgbActiveModal) {
    this.methodList = [...this.methodList, newMethod].sort((a, b) => a.name.localeCompare(b.name));
    this.catchForm.patchValue({ methodId: newMethod.id });

    modal.close();
  }

  addRigQuickAdd() {
    this.modalService.open(this.rigModalTpl, { size: 'md' });
  }

  onRigQuickAddSaved(newRig: RigDto, modal: NgbActiveModal) {
    const withLabel = { ...newRig, displayLabel: rigDisplayLabel(newRig) };
    this.rigList = [...this.rigList, withLabel].sort((a, b) => a.name.localeCompare(b.name));
    this.catchForm.patchValue({ rigId: newRig.id });

    modal.close();
  }
}

function toLocalDateTimeInput(date: Date): string {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

// The backend stores/returns StartDateTime/EndDateTime as naive wall-clock values (no
// UTC offset), and the browser's Date parser reads a no-offset ISO string as local time
// - so this round-trips losslessly with toLocalDateTimeInput above, with no timezone
// conversion happening anywhere. Mirrors CatchTime's TimeSpan handling, which sidesteps
// the same class of issue by never going through a Date object at all.
function toDateTimeInputIso(value: string): string {
  return value.length === 16 ? `${value}:00` : value;
}

// <input type="time"> gives/expects "HH:mm"; the backend's TimeSpan serializes as "HH:mm:ss".
function timeInputToTimeSpan(value?: string | null): string | undefined {
  if (!value) return undefined;
  return value.length === 5 ? `${value}:00` : value;
}

function timeSpanToTimeInput(value?: string | null): string | null {
  if (!value) return null;
  return value.substring(0, 5);
}

// Rig names aren't unique - fold the specs into the label so "Ronnie Rig" tied two
// different ways reads as two distinct options instead of an ambiguous duplicate.
function rigDisplayLabel(rig: {
  name: string;
  lengthMm?: number;
  lengthUnit?: SizeUnit;
  hookSize?: string;
  hookWeightG?: number;
  hookWeightUnit?: HookWeightUnit;
  hookPattern?: string;
  materials?: string;
}): string {
  const parts = [
    rig.lengthMm != null ? formatLength(rig.lengthMm, rig.lengthUnit) : null,
    rig.hookSize ? `Size ${rig.hookSize}` : null,
    rig.hookWeightG != null ? formatHookWeight(rig.hookWeightG, rig.hookWeightUnit) : null,
    rig.hookPattern || null,
    rig.materials || null,
  ].filter(p => p);

  return parts.length ? `${rig.name} (${parts.join(', ')})` : rig.name;
}

// Matches CreateUpdateCatchDto.MaxPhotoBytes on the backend.
const MAX_PHOTO_BYTES = 8 * 1024 * 1024;

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
  id?: number;
  speciesId: number;
  baitId?: number;
  weightUnit?: WeightUnit;
  weightLbs?: number;
  weightOz?: number;
  weightValue?: number;
  methodId?: number;
  rigId?: number;
  lengthValue?: number;
  lengthUnit?: SizeUnit;
  catchTime?: string;
  photoData?: string;
  photoFileName?: string;
  photoMimeType?: string;
  existingPhotoId?: number;
  existingPhotoUrl?: string;
  removePhoto?: boolean;
  notes?: string;
}

export interface SpeciesGroup {
  speciesId: number;
  speciesName: string;
  entries: { index: number; catchItem: CreateUpdateCatchDto }[];
}
