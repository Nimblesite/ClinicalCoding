import { useState, type ReactElement } from 'react';

interface HeaderProps {
  readonly title: string;
}

export const Header = ({ title }: HeaderProps): ReactElement => {
  const [query, setQuery] = useState('');
  return (
    <header className="app-header">
      <h1 className="app-header-title">{title}</h1>
      <div className="app-header-search">
        <span className="material-symbols-outlined">search</span>
        <input
          type="search"
          placeholder="Search patients or codes…"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
          }}
        />
      </div>
      <div className="app-header-actions">
        <button type="button" className="icon-button" aria-label="Notifications">
          <span className="material-symbols-outlined">notifications</span>
        </button>
        <button type="button" className="icon-button" aria-label="Help">
          <span className="material-symbols-outlined">help_outline</span>
        </button>
      </div>
    </header>
  );
};
