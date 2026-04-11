import type { ReactElement } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/auth-context';
import { Header } from './header';
import { Sidebar } from './sidebar';

const TITLES: Readonly<Record<string, string>> = {
  '/': 'Dashboard',
  '/patients': 'Patients',
  '/practitioners': 'Practitioners',
  '/appointments': 'Appointments',
  '/calendar': 'Calendar',
  '/coding': 'Clinical Coding',
};

export const AppShell = (): ReactElement => {
  const { logout } = useAuth();
  const location = useLocation();
  const title = TITLES[location.pathname] ?? 'Nimblesite';
  return (
    <div className="app">
      <Sidebar
        onLogout={() => {
          void logout();
        }}
      />
      <div className="app-main">
        <Header title={title} />
        <div className="app-content">
          <Outlet />
        </div>
      </div>
    </div>
  );
};
