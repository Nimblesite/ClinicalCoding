import type { ReactElement } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { AppointmentForm } from '../forms/appointment-form';
import type { AppointmentFormValues } from '../forms/schemas';
import { useAppointment } from '../hooks/use-appointments';
import { useSaveAppointment } from '../hooks/use-update-appointment';

export const EditAppointmentPage = (): ReactElement => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading } = useAppointment(id);
  const save = useSaveAppointment();

  if (isLoading) return <div className="page">Loading appointment…</div>;

  const handleSubmit = async (values: AppointmentFormValues): Promise<void> => {
    await save.mutateAsync({
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
    });
    navigate('/appointments');
  };

  return (
    <section className="page">
      <h2>{id === undefined ? 'Add appointment' : 'Edit appointment'}</h2>
      {save.isError && <div className="alert alert-error">{save.error.message}</div>}
      <AppointmentForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save'}
      />
    </section>
  );
};
