import { zodResolver } from '@hookform/resolvers/zod';
import { useMemo, useState } from 'react';
import { useForm, useWatch } from 'react-hook-form';
import { z } from 'zod';
import { getErrorMessage } from '../../shared/api/client';
import type { Booking, BookingStatus } from '../../shared/api/types';
import { calculateInclusiveTotal } from '../../shared/utils/bookingRules';
import { formatMoney } from '../../shared/utils/money';
import { ConfirmDialog, LoadingSkeleton, Modal } from '../../shared/components/ui';
import { useToast } from '../../shared/hooks/useToast';
import { useAllCustomers } from '../customers/api';
import { toCustomerOption } from '../customers/viewModels';
import { useAllVehicles } from '../vehicles/api';
import { toVehicleSelectLabel } from '../vehicles/viewModels';
import {
  useBookings,
  useChangeBookingStatus,
  useCreateBooking,
  useDeleteBooking,
  type BookingInput,
} from './api';
import { toBookingRow, toLookupMap } from './viewModels';

const schema = z
  .object({
    vehicleId: z.string().uuid(),
    customerId: z.string().uuid(),
    startDate: z.string().min(1),
    endDate: z.string().min(1),
  })
  .refine((value) => value.endDate > value.startDate, {
    message: 'End date must be after start date',
    path: ['endDate'],
  });

type FormValues = z.infer<typeof schema>;

export function BookingsPage() {
  const { showError, showSuccess } = useToast();
  const [vehicleId, setVehicleId] = useState('');
  const [customerId, setCustomerId] = useState('');
  const [status, setStatus] = useState<BookingStatus | ''>('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [page, setPage] = useState(1);
  const [creating, setCreating] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<Booking | null>(null);

  const bookingsQuery = useBookings({ vehicleId, customerId, status, from, to, page });
  const vehiclesQuery = useAllVehicles(false);
  const customersQuery = useAllCustomers();
  const createMutation = useCreateBooking();
  const statusMutation = useChangeBookingStatus();
  const deleteMutation = useDeleteBooking();

  const vehicleMap = useMemo(
    () =>
      toLookupMap(vehiclesQuery.data ?? [], (vehicle) =>
        `${vehicle.registrationNumber} · ${vehicle.make} ${vehicle.model}`,
      ),
    [vehiclesQuery.data],
  );

  const customerMap = useMemo(
    () =>
      toLookupMap(customersQuery.data ?? [], (customer) => `${customer.firstName} ${customer.lastName}`),
    [customersQuery.data],
  );

  const bookingRows = useMemo(
    () => (bookingsQuery.data?.items ?? []).map((booking) => toBookingRow(booking, vehicleMap, customerMap)),
    [bookingsQuery.data, vehicleMap, customerMap],
  );

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      vehicleId: '',
      customerId: '',
      startDate: '',
      endDate: '',
    },
  });

  const watchedVehicleId = useWatch({ control: form.control, name: 'vehicleId' });
  const watchedStartDate = useWatch({ control: form.control, name: 'startDate' });
  const watchedEndDate = useWatch({ control: form.control, name: 'endDate' });

  const pricePreview = useMemo(() => {
    const vehicle = vehiclesQuery.data?.find((item) => item.id === watchedVehicleId);
    if (!vehicle || !watchedStartDate || !watchedEndDate) {
      return null;
    }

    const total = calculateInclusiveTotal(vehicle.dailyRate, watchedStartDate, watchedEndDate);
    if (total <= 0) {
      return null;
    }

    return formatMoney(total);
  }, [vehiclesQuery.data, watchedVehicleId, watchedStartDate, watchedEndDate]);

  const resetFilters = () => {
    setVehicleId('');
    setCustomerId('');
    setStatus('');
    setFrom('');
    setTo('');
    setPage(1);
  };

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      const input: BookingInput = values;
      await createMutation.mutateAsync(input);
      showSuccess('Booking created');
      setCreating(false);
      form.reset();
    } catch (error) {
      showError(getErrorMessage(error));
    }
  });

  const changeStatus = async (booking: Booking, next: BookingStatus) => {
    try {
      await statusMutation.mutateAsync({ id: booking.id, status: next });
      showSuccess(`Booking marked ${next}`);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  };

  const confirmDelete = async () => {
    if (!pendingDelete) {
      return;
    }
    try {
      await deleteMutation.mutateAsync(pendingDelete.id);
      showSuccess('Booking deleted');
      setPendingDelete(null);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  };

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <h1>Bookings</h1>
          <p>Inclusive pricing, no same-day handoff, explicit status changes.</p>
        </div>
        <button type="button" className="primary-btn" onClick={() => setCreating(true)}>
          New booking
        </button>
      </header>

      <div className="toolbar">
        <select
          value={vehicleId}
          onChange={(event) => {
            setVehicleId(event.target.value);
            setPage(1);
          }}
        >
          <option value="">All vehicles</option>
          {(vehiclesQuery.data ?? []).map((vehicle) => (
            <option key={vehicle.id} value={vehicle.id}>
              {vehicle.registrationNumber} · {vehicle.make} {vehicle.model}
            </option>
          ))}
        </select>
        <select
          value={customerId}
          onChange={(event) => {
            setCustomerId(event.target.value);
            setPage(1);
          }}
        >
          <option value="">All customers</option>
          {(customersQuery.data ?? []).map((customer) => (
            <option key={customer.id} value={customer.id}>
              {customer.firstName} {customer.lastName}
            </option>
          ))}
        </select>
        <select
          value={status}
          onChange={(event) => {
            setStatus(event.target.value as BookingStatus | '');
            setPage(1);
          }}
        >
          <option value="">All statuses</option>
          <option value="Active">Active</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
        <label>
          From
          <input
            type="date"
            value={from}
            onChange={(event) => {
              setFrom(event.target.value);
              setPage(1);
            }}
          />
        </label>
        <label>
          To
          <input
            type="date"
            value={to}
            onChange={(event) => {
              setTo(event.target.value);
              setPage(1);
            }}
          />
        </label>
        <button type="button" className="ghost-btn" onClick={resetFilters}>
          Clear filters
        </button>
      </div>

      {bookingsQuery.isLoading ? <LoadingSkeleton /> : null}
      {bookingsQuery.isError ? <p className="error-text">{getErrorMessage(bookingsQuery.error)}</p> : null}

      {bookingsQuery.data ? (
        <>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Vehicle</th>
                  <th>Customer</th>
                  <th>Dates</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {bookingRows.map((row) => {
                  const booking = bookingsQuery.data!.items.find((item) => item.id === row.id)!;
                  return (
                  <tr key={row.id}>
                    <td>{row.vehicleLabel}</td>
                    <td>{row.customerLabel}</td>
                    <td>{row.dateRangeLabel}</td>
                    <td>{row.totalLabel}</td>
                    <td>{row.status}</td>
                    <td className="row-actions">
                      {row.canComplete ? (
                        <button
                          type="button"
                          className="ghost-btn"
                          onClick={() => changeStatus(booking, 'Completed')}
                        >
                          Complete
                        </button>
                      ) : null}
                      {row.canCancel ? (
                        <button
                          type="button"
                          className="ghost-btn"
                          onClick={() => changeStatus(booking, 'Cancelled')}
                        >
                          Cancel
                        </button>
                      ) : null}
                      {row.canDelete ? (
                        <button
                          type="button"
                          className="danger-btn"
                          onClick={() => setPendingDelete(booking)}
                        >
                          Delete
                        </button>
                      ) : null}
                    </td>
                  </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <div className="pager">
            <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Previous
            </button>
            <span>
              Page {bookingsQuery.data.page} / {Math.max(bookingsQuery.data.totalPages, 1)}
            </span>
            <button
              type="button"
              disabled={page >= bookingsQuery.data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      ) : null}

      <Modal title="New booking" open={creating} onClose={() => setCreating(false)}>
        <form className="stack-form" onSubmit={onSubmit}>
          <label>
            Vehicle
            <select {...form.register('vehicleId')}>
              <option value="">Select vehicle</option>
              {(vehiclesQuery.data ?? [])
                .filter((vehicle) => !vehicle.isDeleted)
                .map((vehicle) => (
                  <option key={vehicle.id} value={vehicle.id}>
                    {toVehicleSelectLabel(vehicle)}
                  </option>
                ))}
            </select>
          </label>
          <label>
            Customer
            <select {...form.register('customerId')}>
              <option value="">Select customer</option>
              {(customersQuery.data ?? []).map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {toCustomerOption(customer).label}
                </option>
              ))}
            </select>
          </label>
          <label>
            Start date
            <input type="date" {...form.register('startDate')} />
          </label>
          <label>
            End date
            <input type="date" {...form.register('endDate')} />
          </label>
          {pricePreview ? <p className="hint-text">Estimated total: {pricePreview}</p> : null}
          {form.formState.errors.endDate ? (
            <p className="error-text">{form.formState.errors.endDate.message}</p>
          ) : null}
          <div className="form-actions">
            <button type="button" className="ghost-btn" onClick={() => setCreating(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-btn">
              Create booking
            </button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={!!pendingDelete}
        title="Delete booking"
        message="Only future bookings (StartDate after today UTC) can be deleted."
        confirmLabel="Delete"
        onCancel={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
      />
    </section>
  );
}
