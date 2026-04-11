import { zodResolver } from '@hookform/resolvers/zod';
import type { ReactElement } from 'react';
import { useForm, type SubmitHandler } from 'react-hook-form';
import type { Patient } from '../types/fhir';
import { patientSchema, type PatientFormValues } from './schemas';

interface PatientFormProps {
  readonly initial?: Patient;
  readonly onSubmit: (values: PatientFormValues) => Promise<void> | void;
  readonly submitLabel: string;
}

export const PatientForm = ({ initial, onSubmit, submitLabel }: PatientFormProps): ReactElement => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PatientFormValues>({
    resolver: zodResolver(patientSchema),
    defaultValues: {
      GivenName: initial?.GivenName ?? '',
      FamilyName: initial?.FamilyName ?? '',
      Gender: initial?.Gender ?? 'other',
      Active: initial?.Active ?? true,
      BirthDate: initial?.BirthDate ?? '',
    },
  });

  const submit: SubmitHandler<PatientFormValues> = async (values) => {
    await onSubmit(values);
  };

  return (
    <form
      // eslint-disable-next-line @typescript-eslint/no-misused-promises -- React Hook Form handleSubmit returns a promise, but the onSubmit attribute expects void
      onSubmit={handleSubmit(submit)}
      className="form"
    >
      <label className="input-label" htmlFor="given">
        Given Name
      </label>
      <input
        id="given"
        data-testid="edit-given-name"
        className="input"
        {...register('GivenName')}
      />
      {errors.GivenName !== undefined && <p className="form-error">{errors.GivenName.message}</p>}

      <label className="input-label" htmlFor="family">
        Family Name
      </label>
      <input
        id="family"
        data-testid="edit-family-name"
        className="input"
        {...register('FamilyName')}
      />
      {errors.FamilyName !== undefined && <p className="form-error">{errors.FamilyName.message}</p>}

      <label className="input-label" htmlFor="gender">
        Gender
      </label>
      <select id="gender" className="input" {...register('Gender')}>
        <option value="male">Male</option>
        <option value="female">Female</option>
        <option value="other">Other</option>
      </select>

      <label className="input-label">
        <input type="checkbox" {...register('Active')} /> Active
      </label>

      <button
        type="submit"
        data-testid="save-patient"
        className="btn btn-primary"
        disabled={isSubmitting}
      >
        {isSubmitting ? 'Saving…' : submitLabel}
      </button>
    </form>
  );
};