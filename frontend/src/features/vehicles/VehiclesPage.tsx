import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { getErrorMessage } from '../../shared/api/client';
import type { Vehicle } from '../../shared/api/types';
import { ConfirmDialog, LoadingSkeleton, Modal } from '../../shared/components/ui';
import { useToast } from '../../shared/hooks/useToast';
import { useCreateVehicle, useDeleteVehicle, useRestoreVehicle, useUpdateVehicle, useVehicles, type VehicleInput } from './api';
import {
  isValidSouthAfricanRegistration,
  REGISTRATION_INVALID_MESSAGE,
} from './registrationNumber';

const schema = z.object({
  registrationNumber: z
    .string()
    .min(1, 'Required')
    .refine(isValidSouthAfricanRegistration, REGISTRATION_INVALID_MESSAGE),
  make: z.string().min(1, 'Required'),
  model: z.string().min(1, 'Required'),
  year: z.number().int().min(1980),
  dailyRate: z.number().positive(),
});

type FormValues = z.infer<typeof schema>;

export function VehiclesPage() {
  const { showError, showSuccess } = useToast();
  const [search, setSearch] = useState('');
  const [includeDeleted, setIncludeDeleted] = useState(false);
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<Vehicle | null>(null);
  const [creating, setCreating] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<Vehicle | null>(null);
  const [pendingRestore, setPendingRestore] = useState<Vehicle | null>(null);

  const query = useVehicles(search, includeDeleted, page);
  const createMutation = useCreateVehicle();
  const updateMutation = useUpdateVehicle();
  const deleteMutation = useDeleteVehicle();
  const restoreMutation = useRestoreVehicle();

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      registrationNumber: '',
      make: '',
      model: '',
      year: new Date().getFullYear(),
      dailyRate: 400,
    },
  });

  const openCreate = () => {
    form.reset({
      registrationNumber: '',
      make: '',
      model: '',
      year: new Date().getFullYear(),
      dailyRate: 400,
    });
    setEditing(null);
    setCreating(true);
  };

  const openEdit = (vehicle: Vehicle) => {
    form.reset({
      registrationNumber: vehicle.registrationNumber,
      make: vehicle.make,
      model: vehicle.model,
      year: vehicle.year,
      dailyRate: vehicle.dailyRate,
    });
    setEditing(vehicle);
    setCreating(true);
  };

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      const input: VehicleInput = values;
      if (editing) {
        await updateMutation.mutateAsync({ id: editing.id, input });
        showSuccess('Vehicle updated');
      } else {
        await createMutation.mutateAsync(input);
        showSuccess('Vehicle created');
      }
      setCreating(false);
      setEditing(null);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  });

  const confirmDelete = async () => {
    if (!pendingDelete) {
      return;
    }
    try {
      await deleteMutation.mutateAsync(pendingDelete.id);
      showSuccess('Vehicle soft-deleted');
      setPendingDelete(null);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  };

  const confirmRestore = async () => {
    if (!pendingRestore) {
      return;
    }
    try {
      await restoreMutation.mutateAsync(pendingRestore.id);
      showSuccess('Vehicle restored');
      setPendingRestore(null);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  };

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <h1>Vehicles</h1>
          <p>Fleet inventory with soft-delete support.</p>
        </div>
        <button type="button" className="primary-btn" onClick={openCreate}>
          Add vehicle
        </button>
      </header>

      <div className="toolbar">
        <input
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
          placeholder="Search registration, make, model"
        />
        <label className="toggle">
          <input
            type="checkbox"
            checked={includeDeleted}
            onChange={(event) => {
              setIncludeDeleted(event.target.checked);
              setPage(1);
            }}
          />
          Show deleted
        </label>
      </div>

      {query.isLoading ? <LoadingSkeleton /> : null}
      {query.isError ? <p className="error-text">{getErrorMessage(query.error)}</p> : null}

      {query.data ? (
        <>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Registration</th>
                  <th>Make / Model</th>
                  <th>Year</th>
                  <th>Daily rate</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((vehicle) => (
                  <tr key={vehicle.id} className={vehicle.isDeleted ? 'row-muted' : undefined}>
                    <td>{vehicle.registrationNumber}</td>
                    <td>
                      {vehicle.make} {vehicle.model}
                    </td>
                    <td>{vehicle.year}</td>
                    <td>R {vehicle.dailyRate.toFixed(2)}</td>
                    <td>{vehicle.isDeleted ? 'Deleted' : 'Active'}</td>
                    <td className="row-actions">
                      {!vehicle.isDeleted ? (
                        <>
                          <button type="button" className="ghost-btn" onClick={() => openEdit(vehicle)}>
                            Edit
                          </button>
                          <button type="button" className="danger-btn" onClick={() => setPendingDelete(vehicle)}>
                            Delete
                          </button>
                        </>
                      ) : (
                        <button type="button" className="ghost-btn" onClick={() => setPendingRestore(vehicle)}>
                          Restore
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="pager">
            <button type="button" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Previous
            </button>
            <span>
              Page {query.data.page} / {Math.max(query.data.totalPages, 1)}
            </span>
            <button
              type="button"
              disabled={page >= query.data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      ) : null}

      <Modal
        title={editing ? 'Edit vehicle' : 'Add vehicle'}
        open={creating}
        onClose={() => {
          setCreating(false);
          setEditing(null);
        }}
      >
        <form className="stack-form" onSubmit={onSubmit}>
          <label>
            Registration
            <input {...form.register('registrationNumber')} placeholder="CA123456 or AB12CD GP" />
            {form.formState.errors.registrationNumber ? (
              <p className="error-text">{form.formState.errors.registrationNumber.message}</p>
            ) : null}
          </label>
          <label>
            Make
            <input {...form.register('make')} />
          </label>
          <label>
            Model
            <input {...form.register('model')} />
          </label>
          <label>
            Year
            <input type="number" {...form.register('year', { valueAsNumber: true })} />
          </label>
          <label>
            Daily rate
            <input type="number" step="0.01" {...form.register('dailyRate', { valueAsNumber: true })} />
          </label>
          <div className="form-actions">
            <button type="button" className="ghost-btn" onClick={() => setCreating(false)}>
              Cancel
            </button>
            <button type="submit" className="primary-btn">
              Save
            </button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={!!pendingDelete}
        title="Soft-delete vehicle"
        message={`Soft-delete ${pendingDelete?.registrationNumber}? It cannot be booked afterwards.`}
        confirmLabel="Soft-delete"
        onCancel={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
      />

      <ConfirmDialog
        open={!!pendingRestore}
        title="Restore vehicle"
        message={`Restore ${pendingRestore?.registrationNumber}? It can be booked again.`}
        confirmLabel="Restore"
        onCancel={() => setPendingRestore(null)}
        onConfirm={confirmRestore}
      />
    </section>
  );
}
