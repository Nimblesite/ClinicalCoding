import type { ReactElement } from 'react';
import { useAppointments } from '../hooks/use-appointments';
import { usePatients } from '../hooks/use-patients';

export const DashboardPage = (): ReactElement => {
  const patients = usePatients();
  const appointments = useAppointments();
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);
  const tomorrowStart = new Date(todayStart);
  tomorrowStart.setDate(tomorrowStart.getDate() + 1);
  const todayAppointments = (appointments.data ?? []).filter((a) => {
    const start = new Date(a.Start);
    return start >= todayStart && start < tomorrowStart;
  });
  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h2 className="welcome-title">Welcome back!</h2>
          <p className="page-description">Here is what is happening in your department today.</p>
        </div>
      </div>
      <div className="metric-grid">
        <div className="metric-card">
          <p className="metric-label">Top Treatment</p>
          <h3 className="metric-value">Cardiology</h3>
          <p className="metric-trend">+12% this month</p>
        </div>
        <div className="metric-card">
          <p className="metric-label">Satisfaction</p>
          <h3 className="metric-value">98.2 / 100</h3>
        </div>
        <div className="metric-card">
          <p className="metric-label">Total Patients</p>
          <h3 className="metric-value">{(patients.data ?? []).length}</h3>
          <p className="metric-trend">From Clinical API</p>
        </div>
        <div className="metric-card">
          <p className="metric-label">Appointments today</p>
          <h3 className="metric-value">{todayAppointments.length}</h3>
        </div>
      </div>
      <div className="dashboard-panels">
        <div className="dashboard-panel quick-actions">
          <h3>Quick Actions</h3>
          <div className="quick-actions-grid">
            <a className="btn btn-secondary" href="#patients">
              <span className="material-symbols-outlined">person_search</span>
              Patient Search
            </a>
            <a className="btn btn-secondary" href="#appointments">
              <span className="material-symbols-outlined">calendar_month</span>
              View Schedule
            </a>
            <a className="btn btn-secondary" href="#patients">
              <span className="material-symbols-outlined">person_add</span>
              Add Patient
            </a>
            <a className="btn btn-secondary" href="#clinical-coding">
              <span className="material-symbols-outlined">terminal</span>
              Clinical Coding
            </a>
          </div>
        </div>
        <div className="dashboard-panel upcoming-appointments">
          <h3>Upcoming Appointments</h3>
          {todayAppointments.length === 0 ? (
            <p className="empty-state">No appointments scheduled for today.</p>
          ) : (
            <ul>
              {todayAppointments.slice(0, 5).map((a) => (
                <li key={a.Id ?? `${a.Start}-${a.PatientReference}`}>
                  <strong>{new Date(a.Start).toLocaleTimeString()}</strong> {a.ServiceType}
                  {' — '}
                  {a.PatientReference}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
};
