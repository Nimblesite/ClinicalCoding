import type { ReactElement } from 'react';

export const DashboardPage = (): ReactElement => (
  <section className="page">
    <div className="page-header">
      <div>
        <h2 className="welcome-title">Welcome back!</h2>
        <p className="page-description">Here is what is happening in your department today.</p>
      </div>
    </div>
    <div className="kpi-grid">
      <div className="kpi-card">
        <p className="kpi-label">Top Treatment</p>
        <h3 className="kpi-value">Cardiology</h3>
        <p className="kpi-trend">+12% this month</p>
      </div>
      <div className="kpi-card">
        <p className="kpi-label">Satisfaction</p>
        <h3 className="kpi-value">98.2 / 100</h3>
      </div>
      <div className="kpi-card">
        <p className="kpi-label">Total Patients</p>
        <h3 className="kpi-value">—</h3>
        <p className="kpi-trend">Live count loading…</p>
      </div>
      <div className="kpi-card">
        <p className="kpi-label">Appointments today</p>
        <h3 className="kpi-value">—</h3>
      </div>
    </div>
  </section>
);
