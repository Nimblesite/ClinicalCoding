import { useState, type ReactElement } from 'react';
import { AppointmentForm } from '../forms/appointment-form';
import type { AppointmentFormValues } from '../forms/schemas';
import { useAppointment } from '../hooks/use-appointments';
import { useSaveAppointment } from '../hooks/use-update-appointment';

interface EditAppointmentPageProps {
  readonly id?: string;
}

const toIso = (local: string): string => {
  if (local === '') return new Date().toISOString();
  const parsed = new Date(local);
  return Number.isNaN(parsed.getTime()) ? new Date().toISOString() : parsed.toISOString();
};

export const EditAppointmentPage = ({ id }: EditAppointmentPageProps): ReactElement => {
  const { data, isLoading } = useAppointment(id);
  const save = useSaveAppointment();
  const [success, setSuccess] = useState(false);

  if (isLoading) return <div className="page">Loading appointment…</div>;

  const handleSubmit = (values: AppointmentFormValues): void => {
    save
      .mutateAsync({
        id,
        appointment: {
          ServiceCategory: values.ServiceCategory,
          ServiceType: values.ServiceType,
          Priority: values.Priority,
          PatientReference: values.PatientReference,
          PractitionerReference: values.PractitionerReference,
          StartTime: toIso(values.StartTime),
          EndTime: toIso(values.EndTime),
        },
      })
      .then(() => {
        setSuccess(true);
      })
      .catch(() => {
        // Error handled by mutation state
      });
  };

  return (
    <section className="page" data-testid="edit-appointment-page">
      <h2>{id === undefined ? 'Add appointment' : 'Edit Appointment'}</h2>
      {save.isError ? <div className="alert alert-error">{save.error.message}</div> : null}
      {success ? (
        <div className="alert alert-success" data-testid="edit-success">
          Appointment updated successfully
        </div>
      ) : null}
      <AppointmentForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save Changes'}
      />
    </section>
  );
};
