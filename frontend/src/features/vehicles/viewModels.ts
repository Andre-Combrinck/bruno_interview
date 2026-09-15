import type { Vehicle } from '../../shared/api/types';
import { formatMoney } from '../../shared/utils/money';

export type VehicleOptionViewModel = {
  id: string;
  label: string;
  dailyRate: number;
  isDeleted: boolean;
};

export function toVehicleOption(vehicle: Vehicle): VehicleOptionViewModel {
  return {
    id: vehicle.id,
    label: `${vehicle.registrationNumber} · ${vehicle.make} ${vehicle.model}`,
    dailyRate: vehicle.dailyRate,
    isDeleted: vehicle.isDeleted,
  };
}

export function toVehicleSelectLabel(vehicle: Vehicle): string {
  return `${vehicle.registrationNumber} · ${vehicle.make} ${vehicle.model} (${formatMoney(vehicle.dailyRate)}/day)`;
}
