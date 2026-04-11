import type { ReactElement } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/auth-context';

interface SidebarProps {
  readonly onLogout: () => void;
}

const NAV_ITEMS: ReadonlyArray<{
  readonly to: string;
  readonly label: string;
  readonly icon: string;
}> = [
  { to: '/', label: 'Dashboard', icon: 'event_note' },
  { to: '/patients', label: 'Patients', icon: 'group' },
  { to: '/practitioners', label: 'Practitioners', icon: 'medical_services' },
  { to: '/appointments', label: 'Appointments', icon: 'calendar_today' },
  { to: '/calendar', label: 'Calendar', icon: 'event' },
  { to: '/coding', label: 'Clinical Coding', icon: 'terminal' },
];

export const Sidebar = ({ onLogout }: SidebarProps): ReactElement => {
  const { currentUser } = useAuth();
  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="brand-mark">
          <span className="material-symbols-outlined">health_metrics</span>
        </div>
        <div>
          <h2>Nimblesite</h2>
          <p>Clinical Data Unit</p>
        </div>
      </div>
      <nav className="sidebar-nav">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
          >
            <span className="material-symbols-outlined">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
      <div className="sidebar-footer">
        <div className="sidebar-user">
          <div className="avatar">{currentUser?.displayName.slice(0, 1) ?? '?'}</div>
          <div>
            <p className="sidebar-user-name">{currentUser?.displayName ?? 'Unknown'}</p>
            <p className="sidebar-user-email">{currentUser?.email ?? ''}</p>
          </div>
        </div>
        <button type="button" className="btn btn-secondary" onClick={onLogout}>
          <span className="material-symbols-outlined">logout</span>
          Sign out
        </button>
      </div>
    </aside>
  );
};
