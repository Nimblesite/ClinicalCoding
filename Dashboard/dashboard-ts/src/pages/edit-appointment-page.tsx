import { useState, type ReactElement } from 'react';
import { AppointmentForm } from '../forms/appointment-form';
import type { AppointmentFormValues } from '../forms/schemas';
import { useAppointment } from '../hooks/use-appointments';
import { useSaveAppointment } from '../hooks/use-update-appointment';
import { navigate } from '../router/hash-router';

interface EditAppointmentPageProps {
  readonly id?: string;
}

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
          Start: values.Start,
          End: values.End,
        },
      })
      .then(() => {
        setSuccess(true);
        globalThis.setTimeout(() => {
          navigate('appointments');
        }, 250);
      })
      .catch(() => undefined);
  };

  return (
    <section className="page" data-testid="edit-appointment-page">
      <h2>{id === undefined ? 'Add appointment' : 'Edit Appointment'}</h2>
      {save.isError && <div className="alert alert-error">{save.error.message}</div>}
      {success && (
        <div className="alert alert-success" data-testid="edit-success">
          Appointment updated successfully
        </div>
      )}
      <AppointmentForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save Changes'}
      />
    </section>
  );
};
