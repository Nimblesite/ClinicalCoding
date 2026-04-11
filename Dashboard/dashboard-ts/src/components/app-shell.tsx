import type { ReactElement, ReactNode } from 'react';
import { useAuth } from '../auth/use-auth';
import { useRoute } from '../router/router-hooks';
import { Header } from './header';
import { Sidebar } from './sidebar';

const TITLES: Readonly<Record<string, string>> = {
  dashboard: 'Dashboard',
  patients: 'Patients',
  practitioners: 'Practitioners',
  appointments: 'Appointments',
  calendar: 'Calendar',
  coding: 'Clinical Coding',
  'clinical-coding': 'Clinical Coding',
  sync: 'Sync Dashboard',
};

interface AppShellProps {
  readonly children: ReactNode;
}

export const AppShell = ({ children }: AppShellProps): ReactElement => {
  const { logout } = useAuth();
  const route = useRoute();
  const title = TITLES[route.name] ?? 'Nimblesite';
  return (
    <div className="app">
      <Sidebar
        onLogout={() => {
          void logout();
        }}
      />
      <div className="app-main">
        <Header title={title} />
        <div className="app-content">{children}</div>
      </div>
    </div>
  );
};
