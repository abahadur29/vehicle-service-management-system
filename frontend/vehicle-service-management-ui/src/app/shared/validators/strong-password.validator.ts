import { AbstractControl, ValidationErrors } from '@angular/forms';

export function strongPasswordValidator(control: AbstractControl): ValidationErrors | null {
  const value = String(control.value ?? '');

  const hasUpper = /[A-Z]/.test(value);
  const hasLower = /[a-z]/.test(value);
  const hasNumber = /[0-9]/.test(value);
  const hasSpecial = /[!@#$%^&*()_\-+=\[\]{};:'",.<>/?\\|`~]/.test(value);
  const minLength = value.length >= 8;

  const ok = hasUpper && hasLower && hasNumber && hasSpecial && minLength;

  return ok
    ? null
    : {
        strongPassword: {
          hasUpper,
          hasLower,
          hasNumber,
          hasSpecial,
          minLength,
        },
      };
}