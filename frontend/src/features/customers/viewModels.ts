import type { Customer } from '../../shared/api/types';

export type CustomerOptionViewModel = {
  id: string;
  label: string;
};

export function toCustomerOption(customer: Customer): CustomerOptionViewModel {
  return {
    id: customer.id,
    label: `${customer.firstName} ${customer.lastName}`,
  };
}
