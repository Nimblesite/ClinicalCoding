import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, type ReactElement, type SyntheticEvent } from 'react';
import { createPractitioner } from '../api/scheduling';
import type { Practitioner } from '../types/fhir';
import { Modal } from './modal';

interface AddPractitionerModalProps {
  readonly open: boolean;
  readonly onClose: () => void;
}

export const AddPractitionerModal = ({
  open,
  onClose,
}: AddPractitionerModalProps): ReactElement => {
  const qc = useQueryClient();
  const [identifier, setIdentifier] = useState('');
  const [givenName, setGivenName] = useState('');
  const [familyName, setFamilyName] = useState('');
  const [qualification, setQualification] = useState('MD');
  const [specialty, setSpecialty] = useState('');

  const mutation = useMutation({
    mutationFn: async (p: Practitioner) => createPractitioner(p),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['practitioners'] });
      setIdentifier('');
      setGivenName('');
      setFamilyName('');
      setQualification('MD');
      setSpecialty('');
      onClose();
    },
  });

  const handleSubmit = (e: SyntheticEvent<HTMLFormElement>): void => {
    e.preventDefault();
    mutation.mutate({
      Identifier: identifier,
      NameGiven: givenName,
      NameFamily: familyName,
      Qualification: qualification,
      ...(specialty !== '' ? { Specialty: specialty } : {}),
    });
  };

  return (
    <Modal open={open} title="Add practitioner" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <label className="input-label" htmlFor="practitioner-identifier">
          Identifier
        </label>
        <input
          id="practitioner-identifier"
          data-testid="practitioner-identifier"
          className="input"
          type="text"
          required
          value={identifier}
          onChange={(e) => {
            setIdentifier(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="practitioner-given-name">
          Given name
        </label>
        <input
          id="practitioner-given-name"
          data-testid="practitioner-given-name"
          className="input"
          type="text"
          required
          value={givenName}
          onChange={(e) => {
            setGivenName(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="practitioner-family-name">
          Family name
        </label>
        <input
          id="practitioner-family-name"
          data-testid="practitioner-family-name"
          className="input"
          type="text"
          required
          value={familyName}
          onChange={(e) => {
            setFamilyName(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="practitioner-qualification">
          Qualification
        </label>
        <input
          id="practitioner-qualification"
          className="input"
          type="text"
          required
          value={qualification}
          onChange={(e) => {
            setQualification(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="practitioner-specialty">
          Specialty
        </label>
        <input
          id="practitioner-specialty"
          data-testid="practitioner-specialty"
          className="input"
          type="text"
          value={specialty}
          onChange={(e) => {
            setSpecialty(e.target.value);
          }}
        />
        {mutation.isError ? (
          <div className="alert alert-error">{mutation.error.message}</div>
        ) : null}
        <button
          type="submit"
          data-testid="submit-practitioner"
          className="btn btn-primary"
          disabled={mutation.isPending}
        >
          {mutation.isPending ? 'Creating…' : 'Create practitioner'}
        </button>
      </form>
    </Modal>
  );
};
