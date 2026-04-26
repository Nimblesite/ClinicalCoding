import { useState, type ReactElement } from 'react';
import { PatientForm } from '../forms/patient-form';
import type { PatientFormValues } from '../forms/schemas';
import { usePatient } from '../hooks/use-patients';
import { useSavePatient } from '../hooks/use-update-patient';

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
      })
      .catch(() => {
        // Error surfaced via save.isError
      });
  };

  const isEdit = id !== undefined;
  const title = isEdit ? 'Edit Demographics' : 'New Patient';
  const subtitle = isEdit
    ? "Update the patient's identity and contact resources following HL7 FHIR standards."
    : 'Create a new patient record following HL7 FHIR standards.';

  return (
    <section className="page editor-page" data-testid="edit-patient-page">
      <header className="editor-page-header">
        <div>
          <span className="editor-tag">Patient Record</span>
          <h1 className="editor-title">{title}</h1>
          <p className="editor-subtitle">{subtitle}</p>
        </div>
      </header>

      {save.isError ? (
        <div className="alert alert-error" role="alert">
          {save.error.message}
        </div>
      ) : null}
      {success ? (
        <div className="alert alert-success" data-testid="edit-success" role="status">
          Patient record saved.
        </div>
      ) : null}

      <PatientForm
        {...(data !== undefined ? { initial: data } : {})}
        onSubmit={handleSubmit}
        submitLabel={isEdit ? 'Save Changes' : 'Create Patient'}
      />
    </section>
  );
};
