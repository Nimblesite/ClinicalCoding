import type { ReactElement } from 'react';
import { useAppointments } from '../hooks/use-appointments';
import { usePatients } from '../hooks/use-patients';
import type { Appointment } from '../types/fhir';

const formatTime = (iso: string): string =>
  new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

const formatRelative = (iso: string): string => {
  const mins = Math.round((new Date(iso).getTime() - Date.now()) / 60000);
  if (mins < 0) return 'Earlier today';
  if (mins < 60) return `In ${String(mins)} minutes`;
  const hrs = Math.round(mins / 60);
  return `In ${String(hrs)} hour${hrs === 1 ? '' : 's'}`;
};

const initials = (ref: string): string => {
  const parts = ref
    .replace(/^Patient\//u, '')
    .split(/[-\s]/u)
    .filter(Boolean);
  return (parts[0]?.[0] ?? '?').toUpperCase() + (parts[1]?.[0] ?? '').toUpperCase();
};

const AppointmentCard = ({ a }: { readonly a: Appointment }): ReactElement => (
  <div className="appt-card">
    <div className="appt-lead">
      <div className="appt-avatar">{initials(a.PatientReference)}</div>
      <div>
        <h4>{a.PatientReference.replace(/^Patient\//u, '')}</h4>
        <p>
          {a.ServiceType} • {a.Description ?? 'Consultation'}
        </p>
      </div>
    </div>
    <div className="appt-trail">
      <div className="appt-time">
        <p className="time">{formatTime(a.StartTime)}</p>
        <p className="rel">{formatRelative(a.StartTime)}</p>
      </div>
      <span className="material-symbols-outlined appt-more">more_vert</span>
    </div>
  </div>
);

export const DashboardPage = (): ReactElement => {
  const patients = usePatients();
  const appointments = useAppointments();
  const today = new Date();
  const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
  const tomorrowStart = new Date(todayStart);
  tomorrowStart.setDate(tomorrowStart.getDate() + 1);
  const todayAppts = (appointments.data ?? []).filter((a) => {
    const s = new Date(a.StartTime);
    return s >= todayStart && s < tomorrowStart;
  });
  const patientCount = (patients.data ?? []).length;
  const dateLabel = todayStart.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });

  return (
    <section className="page dashboard">
      <header className="dashboard-hero">
        <div>
          <h2 className="welcome-title">Welcome back, Dr. Robert!</h2>
          <p className="page-description">
            Here&apos;s what&apos;s happening in your department today.
          </p>
        </div>
        <div className="date-chip">
          <span className="material-symbols-outlined">calendar_today</span>
          <span>{dateLabel}</span>
        </div>
      </header>

      <div className="kpi-grid">
        <article className="kpi-card metric-card">
          <div className="kpi-head">
            <p className="kpi-label">Top Treatment</p>
            <span className="material-symbols-outlined kpi-icon primary">analytics</span>
          </div>
          <div className="kpi-body">
            <div className="kpi-donut">
              <span>64%</span>
            </div>
            <div>
              <h3 className="kpi-value">Cardiology</h3>
              <p className="kpi-trend">
                <span className="material-symbols-outlined">trending_up</span>
                +12% this month
              </p>
            </div>
          </div>
        </article>

        <article className="kpi-card metric-card">
          <div className="kpi-head">
            <p className="kpi-label">Satisfaction Rate</p>
            <span className="material-symbols-outlined kpi-icon tertiary">sentiment_satisfied</span>
          </div>
          <div className="kpi-body column">
            <div className="kpi-inline">
              <h3 className="kpi-value">98.2</h3>
              <span className="kpi-unit">/100</span>
            </div>
            <div className="spark-bars">
              <span style={{ height: '50%' }} className="b1" />
              <span style={{ height: '75%' }} className="b1" />
              <span style={{ height: '100%' }} className="b3" />
              <span style={{ height: '80%' }} className="b2" />
              <span style={{ height: '90%' }} className="b2" />
            </div>
          </div>
        </article>

        <article className="kpi-card metric-card">
          <div className="kpi-head">
            <p className="kpi-label">Total Patients</p>
            <span className="material-symbols-outlined kpi-icon secondary">group</span>
          </div>
          <div className="kpi-body">
            <div>
              <h3 className="kpi-value">{patientCount.toLocaleString()}</h3>
              <p className="kpi-sub">Active clinical cases</p>
            </div>
            <div className="bar-mini">
              <span style={{ height: '50%', opacity: 0.2 }} />
              <span style={{ height: '66%', opacity: 0.4 }} />
              <span style={{ height: '82%', opacity: 0.6 }} />
              <span style={{ height: '100%', opacity: 1 }} />
            </div>
          </div>
        </article>

        <article className="kpi-card metric-card">
          <div className="kpi-head">
            <p className="kpi-label">Appointments</p>
            <span className="material-symbols-outlined kpi-icon primary">bookmark_check</span>
          </div>
          <div className="kpi-body column">
            <h3 className="kpi-value">{todayAppts.length}</h3>
            <p className="kpi-sub">Scheduled for today</p>
            <div className="kpi-progress">
              <span style={{ width: '75%' }} />
            </div>
            <p className="kpi-progress-label">75% capacity</p>
          </div>
        </article>
      </div>

      <div className="dashboard-split">
        <section className="appts-column">
          <div className="section-head">
            <h3>Upcoming Appointments</h3>
            <a className="section-link" href="#calendar">
              View Calendar
            </a>
          </div>
          {todayAppts.length === 0 ? (
            <p className="empty-state">No appointments scheduled for today.</p>
          ) : (
            <div className="appt-list">
              {todayAppts.slice(0, 5).map((a) => (
                <AppointmentCard key={a.Id ?? `${a.StartTime}-${a.PatientReference}`} a={a} />
              ))}
            </div>
          )}
        </section>

        <aside className="requests-column">
          <div className="section-head">
            <h3>Requests</h3>
            <span className="requests-badge">3 New</span>
          </div>
          <div className="request-card primary">
            <div className="request-head">
              <div className="request-icon">
                <span className="material-symbols-outlined">person_add</span>
              </div>
              <div>
                <h4>Emily Watson</h4>
                <p>Requested for: Tomorrow, 10:00 AM</p>
              </div>
            </div>
            <div className="request-actions">
              <button type="button" className="btn btn-primary btn-sm">
                Approve
              </button>
              <button type="button" className="btn btn-ghost btn-sm">
                Decline
              </button>
            </div>
          </div>
          <div className="request-card muted">
            <div className="request-head">
              <div className="request-icon muted">
                <span className="material-symbols-outlined">person_add</span>
              </div>
              <div>
                <h4>Marcus T.</h4>
                <p>Requested for: Friday, 02:30 PM</p>
              </div>
            </div>
            <div className="request-actions">
              <button type="button" className="btn btn-primary btn-sm">
                Approve
              </button>
              <button type="button" className="btn btn-ghost btn-sm">
                Decline
              </button>
            </div>
          </div>
          <a className="view-all-tile" href="#appointments">
            View All Requests (14)
          </a>
        </aside>
      </div>

      <div className="quick-tiles">
        <a className="tile tile-primary" href="#clinical-coding">
          <span className="material-symbols-outlined tile-glyph">medical_information</span>
          <div className="tile-body">
            <h4>Clinical Coding Guide</h4>
            <p>Review latest ICD-11 updates for cardiology.</p>
            <span className="tile-cta">Access Library</span>
          </div>
        </a>
        <a className="tile tile-tertiary" href="#practitioners">
          <span className="material-symbols-outlined tile-glyph">emergency</span>
          <div className="tile-body">
            <h4>On-Call Directory</h4>
            <p>Direct contact list for emergency department staff.</p>
            <span className="tile-cta">View Staff</span>
          </div>
        </a>
        <a className="tile tile-muted" href="#clinical-coding">
          <span className="material-symbols-outlined tile-glyph">lab_research</span>
          <div className="tile-body">
            <h4>Lab Results</h4>
            <p>12 pending lab results require your digital signature.</p>
            <span className="tile-cta on-light">Review Results</span>
          </div>
        </a>
      </div>

      <div className="quick-actions-legacy">
        <a className="btn btn-secondary" href="#patients">
          <span className="material-symbols-outlined">person_search</span>
          Patient Search
        </a>
        <a className="btn btn-secondary" href="#appointments">
          <span className="material-symbols-outlined">calendar_month</span>
          View Schedule
        </a>
      </div>
    </section>
  );
};
