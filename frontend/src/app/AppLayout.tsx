import { NavLink, Outlet } from 'react-router-dom';

export function AppLayout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">B</span>
          <div>
            <strong>Bruno</strong>
            <p>Vehicle Hire</p>
          </div>
        </div>
        <nav>
          <NavLink to="/vehicles">Vehicles</NavLink>
          <NavLink to="/customers">Customers</NavLink>
          <NavLink to="/bookings">Bookings</NavLink>
        </nav>
      </aside>
      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}
