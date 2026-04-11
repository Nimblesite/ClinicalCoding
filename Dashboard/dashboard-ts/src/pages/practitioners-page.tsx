import type { ReactElement } from 'react';
import { usePractitioners } from '../hooks/use-practitioners';

export const PractitionersPage = (): ReactElement => {
  const { data, isLoading, isError, error } = usePractitioners();
  if (isLoading) return <div className="page">Loading practitioners…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  return (
    <section className="page">
      <div className="page-header">
        <h2>Practitioners</h2>
      </div>
      <div className="table-container">
        <table className="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Qualification</th>
              <th>Specialty</th>
              <th>Email</th>
            </tr>
          </thead>
          <tbody>
            {(data ?? []).map((p) => (
              <tr key={p.Id ?? p.Identifier}>
                <td>{p.Identifier}</td>
                <td>
                  {p.NameGiven} {p.NameFamily}
                </td>
                <td>{p.Qualification}</td>
                <td>{p.Specialty ?? ''}</td>
                <td>{p.TelecomEmail ?? ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
};
