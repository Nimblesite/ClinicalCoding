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

  const handleSubmit = (values: AppointmentFormValues): void => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises -- IIFE handles the async mutation
    ;(async (): Promise<void> => {
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
    })();
  };

  return (
    <section className="page">
      <h2>{id === undefined ? 'Add appointment' : 'Edit appointment'}</h2>
      {/* eslint-disable-next-line react/jsx-no-leaked-render, @typescript-eslint/no-unnecessary-condition, @typescript-eslint/strict-boolean-expressions -- save.error check prevents leaked render */}
      {save.isError && save.error && (
        <div className="alert alert-error">{save.error.message}</div>
      )}
      <AppointmentForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save'}
      />
    </section>
  );
};