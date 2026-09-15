import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/api/client';
import type { Customer, PagedResult } from '../../shared/api/types';

export type CustomerInput = {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
};

export function useCustomers(search: string, page: number, pageSize = 20) {
  return useQuery({
    queryKey: ['customers', search, page, pageSize],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Customer>>('/api/customers', {
        params: { search: search || undefined, page, pageSize },
      });
      return data;
    },
  });
}

export function useAllCustomers() {
  return useQuery({
    queryKey: ['customers', 'all'],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Customer>>('/api/customers', {
        params: { page: 1, pageSize: 100 },
      });
      return data.items;
    },
  });
}

export function useCreateCustomer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (input: CustomerInput) => {
      const { data } = await apiClient.post<Customer>('/api/customers', input);
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
}

export function useUpdateCustomer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, input }: { id: string; input: CustomerInput }) => {
      const { data } = await apiClient.put<Customer>(`/api/customers/${id}`, input);
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
}

export function useDeleteCustomer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/customers/${id}`);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['customers'] });
      await queryClient.invalidateQueries({ queryKey: ['bookings'] });
    },
  });
}
