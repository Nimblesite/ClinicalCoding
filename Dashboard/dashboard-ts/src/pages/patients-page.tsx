import type { ReactElement } from 'react';
import { Link } from 'react-router-dom';
import { usePatients } from '../hooks/use-patients';

export const PatientsPage = (): ReactElement => {
  const { data, isLoading, isError, error } = usePatients();
  if (isLoading) return <div className="page">Loading patients…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  return (
    <section className="page">
      <div className="page-header">
        <h2>Patients</h2>
        <Link className="btn btn-primary" to="/patients/new">
          Add patient
        </Link>
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
            {(data ?? []).map((p) => (
              <tr key={p.Id ?? `${p.GivenName}-${p.FamilyName}`}>
                <td>{p.GivenName}</td>
                <td>{p.FamilyName}</td>
                <td>{p.Gender}</td>
                <td>{p.Active ? 'Yes' : 'No'}</td>
                <td>{p.Id !== undefined && <Link to={`/patients/edit/${p.Id}`}>Edit</Link>}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
};
