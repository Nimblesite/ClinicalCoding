import { useState, type ReactElement } from 'react';
import { AddAppointmentModal } from '../components/add-appointment-modal';
import { useAppointments } from '../hooks/use-appointments';

export const AppointmentsPage = (): ReactElement => {
  const { data, isLoading, isError, error } = useAppointments();
  const [modalOpen, setModalOpen] = useState(false);
  if (isLoading) return <div className="page">Loading appointments…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  return (
    <section className="page">
      <div className="page-header">
        <h2>Appointments</h2>
        <button
          type="button"
          data-testid="add-appointment-btn"
          className="btn btn-primary"
          onClick={() => {
            setModalOpen(true);
          }}
        >
          Add Appointment
        </button>
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
              <tr
                key={a.Id ?? `${a.StartTime}-${a.PatientReference}`}
                className="appointment-row"
                data-testid="appointment-row"
              >
                <td>{a.ServiceType}</td>
                <td>{a.PatientReference}</td>
                <td>{a.PractitionerReference}</td>
                <td>{new Date(a.StartTime).toLocaleString()}</td>
                <td>{a.Priority}</td>
                <td>
                  {a.Id !== undefined && (
                    <a
                      className="btn btn-secondary"
                      data-testid={`edit-appointment-${a.Id}`}
                      href={`#appointments/edit/${a.Id}`}
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
      <AddAppointmentModal
        open={modalOpen}
        onClose={() => {
          setModalOpen(false);
        }}
      />
    </section>
  );
};
