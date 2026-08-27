import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, SimpleChanges, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';
import { RigService, RigDto, HookWeightUnit } from '@proxy/rigs';
import { SizeUnit } from '@proxy/sessions';
import { lengthMmToInput, hookWeightGToInput } from '../../shared/unit-display';

// Single source of truth for the rig create/edit fields - used by both the Rigs
// page's own form and the session form's quick-add-rig modal, same reasoning as
// VenueFormComponent.
@Component({
  selector: 'app-rig-form',
  templateUrl: './rig-form.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class RigFormComponent implements OnChanges {
  @Input() rig: RigDto | null = null;
  @Output() saved = new EventEmitter<RigDto>();
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;
  SizeUnit = SizeUnit;
  HookWeightUnit = HookWeightUnit;

  constructor(
    private rigService: RigService,
    private fb: FormBuilder) {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.rig) {
      this.buildForm();
    }
  }

  private buildForm() {
    const r = this.rig as any;
    // Re-editing starts from the unit it was originally entered in, so the form
    // shows the value the way it was entered rather than a reformatted one. Rows
    // with no stored unit (created before LengthUnit/HookWeightUnit existed) fall
    // back to the same defaults a brand new rig uses.
    const lengthInput = r ? lengthMmToInput(r.lengthMm, r.lengthUnit ?? SizeUnit.Inches) : null;
    const hookWeightInput = r ? hookWeightGToInput(r.hookWeightG, r.hookWeightUnit ?? HookWeightUnit.Grams) : null;

    this.form = this.fb.group({
      name: [r?.name || '', [Validators.required, Validators.maxLength(255)]],
      lengthValue: [lengthInput?.lengthValue ?? null],
      lengthUnit: [lengthInput?.lengthUnit ?? SizeUnit.Inches],
      hookSize: [r?.hookSize || '', Validators.maxLength(50)],
      hookWeightValue: [hookWeightInput?.hookWeightValue ?? null],
      hookWeightUnit: [hookWeightInput?.hookWeightUnit ?? HookWeightUnit.Grams],
      hookPattern: [r?.hookPattern || ''],
      materials: [r?.materials || ''],
      notes: [r?.notes || ''],
    });
  }

  async save() {
    if (this.form.invalid) {
      return;
    }

    const formValue = this.form.value;
    const result = this.rig
      ? await lastValueFrom(this.rigService.update(this.rig.id, formValue))
      : await lastValueFrom(this.rigService.create(formValue));

    this.saved.emit(result);
  }

  cancel() {
    this.cancelled.emit();
  }
}
