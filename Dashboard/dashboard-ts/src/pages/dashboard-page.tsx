import type { ReactElement } from 'react';

export const DashboardPage = (): ReactElement => (
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
        <h3 className="metric-value">—</h3>
        <p className="metric-trend">Live count loading…</p>
      </div>
      <div className="metric-card">
        <p className="metric-label">Appointments today</p>
        <h3 className="metric-value">—</h3>
      </div>
    </div>
  </section>
);
