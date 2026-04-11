import { createHashRouter, type RouteObject } from 'react-router-dom';
import { AuthGate } from './components/auth-gate';
import { AppShell } from './components/app-shell';
import { AppointmentsPage } from './pages/appointments-page';
import { CalendarPage } from './pages/calendar-page';
import { ClinicalCodingPage } from './pages/clinical-coding-page';
import { DashboardPage } from './pages/dashboard-page';
import { EditAppointmentPage } from './pages/edit-appointment-page';
import { EditPatientPage } from './pages/edit-patient-page';
import { LoginPage } from './pages/login-page';
import { NotFoundPage } from './pages/not-found-page';
import { PatientsPage } from './pages/patients-page';
import { PractitionersPage } from './pages/practitioners-page';

const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: (
      <AuthGate>
        <AppShell />
      </AuthGate>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'patients', element: <PatientsPage /> },
      { path: 'patients/new', element: <EditPatientPage /> },
      { path: 'patients/edit/:id', element: <EditPatientPage /> },
      { path: 'practitioners', element: <PractitionersPage /> },
      { path: 'appointments', element: <AppointmentsPage /> },
      { path: 'appointments/new', element: <EditAppointmentPage /> },
      { path: 'appointments/edit/:id', element: <EditAppointmentPage /> },
      { path: 'calendar', element: <CalendarPage /> },
      { path: 'coding', element: <ClinicalCodingPage /> },
    ],
  },
  { path: '*', element: <NotFoundPage /> },
];

export const router = createHashRouter(routes);