import { useQuery } from '@tanstack/react-query';
import { useMemo, useState, type ReactElement } from 'react';
import { apiFetch } from '../api/client';
import { CLINICAL_API, SCHEDULING_API } from '../api/config';

interface SyncRecord {
  readonly Id: string;
  readonly Service: 'clinical' | 'scheduling';
  readonly Operation: number;
  readonly EntityType: string;
  readonly EntityId: string;
  readonly Timestamp: string;
  readonly Version: number;
}

interface SyncChange {
  readonly Version: number;
  readonly Operation: number;
  readonly EntityType: string;
  readonly EntityId: string;
  readonly Timestamp: string;
}

const fetchClinicalChanges = async (): Promise<SyncChange[]> =>
  apiFetch<SyncChange[]>(`${CLINICAL_API}/sync/changes?fromVersion=0&limit=100`);

const fetchSchedulingChanges = async (): Promise<SyncChange[]> =>
  apiFetch<SyncChange[]>(`${SCHEDULING_API}/sync/changes?fromVersion=0&limit=100`);

const operationLabel = (op: number): string => {
  switch (op) {
    case 0: {
      return 'Insert';
    }
    case 1: {
      return 'Update';
    }
    case 2: {
      return 'Delete';
    }
    default: {
      return `Op ${String(op)}`;
    }
  }
};

export const SyncPage = (): ReactElement => {
  const clinical = useQuery({
    queryKey: ['sync', 'clinical'],
    queryFn: fetchClinicalChanges,
  });
  const scheduling = useQuery({
    queryKey: ['sync', 'scheduling'],
    queryFn: fetchSchedulingChanges,
  });

  const [serviceFilter, setServiceFilter] = useState<'all' | 'clinical' | 'scheduling'>('all');
  const [actionFilter, setActionFilter] = useState<string>('all');
  const [search, setSearch] = useState('');

  const allRecords = useMemo<SyncRecord[]>(() => {
    const fromClinical = (clinical.data ?? []).map<SyncRecord>((c) => ({
      Id: `clinical-${String(c.Version)}`,
      Service: 'clinical',
      Operation: c.Operation,
      EntityType: c.EntityType,
      EntityId: c.EntityId,
      Timestamp: c.Timestamp,
      Version: c.Version,
    }));
    const fromScheduling = (scheduling.data ?? []).map<SyncRecord>((c) => ({
      Id: `scheduling-${String(c.Version)}`,
      Service: 'scheduling',
      Operation: c.Operation,
      EntityType: c.EntityType,
      EntityId: c.EntityId,
      Timestamp: c.Timestamp,
      Version: c.Version,
    }));
    return [...fromClinical, ...fromScheduling];
  }, [clinical.data, scheduling.data]);

  const filtered = allRecords.filter((r) => {
    if (serviceFilter !== 'all' && r.Service !== serviceFilter) return false;
    if (actionFilter !== 'all' && String(r.Operation) !== actionFilter) return false;
    if (search !== '') {
      const q = search.toLowerCase();
      if (!r.EntityType.toLowerCase().includes(q) && !r.EntityId.toLowerCase().includes(q)) {
        return false;
      }
    }
    return true;
  });

  return (
    <section className="page" data-testid="sync-page">
      <div className="page-header">
        <div>
          <h2>Sync Dashboard</h2>
          <p className="page-description">Monitor and manage sync operations</p>
        </div>
      </div>
      <div className="sync-service-grid">
        <div className="sync-service-card" data-testid="service-status-clinical">
          <h3>Clinical.Api</h3>
          <p>{clinical.isError ? 'Error' : `${String((clinical.data ?? []).length)} changes`}</p>
        </div>
        <div className="sync-service-card" data-testid="service-status-scheduling">
          <h3>Scheduling.Api</h3>
          <p>
            {scheduling.isError ? 'Error' : `${String((scheduling.data ?? []).length)} changes`}
          </p>
        </div>
      </div>
      <div className="sync-filters">
        <select
          data-testid="service-filter"
          value={serviceFilter}
          onChange={(e) => {
            const v = e.target.value;
            if (v === 'all' || v === 'clinical' || v === 'scheduling') setServiceFilter(v);
          }}
        >
          <option value="all">All services</option>
          <option value="clinical">Clinical</option>
          <option value="scheduling">Scheduling</option>
        </select>
        <select
          data-testid="action-filter"
          value={actionFilter}
          onChange={(e) => {
            setActionFilter(e.target.value);
          }}
        >
          <option value="all">All operations</option>
          <option value="0">Insert</option>
          <option value="1">Update</option>
          <option value="2">Delete</option>
        </select>
        <input
          data-testid="sync-search"
          className="input"
          type="search"
          placeholder="Search entity..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
          }}
        />
      </div>
      <h3>Sync Records</h3>
      <table className="table" data-testid="sync-records-table">
        <thead>
          <tr>
            <th>Service</th>
            <th>Operation</th>
            <th>Entity</th>
            <th>ID</th>
            <th>Version</th>
            <th>Time</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map((r) => (
            <tr key={r.Id} data-service={r.Service} data-operation={String(r.Operation)}>
              <td>{r.Service}</td>
              <td>{operationLabel(r.Operation)}</td>
              <td>{r.EntityType}</td>
              <td>{r.EntityId}</td>
              <td>{r.Version}</td>
              <td>{new Date(r.Timestamp).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
};
