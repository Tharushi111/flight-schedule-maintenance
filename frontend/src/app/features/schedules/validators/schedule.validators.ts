import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';

export const differentAirportsValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const originAirportId =
    control.get('originAirportId')?.value;

  const destinationAirportId =
    control.get('destinationAirportId')?.value;

  if (
    originAirportId == null ||
    destinationAirportId == null
  ) {
    return null;
  }

  return originAirportId === destinationAirportId
    ? { sameAirport: true }
    : null;
};


export const effectiveDateValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const effectiveFrom =
    control.get('effectiveFrom')?.value;

  const effectiveTo =
    control.get('effectiveTo')?.value;

  if (!effectiveFrom || !effectiveTo) {
    return null;
  }

  return effectiveTo < effectiveFrom
    ? { invalidEffectivePeriod: true }
    : null;
};