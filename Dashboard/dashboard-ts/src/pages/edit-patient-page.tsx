import type { ReactElement } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PatientForm } from '../forms/patient-form';
import type { PatientFormValues } from '../forms/schemas';
import { usePatient } from '../hooks/use-patients';
import { useSavePatient } from '../hooks/use-update-patient';

export const EditPatientPage = (): ReactElement => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data, isLoading } = usePatient(id);
  const save = useSavePatient();

  if (isLoading) return <div className="page">Loading patient…</div>;

  const handleSubmit = async (values: PatientFormValues): Promise<void> => {
    await save.mutateAsync({
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
    });
    navigate('/patients');
  };

  return (
    <section className="page">
      <h2>{id === undefined ? 'Add patient' : 'Edit patient'}</h2>
      {save.isError && save.error !== null && (
        <div className="alert alert-error">{save.error.message}</div>
      )}
      <PatientForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={id === undefined ? 'Create' : 'Save'}
      />
    </section>
  );
};