import { Navigate, Route, Routes } from 'react-router-dom';
import { BookingsPage } from '../features/bookings/BookingsPage';
import { CustomersPage } from '../features/customers/CustomersPage';
import { VehiclesPage } from '../features/vehicles/VehiclesPage';
import { AppLayout } from './AppLayout';

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Navigate to="/vehicles" replace />} />
        <Route path="vehicles" element={<VehiclesPage />} />
        <Route path="customers" element={<CustomersPage />} />
        <Route path="bookings" element={<BookingsPage />} />
      </Route>
    </Routes>
  );
}
