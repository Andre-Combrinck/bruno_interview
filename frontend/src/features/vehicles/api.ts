import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../shared/api/client';
import type { PagedResult, Vehicle } from '../../shared/api/types';

export type VehicleInput = {
  registrationNumber: string;
  make: string;
  model: string;
  year: number;
  dailyRate: number;
};

export function useVehicles(search: string, includeDeleted: boolean, page: number, pageSize = 20) {
  return useQuery({
    queryKey: ['vehicles', search, includeDeleted, page, pageSize],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Vehicle>>('/api/vehicles', {
        params: { search: search || undefined, includeDeleted, page, pageSize },
      });
      return data;
    },
  });
}

export function useAllVehicles(includeDeleted = false) {
  return useQuery({
    queryKey: ['vehicles', 'all', includeDeleted],
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<Vehicle>>('/api/vehicles', {
        params: { includeDeleted, page: 1, pageSize: 100 },
      });
      return data.items;
    },
  });
}

export function useCreateVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (input: VehicleInput) => {
      const { data } = await apiClient.post<Vehicle>('/api/vehicles', input);
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    },
  });
}

export function useUpdateVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, input }: { id: string; input: VehicleInput }) => {
      const { data } = await apiClient.put<Vehicle>(`/api/vehicles/${id}`, input);
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    },
  });
}

export function useDeleteVehicle() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/vehicles/${id}`);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    },
  });
}
