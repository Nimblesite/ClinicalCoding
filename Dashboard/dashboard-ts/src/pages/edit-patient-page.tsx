import { useState, type ReactElement } from 'react';
import { PatientForm } from '../forms/patient-form';
import type { PatientFormValues } from '../forms/schemas';
import { usePatient } from '../hooks/use-patients';
import { useSavePatient } from '../hooks/use-update-patient';
import { navigate } from '../router/hash-router';

interface EditPatientPageProps {
  readonly id?: string;
}

export const EditPatientPage = ({ id }: EditPatientPageProps): ReactElement => {
  const { data, isLoading } = usePatient(id);
  const save = useSavePatient();
  const [success, setSuccess] = useState(false);

  if (isLoading) return <div className="page">Loading patient…</div>;

  const handleSubmit = (values: PatientFormValues): void => {
    save
      .mutateAsync({
        id,
        patient: {
          GivenName: values.GivenName,
          FamilyName: values.FamilyName,
          Gender: values.Gender,
          Active: values.Active,
          ...(values.BirthDate !== undefined && values.BirthDate !== ''
            ? { BirthDate: values.BirthDate }
            : {}),
        },
      })
      .then(() => {
        setSuccess(true);
        globalThis.setTimeout(() => {
          navigate('patients');
        }, 250);
      })
      .catch(() => undefined);
  };

  return (
    <section className="page" data-testid="edit-patient-page">
      <h2>{id === undefined ? 'Add patient' : 'Edit patient'}</h2>
      {save.isError && <div className="alert alert-error">{save.error.message}</div>}
      {success && (
        <div className="alert alert-success" data-testid="edit-success">
          Patient updated successfully
        </div>
      )}
      <PatientForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save'}
      />
    </section>
  );
};
