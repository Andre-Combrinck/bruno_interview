import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { getErrorMessage } from '../../shared/api/client';
import type { Customer } from '../../shared/api/types';
import { ConfirmDialog, LoadingSkeleton, Modal } from '../../shared/components/ui';
import { useToast } from '../../shared/hooks/useToast';
import {
  useCreateCustomer,
  useCustomers,
  useDeleteCustomer,
  useUpdateCustomer,
  type CustomerInput,
} from './api';

const schema = z.object({
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  email: z.string().email(),
  phoneNumber: z.string().min(1),
});

type FormValues = z.infer<typeof schema>;

export function CustomersPage() {
  const { showError, showSuccess } = useToast();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<Customer | null>(null);
  const [creating, setCreating] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<Customer | null>(null);

  const query = useCustomers(search, page);
  const createMutation = useCreateCustomer();
  const updateMutation = useUpdateCustomer();
  const deleteMutation = useDeleteCustomer();

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { firstName: '', lastName: '', email: '', phoneNumber: '' },
  });

  const openCreate = () => {
    form.reset({ firstName: '', lastName: '', email: '', phoneNumber: '' });
    setEditing(null);
    setCreating(true);
  };

  const openEdit = (customer: Customer) => {
    form.reset({
      firstName: customer.firstName,
      lastName: customer.lastName,
      email: customer.email,
      phoneNumber: customer.phoneNumber,
    });
    setEditing(customer);
    setCreating(true);
  };

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      const input: CustomerInput = values;
      if (editing) {
        await updateMutation.mutateAsync({ id: editing.id, input });
        showSuccess('Customer updated');
      } else {
        await createMutation.mutateAsync(input);
        showSuccess('Customer created');
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
      showSuccess('Customer deleted');
      setPendingDelete(null);
    } catch (error) {
      showError(getErrorMessage(error));
    }
  };

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <h1>Customers</h1>
          <p>Cannot delete customers with existing bookings.</p>
        </div>
        <button type="button" className="primary-btn" onClick={openCreate}>
          Add customer
        </button>
      </header>

      <div className="toolbar">
        <input
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
          placeholder="Search name, email, phone"
        />
      </div>

      {query.isLoading ? <LoadingSkeleton /> : null}
      {query.isError ? <p className="error-text">{getErrorMessage(query.error)}</p> : null}

      {query.data ? (
        <>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((customer) => (
                  <tr key={customer.id}>
                    <td>
                      {customer.firstName} {customer.lastName}
                    </td>
                    <td>{customer.email}</td>
                    <td>{customer.phoneNumber}</td>
                    <td className="row-actions">
                      <button type="button" className="ghost-btn" onClick={() => openEdit(customer)}>
                        Edit
                      </button>
                      <button type="button" className="danger-btn" onClick={() => setPendingDelete(customer)}>
                        Delete
                      </button>
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
        title={editing ? 'Edit customer' : 'Add customer'}
        open={creating}
        onClose={() => {
          setCreating(false);
          setEditing(null);
        }}
      >
        <form className="stack-form" onSubmit={onSubmit}>
          <label>
            First name
            <input {...form.register('firstName')} />
          </label>
          <label>
            Last name
            <input {...form.register('lastName')} />
          </label>
          <label>
            Email
            <input type="email" {...form.register('email')} />
          </label>
          <label>
            Phone
            <input {...form.register('phoneNumber')} />
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
        title="Delete customer"
        message={`Delete ${pendingDelete?.firstName} ${pendingDelete?.lastName}? Blocked if they have any bookings.`}
        confirmLabel="Delete"
        onCancel={() => setPendingDelete(null)}
        onConfirm={confirmDelete}
      />
    </section>
  );
}
