import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SessionService, SessionDto, CatchDto, CreateUpdateCatchDto, BaitDto, SizeUnit, WeightUnit } from '@proxy/sessions';
import { SpeciesService, SpeciesDto } from '@proxy/species';
import { VenueService, VenueDto } from '@proxy/venues';
import { TicketService, TicketDto } from '@proxy/tickets';
import { MethodService, MethodDto } from '@proxy/methods';
import { RigService, RigDto } from '@proxy/rigs';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { FormGroup, FormArray, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { lastValueFrom, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BaitService } from '@proxy/baits';
import { UrlService } from '../services/url.service';
import { lengthDisplay as formatLength, weightDisplay as formatWeight, lengthMmToInput, weightGToInput, lengthInputToMm, weightInputToGrams } from '../shared/unit-display';
import { rigDisplayLabel } from '../shared/rig-display';
import { sessionCatchCount, sessionSpeciesSummary as sharedSpeciesSummary, sessionBestCatch as sharedBestCatch } from '../shared/session-summary';
import { speciesForVenueWaterType } from '../shared/target-species';

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
      // The page filters, sorts and "shows more" in memory (see applyFilters), so it needs
      // every session up front. This used to be 25 with no pager, which silently hid
      // everything older; 1000 is the API's page-size ceiling, and a note under the list
      // says so if a log ever outgrows it.
      this.list.maxResultCount = 1000;
    }

  ngOnInit() {
    const sessionStreamCreator = (query) => this.sessionService.getList(query);
    this.list.hookToQuery(sessionStreamCreator).subscribe((response) => {
      this.session = response;
      // A reload (after a save or delete) shouldn't throw away how far down the list
      // the user had expanded, or what they'd filtered to.
      this.applyFilters(false);
    });
    this.loadLookups();
  }

  // --- Search, filters and sorting for the sessions list -------------------
  // Everything is an in-memory transform of `session.items`, the same approach as the
  // venues page - a personal log is small enough that a round trip per keystroke
  // would only add latency.

  readonly pageSize = 25;

  searchFilter = '';
  // Empty = no filter. A session matches if its venue / any of its catches' species is
  // any one of the selected values (an "or" within a filter, "and" across filters).
  venueFilter: number[] = [];
  speciesFilter: number[] = [];
  // yyyy-MM-dd, straight from <input type="date">.
  dateFrom = '';
  dateTo = '';
  outcomeFilter: 'all' | 'caught' | 'blank' = 'all';
  sortBy: 'newest' | 'oldest' | 'catches' = 'newest';

  // Offered from what's actually in the log rather than the full lookup lists, so the
  // dropdowns only list venues/species that can match something.
  venueFilterItems: { id: number; name: string }[] = [];
  speciesFilterItems: { id: number; name: string }[] = [];

  // Every session that passes the filters, in order; `visibleSessions` is the slice of it
  // rendered so far (grows by `pageSize` each time "Show more" is pressed).
  displayedSessions: SessionDto[] = [];
  visibleSessions: SessionDto[] = [];
  private visibleCount = this.pageSize;

  get hasActiveFilters(): boolean {
    return !!(
      this.searchFilter.trim() ||
      this.venueFilter.length ||
      this.speciesFilter.length ||
      this.dateFrom ||
      this.dateTo ||
      this.outcomeFilter !== 'all'
    );
  }

  get remainingSessions(): number {
    return this.displayedSessions.length - this.visibleSessions.length;
  }

  get venueFilterLabel(): string {
    return this.venueFilterItems
      .filter((v) => this.venueFilter.includes(v.id))
      .map((v) => v.name)
      .join(', ');
  }

  get speciesFilterLabel(): string {
    return this.speciesFilterItems
      .filter((s) => this.speciesFilter.includes(s.id))
      .map((s) => s.name)
      .join(', ');
  }

  applyFilters(resetPaging = true) {
    const items = this.session.items;

    this.venueFilterItems = this.distinctVenues(items);
    this.speciesFilterItems = this.distinctSpecies(items);
    // A selection whose venue/species has since vanished from the log (its last session
    // was deleted) would otherwise stay applied with nothing in the control to remove it.
    this.venueFilter = this.venueFilter.filter((id) => this.venueFilterItems.some((v) => v.id === id));
    this.speciesFilter = this.speciesFilter.filter((id) => this.speciesFilterItems.some((s) => s.id === id));

    const terms = this.searchFilter.trim().toLowerCase().split(/\s+/).filter((t) => t);
    const from = this.dateFrom ? new Date(`${this.dateFrom}T00:00:00`).getTime() : null;
    const to = this.dateTo ? new Date(`${this.dateTo}T23:59:59.999`).getTime() : null;

    let matches = items.filter((s) => {
      const start = new Date(s.startDateTime).getTime();
      if (from != null && start < from) return false;
      if (to != null && start > to) return false;
      if (this.outcomeFilter === 'caught' && s.isBlank) return false;
      if (this.outcomeFilter === 'blank' && !s.isBlank) return false;
      if (this.venueFilter.length && !this.venueFilter.includes(s.venueId)) return false;
      if (this.speciesFilter.length && !(s.catches ?? []).some((c) => this.speciesFilter.includes(c.speciesId))) {
        return false;
      }
      if (terms.length) {
        // Every word has to match somewhere (so "canal perch" narrows, not widens).
        const haystack = this.searchText(s);
        if (!terms.every((t) => haystack.includes(t))) return false;
      }
      return true;
    });

    // Sorted here rather than trusting the API's order: the server returns sessions by
    // when they were created, which isn't the order they were fished in once older trips
    // have been back-filled.
    const byStart = (a: SessionDto, b: SessionDto) =>
      new Date(b.startDateTime).getTime() - new Date(a.startDateTime).getTime();
    matches = [...matches].sort((a, b) => {
      switch (this.sortBy) {
        case 'oldest': return -byStart(a, b);
        case 'catches': return (b.catches?.length ?? 0) - (a.catches?.length ?? 0) || byStart(a, b);
        default: return byStart(a, b);
      }
    });

    if (resetPaging) {
      this.visibleCount = this.pageSize;
    }
    this.displayedSessions = matches;
    this.visibleSessions = matches.slice(0, this.visibleCount);
  }

  showMoreSessions() {
    this.visibleCount += this.pageSize;
    this.visibleSessions = this.displayedSessions.slice(0, this.visibleCount);
  }

  clearFilters() {
    this.searchFilter = '';
    this.venueFilter = [];
    this.speciesFilter = [];
    this.dateFrom = '';
    this.dateTo = '';
    this.outcomeFilter = 'all';
    this.sortBy = 'newest';
    this.applyFilters();
  }

  // Everything a search term can hit, lower-cased once per session: the venue, the
  // date as written out ("16 October 2026", so "october" finds a month), the session's
  // notes, and each catch's species/bait/method/rig/notes.
  private searchText(s: SessionDto): string {
    const parts: (string | undefined)[] = [
      s.venueName,
      s.notes,
      new Date(s.startDateTime).toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric' }),
    ];
    for (const c of s.catches ?? []) {
      parts.push(c.speciesName, c.baitName, c.methodName, c.rigName, c.notes);
    }
    return parts.filter((p) => p).join(' ').toLowerCase();
  }

  private distinctVenues(items: SessionDto[]): { id: number; name: string }[] {
    const byId = new Map<number, string>();
    for (const s of items) {
      byId.set(s.venueId, s.venueName || 'Unknown venue');
    }
    return [...byId.entries()].map(([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name));
  }

  private distinctSpecies(items: SessionDto[]): { id: number; name: string }[] {
    const byId = new Map<number, string>();
    for (const s of items) {
      for (const c of s.catches ?? []) {
        byId.set(c.speciesId, c.speciesName || 'Unknown');
      }
    }
    return [...byId.entries()].map(([id, name]) => ({ id, name })).sort((a, b) => a.name.localeCompare(b.name));
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
    this.refreshTargetSpeciesOptions();
  }

  // --- Target species ------------------------------------------------------
  // What the session set out to catch, limited to species that live where the venue is
  // (salt vs fresh). An empty selection is "Any" - no particular target.

  targetSpeciesOptions: SpeciesDto[] = [];

  // The selection as one comma-separated line, like the list's venue/species filters.
  get targetSpeciesLabel(): string {
    const selected: number[] = this.sessionForm?.value.targetSpeciesIds ?? [];
    return this.targetSpeciesOptions
      .filter((s) => selected.includes(s.id))
      .map((s) => s.name)
      .join(', ');
  }

  // Re-run whenever the venue changes or the species list arrives, since either can
  // change which species are on offer.
  refreshTargetSpeciesOptions() {
    const venueId = this.sessionForm?.value.venueId;
    const venue = this.venueList.find((v) => v.id === venueId);
    // A deleted species the session already targets stays on offer (the venue's water
    // type can't be judged for it - the stub doesn't carry one), so editing doesn't drop it.
    this.targetSpeciesOptions = [
      ...speciesForVenueWaterType(this.speciesList.filter((s) => !isDeletedStub(s)), venue?.waterType),
      ...this.speciesList.filter((s) => isDeletedStub(s)),
    ];

    // Switching from, say, a canal to the sea shouldn't leave freshwater species silently
    // selected where the dropdown no longer offers them.
    const control = this.sessionForm?.get('targetSpeciesIds');
    const current: number[] = control?.value ?? [];
    const stillValid = current.filter((id) => this.targetSpeciesOptions.some((s) => s.id === id));
    if (control && stillValid.length !== current.length) {
      control.setValue(stillValid);
    }
  }

  // --- Deleted lookups ------------------------------------------------------
  // Venues, species, baits, methods and rigs are soft-deleted, so a session can still point
  // at one the lookup lists no longer return. Without an entry the dropdown would render
  // that field blank even though the value is intact. Each such item is added back as a
  // stub named from the session itself, and the stubs are cleared again for the next form.

  private syncDeletedStubs(session: SessionDto | null) {
    const strip = <T>(list: T[]) => list.filter((x) => !isDeletedStub(x));
    this.speciesList = strip(this.speciesList);
    this.baitList = strip(this.baitList);
    this.methodList = strip(this.methodList);
    this.rigList = strip(this.rigList);
    this.venueList = strip(this.venueList);

    if (!session) {
      return;
    }

    const stub = (id: number, name: string) => ({
      id, name, displayName: name, displayLabel: name, isDeletedStub: true,
    }) as any;
    const missing = (list: { id?: number }[], id?: number | null) => id != null && !list.some((x) => x.id === id);

    if (missing(this.venueList, session.venueId)) {
      this.venueList = [...this.venueList, stub(session.venueId, session.venueName ?? 'Deleted venue')];
    }
    (session.targetSpeciesIds ?? []).forEach((id, i) => {
      if (missing(this.speciesList, id)) {
        this.speciesList = [...this.speciesList, stub(id, session.targetSpeciesNames?.[i] ?? 'Deleted species')];
      }
    });
    for (const c of session.catches ?? []) {
      if (missing(this.speciesList, c.speciesId)) {
        this.speciesList = [...this.speciesList, stub(c.speciesId, c.speciesName)];
      }
      if (missing(this.baitList, c.baitId)) {
        this.baitList = [...this.baitList, stub(c.baitId, c.baitName)];
      }
      if (missing(this.methodList, c.methodId)) {
        this.methodList = [...this.methodList, stub(c.methodId, c.methodName)];
      }
      if (missing(this.rigList, c.rigId)) {
        this.rigList = [...this.rigList, stub(c.rigId, c.rigName)];
      }
    }
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
    return this.bulkForm?.value.weightUnit === WeightUnit.LbOz;
  }

  get bulkWeights(): FormArray {
    return this.bulkForm.get('weights') as FormArray;
  }

  // Each weight row belongs to one catch, so there can never be more rows than fish.
  get canAddBulkWeight(): boolean {
    const count = Number(this.bulkForm?.value.count);
    return count >= 1 && this.bulkWeights.length < count;
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
    this.syncDeletedStubs(null);

    this.editSessionItem = {
      startDateTime: toLocalDateTimeInput(new Date()),
      endDateTime: toLocalDateTimeInput(new Date()),
      venueId: null,
      notes: '',
      targetSpeciesIds: [],
      catches: [],
    };

    this.buildSessionForm(this.editSessionItem);
    this.view = 'sessionForm';
  }

  async editSession(id) {
    this.sessionItem = await lastValueFrom(this.sessionService.get(id));
    this.syncDeletedStubs(this.sessionItem);

    this.editSessionItem = {
      startDateTime: toLocalDateTimeInput(new Date(this.sessionItem.startDateTime)),
      endDateTime: toLocalDateTimeInput(new Date(this.sessionItem.endDateTime)),
      venueId: this.sessionItem.venueId,
      notes: this.sessionItem.notes ?? '',
      targetSpeciesIds: [...(this.sessionItem.targetSpeciesIds ?? [])],
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
      targetSpeciesIds: [sessionItem.targetSpeciesIds ?? []],
    });
    this.refreshTargetSpeciesOptions();
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
      targetSpeciesIds: formValue.targetSpeciesIds ?? [],
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

  // bulk entry - fans out into N individual catch rows, no quantity stored. Weights are
  // optional and entered one row per weighed fish: row 1 goes on the first catch created,
  // row 2 on the second, and so on - any remaining catches are saved unweighed.

  addBulkCatch() {
    this.bulkForm = this.fb.group({
      speciesId: [null, Validators.required],
      baitId: [null],
      methodId: [null],
      rigId: [null],
      count: [1, [Validators.required, Validators.min(1), Validators.max(200)]],
      weightUnit: [WeightUnit.LbOz],
      weights: this.fb.array([]),
    }, { validators: weightsWithinCount });
    this.modalService.open(this.bulkModalTpl, { size: 'md' });
  }

  addBulkWeight() {
    if (!this.canAddBulkWeight) {
      return;
    }
    this.bulkWeights.push(this.fb.group({
      lbs: [null],
      oz: [null, [Validators.min(0), Validators.max(15)]],
      value: [null],
    }));
  }

  removeBulkWeight(index: number) {
    this.bulkWeights.removeAt(index);
  }

  saveBulkCatch(modal: NgbActiveModal) {
    if (this.bulkForm.invalid) {
      return;
    }

    const formValue = this.bulkForm.value;
    const weightUnit = formValue.weightUnit ?? WeightUnit.LbOz;
    const blankToNull = (v: any) => (v === '' || v == null ? null : v);

    for (let i = 0; i < formValue.count; i++) {
      const row = formValue.weights[i];
      // Only the fields the chosen unit actually uses are sent, so switching units
      // mid-entry can't leave a stale lb/oz or kg/g value riding along on the DTO.
      const weightInput = row
        ? weightUnit === WeightUnit.LbOz
          ? { weightLbs: blankToNull(row.lbs), weightOz: blankToNull(row.oz), weightValue: null }
          : { weightLbs: null, weightOz: null, weightValue: blankToNull(row.value) }
        : { weightLbs: null, weightOz: null, weightValue: null };

      const catchEntry: CreateUpdateCatchDto = {
        sessionId: this.sessionItem?.id ?? 0,
        speciesId: formValue.speciesId,
        baitId: formValue.baitId || null,
        methodId: formValue.methodId || null,
        rigId: formValue.rigId || null,
        lengthUnit: SizeUnit.Centimetres,
        weightUnit,
        ...weightInput,
      };

      // weightG is display-only and computed server-side on save - set here too (as
      // saveCatch does) so the session-edit table shows the weight immediately instead
      // of a blank until the session round-trips.
      this.editSessionItem.catches.push({ ...catchEntry, weightG: weightInputToGrams(catchEntry) } as any);
    }

    modal.close();
  }

  // venue quick-add - lets the user create a venue without losing session form progress

  addVenueQuickAdd() {
    this.modalService.open(this.venueModalTpl, { size: 'md' });
  }

  // Reloaded rather than just appending the new venue: if it shares a name with an
  // existing venue (on a different ticket), that venue's display label has just changed
  // too, to include its ticket.
  async onVenueQuickAddSaved(newVenue: VenueDto, modal: NgbActiveModal) {
    const venues = await lastValueFrom(this.venueService.getList({ maxResultCount: 1000 }));
    this.venueList = venues.items.sort((a, b) => a.displayName.localeCompare(b.displayName));
    this.sessionForm.patchValue({ venueId: newVenue.id });
    this.refreshTargetSpeciesOptions();

    modal.close();
  }

  // bait/method/rig quick-add - same pattern as venue quick-add, but patch into the
  // catch form (these are fields on a catch, not the session itself). The bulk-add modal
  // offers the same buttons, so the form that opened the quick-add is remembered and the
  // new item is selected there, rather than always in the single-catch form.

  private quickAddTarget: FormGroup;

  addBaitQuickAdd(target?: FormGroup) {
    this.quickAddTarget = target ?? this.catchForm;
    this.modalService.open(this.baitModalTpl, { size: 'md' });
  }

  onBaitQuickAddSaved(newBait: BaitDto, modal: NgbActiveModal) {
    this.baitList = [...this.baitList, newBait].sort((a, b) => a.name.localeCompare(b.name));
    this.quickAddTarget.patchValue({ baitId: newBait.id });

    modal.close();
  }

  addMethodQuickAdd(target?: FormGroup) {
    this.quickAddTarget = target ?? this.catchForm;
    this.modalService.open(this.methodModalTpl, { size: 'md' });
  }

  onMethodQuickAddSaved(newMethod: MethodDto, modal: NgbActiveModal) {
    this.methodList = [...this.methodList, newMethod].sort((a, b) => a.name.localeCompare(b.name));
    this.quickAddTarget.patchValue({ methodId: newMethod.id });

    modal.close();
  }

  addRigQuickAdd(target?: FormGroup) {
    this.quickAddTarget = target ?? this.catchForm;
    this.modalService.open(this.rigModalTpl, { size: 'md' });
  }

  onRigQuickAddSaved(newRig: RigDto, modal: NgbActiveModal) {
    const withLabel = { ...newRig, displayLabel: rigDisplayLabel(newRig) };
    this.rigList = [...this.rigList, withLabel].sort((a, b) => a.name.localeCompare(b.name));
    this.quickAddTarget.patchValue({ rigId: newRig.id });

    modal.close();
  }
}

function isDeletedStub(item: unknown): boolean {
  return !!(item as { isDeletedStub?: boolean })?.isDeletedStub;
}

function weightsWithinCount(group: AbstractControl): ValidationErrors | null {
  const count = Number(group.get('count')?.value);
  const weights = group.get('weights') as FormArray;
  return weights && count >= 1 && weights.length > count ? { tooManyWeights: true } : null;
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
  targetSpeciesIds: number[];
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
