import { AbstractControl, Validators } from '@angular/forms';

export function makeControlOptional(control: AbstractControl) {
  control.setValidators([]);
  control.setValue(null);
  resetControl(control);
}

export function makeControlRequired(control: AbstractControl) {
  control.setValidators([Validators.required]);
  resetControl(control);
}

export function resetControl(control: AbstractControl) {
  control.markAsPristine();
  control.markAsUntouched();
  control.updateValueAndValidity();
}
