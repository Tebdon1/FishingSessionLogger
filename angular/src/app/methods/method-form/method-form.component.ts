import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, SimpleChanges, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { lastValueFrom } from 'rxjs';
import { MethodService, MethodDto } from '@proxy/methods';

// Single source of truth for the method create/edit fields - used by both the
// Methods page's own form and the session form's quick-add-method modal, same
// reasoning as VenueFormComponent.
@Component({
  selector: 'app-method-form',
  templateUrl: './method-form.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
})
export class MethodFormComponent implements OnChanges {
  @Input() method: MethodDto | null = null;
  @Output() saved = new EventEmitter<MethodDto>();
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;

  constructor(
    private methodService: MethodService,
    private fb: FormBuilder) {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.method) {
      this.buildForm();
    }
  }

  private buildForm() {
    this.form = this.fb.group({
      name: [this.method?.name || '', [Validators.required, Validators.maxLength(255)]],
    });
  }

  async save() {
    if (this.form.invalid) {
      return;
    }

    const formValue = this.form.value;
    const result = this.method
      ? await lastValueFrom(this.methodService.update(this.method.id, formValue))
      : await lastValueFrom(this.methodService.create(formValue));

    this.saved.emit(result);
  }

  cancel() {
    this.cancelled.emit();
  }
}
