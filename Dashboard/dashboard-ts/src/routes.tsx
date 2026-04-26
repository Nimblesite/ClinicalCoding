import { useEffect, type ReactElement } from 'react';
import { useAuth } from './auth/use-auth';
import { AppShell } from './components/app-shell';
import { AppointmentsPage } from './pages/appointments-page';
import { CalendarPage } from './pages/calendar-page';
import { ClinicalCodingPage } from './pages/clinical-coding-page';
import { DashboardPage } from './pages/dashboard-page';
import { EditAppointmentPage } from './pages/edit-appointment-page';
import { EditPatientPage } from './pages/edit-patient-page';
import { EditPractitionerPage } from './pages/edit-practitioner-page';
import { LoginPage } from './pages/login-page';
import { NotFoundPage } from './pages/not-found-page';
import { PatientsPage } from './pages/patients-page';
import { PractitionersPage } from './pages/practitioners-page';
import { SyncPage } from './pages/sync-page';
import { useRoute } from './router/router-hooks';

const renderPage = (name: string, params: readonly string[]): ReactElement => {
  switch (name) {
    case '':
    case 'dashboard': {
      return <DashboardPage />;
    }
    case 'patients': {
      if (params[0] === 'edit' && params[1] !== undefined) {
        return <EditPatientPage id={params[1]} />;
      }
      if (params[0] === 'new') {
        return <EditPatientPage />;
      }
      return <PatientsPage />;
    }
    case 'practitioners': {
      if (params[0] === 'edit' && params[1] !== undefined) {
        return <EditPractitionerPage id={params[1]} />;
      }
      return <PractitionersPage />;
    }
    case 'appointments': {
      if (params[0] === 'edit' && params[1] !== undefined) {
        return <EditAppointmentPage id={params[1]} />;
      }
      if (params[0] === 'new') {
        return <EditAppointmentPage />;
      }
      return <AppointmentsPage />;
    }
    case 'calendar': {
      return <CalendarPage />;
    }
    case 'clinical-coding':
    case 'coding': {
      return <ClinicalCodingPage />;
    }
    case 'sync': {
      return <SyncPage />;
    }
    default: {
      return <NotFoundPage />;
    }
  }
};

export const Routes = (): ReactElement => {
  const { isAuthenticated } = useAuth();
  const route = useRoute();

  useEffect(() => {
    if (isAuthenticated && route.name === 'login') {
      globalThis.location.replace('#dashboard');
    }
  }, [isAuthenticated, route.name]);

  if (!isAuthenticated || route.name === 'login') {
    return <LoginPage />;
  }

  return <AppShell>{renderPage(route.name, route.params)}</AppShell>;
};
