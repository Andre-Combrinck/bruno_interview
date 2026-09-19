export function inclusiveDayCount(startDate: string, endDate: string): number {
  if (!startDate || !endDate || endDate <= startDate) {
    return 0;
  }

  const start = new Date(`${startDate}T00:00:00Z`);
  const end = new Date(`${endDate}T00:00:00Z`);
  const diffMs = end.getTime() - start.getTime();
  return Math.floor(diffMs / (1000 * 60 * 60 * 24)) + 1;
}

export function utcTodayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

export function isFutureBookingStart(startDate: string, todayUtc = utcTodayIso()): boolean {
  return startDate > todayUtc;
}

export function formatDateRange(startDate: string, endDate: string): string {
  return `${startDate} → ${endDate}`;
}
