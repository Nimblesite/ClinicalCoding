import { useState, type ReactElement } from 'react';
import { useAuth } from '../auth/use-auth';
import { useRoute } from '../router/router-hooks';

interface SidebarProps {
  readonly onLogout: () => void;
}

const NAV_ITEMS: ReadonlyArray<{
  readonly to: string;
  readonly label: string;
  readonly icon: string;
}> = [
  { to: 'dashboard', label: 'Dashboard', icon: 'event_note' },
  { to: 'patients', label: 'Patients', icon: 'group' },
  { to: 'practitioners', label: 'Practitioners', icon: 'medical_services' },
  { to: 'appointments', label: 'Appointments', icon: 'calendar_today' },
  { to: 'calendar', label: 'Schedule', icon: 'event' },
  { to: 'clinical-coding', label: 'Clinical Coding', icon: 'terminal' },
  { to: 'sync', label: 'Sync Dashboard', icon: 'sync' },
];

export const Sidebar = ({ onLogout }: SidebarProps): ReactElement => {
  const { currentUser } = useAuth();
  const route = useRoute();
  const [menuOpen, setMenuOpen] = useState(false);
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
        {NAV_ITEMS.map((item) => {
          const active =
            route.name === item.to ||
            (item.to === 'clinical-coding' && route.name === 'coding') ||
            (item.to === 'dashboard' && route.name === '');
          return (
            <a
              key={item.to}
              className={`sidebar-link${active ? ' active' : ''}`}
              href={`#${item.to}`}
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span>{item.label}</span>
            </a>
          );
        })}
      </nav>
      <div className="sidebar-footer">
        <button
          className="sidebar-user-menu"
          data-testid="user-menu-button"
          type="button"
          onClick={() => {
            setMenuOpen((o) => !o);
          }}
        >
          <div className="avatar">{currentUser?.displayName.slice(0, 1) ?? '?'}</div>
          <div>
            <p className="sidebar-user-name">{currentUser?.displayName ?? 'Unknown'}</p>
            <p className="sidebar-user-email">{currentUser?.email ?? ''}</p>
          </div>
        </button>
        {menuOpen ? (
          <div className="user-dropdown" data-testid="user-dropdown">
            <p className="user-dropdown-name">{currentUser?.displayName ?? ''}</p>
            <p className="user-dropdown-email">{currentUser?.email ?? ''}</p>
            <button
              className="btn btn-secondary"
              data-testid="logout-button"
              type="button"
              onClick={onLogout}
            >
              <span className="material-symbols-outlined">logout</span>
              Sign out
            </button>
          </div>
        ) : null}
      </div>
    </aside>
  );
};
