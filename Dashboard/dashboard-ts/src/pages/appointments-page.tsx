import type { ReactElement } from 'react';
import { Link } from 'react-router-dom';
import { useAppointments } from '../hooks/use-appointments';

export const AppointmentsPage = (): ReactElement => {
  const { data, isLoading, isError, error } = useAppointments();
  if (isLoading) return <div className="page">Loading appointments…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  return (
    <section className="page">
      <div className="page-header">
        <h2>Appointments</h2>
      </div>
      <div className="table-container">
        <table className="table">
          <thead>
            <tr>
              <th>Service</th>
              <th>Patient</th>
              <th>Practitioner</th>
              <th>Start</th>
              <th>Priority</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {(data ?? []).map((a) => (
              <tr key={a.Id ?? `${a.Start}-${a.PatientReference}`}>
                <td>{a.ServiceType}</td>
                <td>{a.PatientReference}</td>
                <td>{a.PractitionerReference}</td>
                <td>{new Date(a.Start).toLocaleString()}</td>
                <td>{a.Priority}</td>
                <td>{a.Id !== undefined && <Link to={`/appointments/edit/${a.Id}`}>Edit</Link>}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
};
