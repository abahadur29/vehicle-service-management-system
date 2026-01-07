import { Injectable } from '@angular/core';
import { NativeDateAdapter } from '@angular/material/core';

@Injectable()
export class CustomDateAdapter extends NativeDateAdapter {
  override format(date: Date, displayFormat: any): string {
    if (displayFormat === 'DD/MM/YYYY' || displayFormat === 'input') {
      const day = date.getDate().toString().padStart(2, '0');
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    }
    return super.format(date, displayFormat);
  }

  override parse(value: any): Date | null {
    if (typeof value === 'string' && value.indexOf('/') > -1) {
      const str = value.split('/');
      if (str.length === 3) {
        const day = parseInt(str[0], 10);
        const month = parseInt(str[1], 10) - 1;
        const year = parseInt(str[2], 10);
        const date = new Date(year, month, day);
        if (this.isValidDate(date)) {
          return date;
        }
      }
    }
    return super.parse(value);
  }

  private isValidDate(date: Date): boolean {
    return !isNaN(date.getTime());
  }
}

