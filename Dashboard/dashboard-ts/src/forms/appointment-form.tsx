import { zodResolver } from '@hookform/resolvers/zod';
import type { ReactElement } from 'react';
import { useForm, type SubmitHandler } from 'react-hook-form';
import type { Appointment } from '../types/fhir';
import { appointmentSchema, type AppointmentFormValues } from './schemas';

interface AppointmentFormProps {
  readonly initial?: Appointment;
  readonly onSubmit: (values: AppointmentFormValues) => Promise<void> | void;
  readonly submitLabel: string;
}

const toLocalInputValue = (iso: string): string => {
  if (iso === '') return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000).toISOString().slice(0, 16);
};

export const AppointmentForm = ({
  initial,
  onSubmit,
  submitLabel,
}: AppointmentFormProps): ReactElement => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<AppointmentFormValues>({
    resolver: zodResolver(appointmentSchema),
    defaultValues: {
      ServiceCategory: initial?.ServiceCategory ?? 'General',
      ServiceType: initial?.ServiceType ?? 'Checkup',
      Priority: initial?.Priority ?? 'routine',
      PatientReference: initial?.PatientReference ?? 'Patient/1',
      PractitionerReference: initial?.PractitionerReference ?? 'Practitioner/1',
      StartTime: toLocalInputValue(initial?.StartTime ?? ''),
      EndTime: toLocalInputValue(initial?.EndTime ?? ''),
    },
  });

  const submit: SubmitHandler<AppointmentFormValues> = async (values) => {
    await onSubmit(values);
  };

  return (
    <form
      // eslint-disable-next-line @typescript-eslint/no-misused-promises -- React Hook Form handleSubmit returns a promise, but the onSubmit attribute expects void
      onSubmit={handleSubmit(submit)}
      className="form"
    >
      <label className="input-label" htmlFor="appointment-service-type">
        Service Type
      </label>
      <input
        id="appointment-service-type"
        data-testid="appointment-service-type"
        className="input"
        {...register('ServiceType')}
      />
      {errors.ServiceType !== undefined && (
        <p className="form-error">{errors.ServiceType.message}</p>
      )}

      <label className="input-label" htmlFor="patient">
        Patient Reference
      </label>
      <input id="patient" className="input" {...register('PatientReference')} />

      <label className="input-label" htmlFor="prac">
        Practitioner Reference
      </label>
      <input id="prac" className="input" {...register('PractitionerReference')} />

      <label className="input-label" htmlFor="start">
        Start
      </label>
      <input id="start" type="datetime-local" className="input" {...register('StartTime')} />

      <label className="input-label" htmlFor="end">
        End
      </label>
      <input id="end" type="datetime-local" className="input" {...register('EndTime')} />

      <label className="input-label" htmlFor="priority">
        Priority
      </label>
      <select id="priority" className="input" {...register('Priority')}>
        <option value="routine">Routine</option>
        <option value="urgent">Urgent</option>
        <option value="asap">ASAP</option>
        <option value="stat">STAT</option>
      </select>

      <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
        {isSubmitting ? 'Saving…' : submitLabel}
      </button>
    </form>
  );
};
