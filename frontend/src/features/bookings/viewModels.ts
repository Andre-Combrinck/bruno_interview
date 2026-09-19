import type { Booking, BookingStatus } from '../../shared/api/types';
import { canDeleteBooking } from '../../shared/utils/bookingRules';
import { formatDateRange } from '../../shared/utils/dates';
import { formatMoney } from '../../shared/utils/money';

export type BookingRowViewModel = {
  id: string;
  vehicleLabel: string;
  customerLabel: string;
  dateRangeLabel: string;
  totalLabel: string;
  status: BookingStatus;
  canComplete: boolean;
  canCancel: boolean;
  canDelete: boolean;
};

export function toBookingRow(
  booking: Booking,
  vehicleMap: Map<string, string>,
  customerMap: Map<string, string>,
): BookingRowViewModel {
  return {
    id: booking.id,
    vehicleLabel: vehicleMap.get(booking.vehicleId) ?? booking.vehicleId.slice(0, 8),
    customerLabel: customerMap.get(booking.customerId) ?? booking.customerId.slice(0, 8),
    dateRangeLabel: formatDateRange(booking.startDate, booking.endDate),
    totalLabel: formatMoney(booking.totalPrice),
    status: booking.status,
    canComplete: booking.status === 'Active',
    canCancel: booking.status === 'Active',
    canDelete: canDeleteBooking(booking.startDate),
  };
}

export function toLookupMap<T extends { id: string }>(
  items: T[],
  labelFn: (item: T) => string,
): Map<string, string> {
  const map = new Map<string, string>();
  items.forEach((item) => map.set(item.id, labelFn(item)));
  return map;
}
