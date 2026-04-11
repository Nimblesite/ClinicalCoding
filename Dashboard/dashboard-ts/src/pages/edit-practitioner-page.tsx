import { useEffect, useState, type ReactElement, type SyntheticEvent } from 'react';
import { usePractitioner } from '../hooks/use-practitioners';
import { useSavePractitioner } from '../hooks/use-update-practitioner';

interface EditPractitionerPageProps {
  readonly id?: string;
}

export const EditPractitionerPage = ({ id }: EditPractitionerPageProps): ReactElement => {
  const { data, isLoading } = usePractitioner(id);
  const save = useSavePractitioner();
  const [givenName, setGivenName] = useState('');
  const [familyName, setFamilyName] = useState('');
  const [qualification, setQualification] = useState('MD');
  const [specialty, setSpecialty] = useState('');
  const [success, setSuccess] = useState(false);

  // Initialize form state when data loads - using setTimeout to avoid synchronous setState in effect
  useEffect((): (() => void) | undefined => {
    if (data === undefined) {
      return;
    }
    const timer = globalThis.setTimeout((): void => {
      setGivenName(data.NameGiven);
      setFamilyName(data.NameFamily);
      setQualification(data.Qualification);
      setSpecialty(data.Specialty ?? '');
    }, 0);
    return (): void => {
      globalThis.clearTimeout(timer);
    };
  }, [data]);

  if (isLoading) return <div className="page">Loading practitioner…</div>;

  const handleSubmit = (e: SyntheticEvent<HTMLFormElement>): void => {
    e.preventDefault();
    save
      .mutateAsync({
        id,
        practitioner: {
          Identifier: data?.Identifier ?? '',
          NameGiven: givenName,
          NameFamily: familyName,
          Qualification: qualification,
          ...(specialty !== '' ? { Specialty: specialty } : {}),
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
    <section className="page" data-testid="edit-practitioner-page">
      <h2>Edit Practitioner</h2>
      {save.isError ? <div className="alert alert-error">{save.error.message}</div> : null}
      {success ? (
        <div className="alert alert-success" data-testid="edit-practitioner-success">
          Practitioner updated successfully
        </div>
      ) : null}
      <form onSubmit={handleSubmit} className="form">
        <label className="input-label" htmlFor="edit-p-given">
          Given Name
        </label>
        <input
          id="edit-p-given"
          className="input"
          value={givenName}
          onChange={(e) => {
            setGivenName(e.target.value);
          }}
          required
        />
        <label className="input-label" htmlFor="edit-p-family">
          Family Name
        </label>
        <input
          id="edit-p-family"
          className="input"
          value={familyName}
          onChange={(e) => {
            setFamilyName(e.target.value);
          }}
          required
        />
        <label className="input-label" htmlFor="edit-p-qual">
          Qualification
        </label>
        <input
          id="edit-p-qual"
          className="input"
          value={qualification}
          onChange={(e) => {
            setQualification(e.target.value);
          }}
          required
        />
        <label className="input-label" htmlFor="edit-p-spec">
          Specialty
        </label>
        <input
          id="edit-p-spec"
          data-testid="edit-practitioner-specialty"
          className="input"
          value={specialty}
          onChange={(e) => {
            setSpecialty(e.target.value);
          }}
        />
        <button
          type="submit"
          data-testid="save-practitioner"
          className="btn btn-primary"
          disabled={save.isPending}
        >
          {save.isPending ? 'Saving…' : 'Save Changes'}
        </button>
      </form>
    </section>
  );
};
