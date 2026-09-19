import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/api/client';
import type { Booking, BookingStatus, PagedResult } from '../../shared/api/types';

export type BookingInput = {
  vehicleId: string;
  customerId: string;
  startDate: string;
  endDate: string;
};

export type BookingFilters = {
  vehicleId?: string;
  customerId?: string;
  status?: BookingStatus | '';
  from?: string;
  to?: string;
  page: number;
};

export function useBookings(filters: BookingFilters) {
  return useQuery({
    queryKey: ['bookings', filters],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Booking>>('/api/bookings', {
        params: {
          vehicleId: filters.vehicleId || undefined,
          customerId: filters.customerId || undefined,
          status: filters.status || undefined,
          from: filters.from || undefined,
          to: filters.to || undefined,
          page: filters.page,
          pageSize: 20,
        },
      });
      return data;
    },
  });
}

export function useCreateBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (input: BookingInput) => {
      const { data } = await apiClient.post<Booking>('/api/bookings', input);
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
  });
}

export function useChangeBookingStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: BookingStatus }) => {
      const { data } = await apiClient.patch<Booking>(`/api/bookings/${id}/status`, { status });
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
}

export function useDeleteBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/bookings/${id}`);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
  });
}
