import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'monthYearFormat',
  standalone: true
})
export class MonthYearFormatPipe implements PipeTransform {
  private monthNames = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
  ];

  transform(value: { month: number; year: number } | null | undefined): string {
    if (!value || !value.month || !value.year) {
      return 'N/A';
    }

    const month = typeof value.month === 'number' ? value.month : parseInt(value.month, 10);
    const year = typeof value.year === 'number' ? value.year : parseInt(value.year, 10);

    if (isNaN(month) || isNaN(year) || month < 1 || month > 12) {
      return `${value.month} ${value.year}`;
    }

    return `${this.monthNames[month - 1]} ${year}`;
  }
}

