import { zodResolver } from '@hookform/resolvers/zod';
import type { ReactElement } from 'react';
import { useForm, type SubmitHandler } from 'react-hook-form';
import type { Patient } from '../types/fhir';
import { patientSchema, type PatientFormValues } from './schemas';

interface PatientFormProps {
  readonly initial?: Patient;
  readonly onSubmit: (values: PatientFormValues) => Promise<void> | void;
  readonly onCancel?: () => void;
  readonly submitLabel: string;
}

export const PatientForm = ({
  initial,
  onSubmit,
  onCancel,
  submitLabel,
}: PatientFormProps): ReactElement => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isDirty },
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
      // eslint-disable-next-line @typescript-eslint/no-misused-promises -- RHF handleSubmit returns a promise; onSubmit attribute expects void
      onSubmit={handleSubmit(submit)}
      className="patient-editor-form"
    >
      <div className="editor-grid">
        <section className="editor-card editor-card--primary">
          <header className="editor-card-header">
            <div className="editor-card-icon">
              <span className="material-symbols-outlined">badge</span>
            </div>
            <h3>Identity &amp; Personal Details</h3>
          </header>

          <div className="field-grid field-grid--two">
            <div className="field">
              <label className="field-label" htmlFor="family">
                Family Name
              </label>
              <input
                id="family"
                data-testid="edit-family-name"
                className="field-input"
                {...register('FamilyName')}
              />
              {errors.FamilyName !== undefined && (
                <p className="form-error">{errors.FamilyName.message}</p>
              )}
            </div>

            <div className="field">
              <label className="field-label" htmlFor="given">
                Given Name(s)
              </label>
              <input
                id="given"
                data-testid="edit-given-name"
                className="field-input"
                {...register('GivenName')}
              />
              {errors.GivenName !== undefined && (
                <p className="form-error">{errors.GivenName.message}</p>
              )}
            </div>

            <div className="field">
              <label className="field-label" htmlFor="gender">
                Administrative Gender
              </label>
              <select id="gender" className="field-input" {...register('Gender')}>
                <option value="female">Female</option>
                <option value="male">Male</option>
                <option value="other">Other</option>
              </select>
            </div>

            <div className="field">
              <label className="field-label" htmlFor="birthdate">
                Birth Date
              </label>
              <input
                id="birthdate"
                type="date"
                className="field-input"
                {...register('BirthDate')}
              />
            </div>
          </div>

          <div className="editor-card-section">
            <label className="field-toggle">
              <input type="checkbox" {...register('Active')} />
              <span>Active patient record</span>
            </label>
          </div>
        </section>

        <aside className="editor-side">
          <div className="editor-card editor-card--soft">
            <h3 className="editor-side-title">Record Status</h3>
            <div className="status-row">
              <span className="status-dot" />
              <div>
                <p className="status-label">FHIR Resource</p>
                <p className="status-value">Validated v4.0.1</p>
              </div>
            </div>
          </div>

          <div className="editor-card editor-card--soft">
            <h3 className="editor-side-title">Verification</h3>
            <div className="verify-row">
              <div className="verify-marker verify-marker--tertiary" />
              <div>
                <p className="verify-label">Resource Completeness</p>
                <p className="verify-desc">All required FHIR elements present.</p>
              </div>
            </div>
            <div className="verify-row">
              <div className="verify-marker verify-marker--secondary" />
              <div>
                <p className="verify-label">Identity Fields</p>
                <p className="verify-desc">Given and family name captured.</p>
              </div>
            </div>
          </div>
        </aside>
      </div>

      <footer className="editor-actions">
        {onCancel !== undefined && (
          <button type="button" className="btn btn-ghost" onClick={onCancel}>
            Cancel
          </button>
        )}
        <button
          type="submit"
          data-testid="save-patient"
          className="btn btn-primary btn-pill"
          disabled={isSubmitting || !isDirty}
        >
          {isSubmitting ? 'Saving…' : submitLabel}
        </button>
      </footer>
    </form>
  );
};
