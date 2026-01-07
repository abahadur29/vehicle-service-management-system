import { MAT_DATE_FORMATS } from '@angular/material/core';

export const DD_MM_YYYY_FORMAT = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

export const DATE_FORMAT_PROVIDERS = [
  { provide: MAT_DATE_FORMATS, useValue: DD_MM_YYYY_FORMAT }
];

