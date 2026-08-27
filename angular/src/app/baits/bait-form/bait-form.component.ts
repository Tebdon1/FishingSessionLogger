import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, SimpleChanges, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';
import { BaitService } from '@proxy/baits';
import { BaitDto, BaitType, SizeUnit } from '@proxy/sessions';
import { lengthMmToInput } from '../../shared/unit-display';

// Single source of truth for the bait create/edit fields - used by both the Baits
// page's own form and the session form's quick-add-bait modal, same reasoning as
// VenueFormComponent.
@Component({
  selector: 'app-bait-form',
  templateUrl: './bait-form.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class BaitFormComponent implements OnChanges {
  @Input() bait: BaitDto | null = null;
  @Output() saved = new EventEmitter<BaitDto>();
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;
  BaitType = BaitType;
  SizeUnit = SizeUnit;

  constructor(
    private baitService: BaitService,
    private fb: FormBuilder) {
    // Covers usage with no [bait] binding at all (e.g. the always-create quick-add
    // modal), where ngOnChanges below never fires since there's no bound input.
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.bait) {
      this.buildForm();
    }
  }

  get isNatural(): boolean {
    return this.form?.value.baitType == BaitType.Natural;
  }

  get isLure(): boolean {
    return this.form?.value.baitType == BaitType.Lure;
  }

  get isManufacturedBait(): boolean {
    return this.form?.value.baitType == BaitType.Bait;
  }

  // Name/Brand aren't Validators.required on the form itself, since which one is
  // required depends on baitType - so validity has to be checked here instead.
  get invalid(): boolean {
    if (!this.form || this.form.invalid) {
      return true;
    }
    const v = this.form.value;
    return v.baitType == BaitType.Natural ? !v.name?.trim() : !v.brand?.trim();
  }

  private buildForm() {
    const b = this.bait as any;
    // Re-editing starts from the unit it was originally entered in, so the form
    // shows the value the way it was entered rather than a reformatted one. Rows
    // with no stored unit (created before SizeUnit existed) fall back to mm.
    const sizeInput = b ? lengthMmToInput(b.sizeMm, b.sizeUnit ?? SizeUnit.Millimetres) : null;

    this.form = this.fb.group({
      baitType: [b?.baitType ?? BaitType.Lure, Validators.required],
      name: [b?.name || ''],
      brand: [b?.brand || ''],
      range: [b?.range || ''],
      colour: [b?.colour || ''],
      flavour: [b?.flavour || ''],
      sizeValue: [sizeInput?.lengthValue ?? null],
      sizeUnit: [sizeInput?.lengthUnit ?? SizeUnit.Millimetres],
    });
  }

  async save() {
    if (this.invalid) {
      return;
    }

    const formValue = this.form.value;
    const result = this.bait
      ? await lastValueFrom(this.baitService.update(this.bait.id, formValue))
      : await lastValueFrom(this.baitService.create(formValue));

    this.saved.emit(result);
  }

  cancel() {
    this.cancelled.emit();
  }
}
