import { inclusiveDayCount, isFutureBookingStart } from './dates';

export function calculateInclusiveTotal(dailyRate: number, startDate: string, endDate: string): number {
  const days = inclusiveDayCount(startDate, endDate);
  if (days <= 0) {
    return 0;
  }

  return dailyRate * days;
}

export function canDeleteBooking(startDate: string): boolean {
  return isFutureBookingStart(startDate);
}
