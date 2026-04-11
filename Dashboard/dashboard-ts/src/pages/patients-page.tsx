import { useState, type ReactElement } from 'react';
import { AddPatientModal } from '../components/add-patient-modal';
import { usePatients } from '../hooks/use-patients';

export const PatientsPage = (): ReactElement => {
  const { data, isLoading, isError, error } = usePatients();
  const [search, setSearch] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  if (isLoading) return <div className="page">Loading patients…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  const rows = (data ?? []).filter((p) => {
    if (search === '') return true;
    const q = search.toLowerCase();
    return (
      p.GivenName.toLowerCase().includes(q) ||
      p.FamilyName.toLowerCase().includes(q)
    );
  });
  return (
    <section className="page">
      <div className="page-header">
        <h2>Patients</h2>
        <div className="page-header-actions">
          <input
            className="input"
            type="search"
            placeholder="Search patients…"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
            }}
          />
          <button
            type="button"
            data-testid="add-patient-btn"
            className="btn btn-primary"
            onClick={() => {
              setModalOpen(true);
            }}
          >
            Add Patient
          </button>
        </div>
      </div>
      <div className="table-container">
        <table className="table">
          <thead>
            <tr>
              <th>Given Name</th>
              <th>Family Name</th>
              <th>Gender</th>
              <th>Active</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((p) => (
              <tr
                key={p.Id ?? `${p.GivenName}-${p.FamilyName}`}
                className="patient-row"
                data-testid="patient-row"
              >
                <td>{p.GivenName}</td>
                <td>{p.FamilyName}</td>
                <td>{p.Gender}</td>
                <td>{p.Active ? 'Yes' : 'No'}</td>
                <td>
                  {p.Id !== undefined && (
                    <a
                      className="btn btn-secondary"
                      data-testid={`edit-patient-${p.Id}`}
                      href={`#patients/edit/${p.Id}`}
                    >
                      Edit
                    </a>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <AddPatientModal
        open={modalOpen}
        onClose={() => {
          setModalOpen(false);
        }}
      />
    </section>
  );
};
