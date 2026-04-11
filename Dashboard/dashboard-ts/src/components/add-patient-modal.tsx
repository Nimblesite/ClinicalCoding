import { useState, type ReactElement, type FormEvent } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createPatient } from '../api/clinical';
import type { Patient } from '../types/fhir';
import { Modal } from './modal';

interface AddPatientModalProps {
  readonly open: boolean;
  readonly onClose: () => void;
}

export const AddPatientModal = ({ open, onClose }: AddPatientModalProps): ReactElement => {
  const qc = useQueryClient();
  const [givenName, setGivenName] = useState('');
  const [familyName, setFamilyName] = useState('');
  const [gender, setGender] = useState<'male' | 'female' | 'other'>('other');
  const mutation = useMutation({
    mutationFn: async (patient: Patient) => createPatient(patient),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['patients'] });
      setGivenName('');
      setFamilyName('');
      setGender('other');
      onClose();
    },
  });

  const handleSubmit = (e: FormEvent<HTMLFormElement>): void => {
    e.preventDefault();
    mutation.mutate({
      GivenName: givenName,
      FamilyName: familyName,
      Gender: gender,
      Active: true,
    });
  };

  return (
    <Modal open={open} title="Add patient" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <label className="input-label" htmlFor="patient-given-name">
          Given name
        </label>
        <input
          id="patient-given-name"
          data-testid="patient-given-name"
          className="input"
          type="text"
          required
          value={givenName}
          onChange={(e) => {
            setGivenName(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="patient-family-name">
          Family name
        </label>
        <input
          id="patient-family-name"
          data-testid="patient-family-name"
          className="input"
          type="text"
          required
          value={familyName}
          onChange={(e) => {
            setFamilyName(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="patient-gender">
          Gender
        </label>
        <select
          id="patient-gender"
          data-testid="patient-gender"
          className="input"
          value={gender}
          onChange={(e) => {
            const v = e.target.value;
            if (v === 'male' || v === 'female' || v === 'other') setGender(v);
          }}
        >
          <option value="male">Male</option>
          <option value="female">Female</option>
          <option value="other">Other</option>
        </select>
        {mutation.isError && (
          <div className="alert alert-error">{mutation.error.message}</div>
        )}
        <button
          type="submit"
          data-testid="submit-patient"
          className="btn btn-primary"
          disabled={mutation.isPending}
        >
          {mutation.isPending ? 'Creating…' : 'Create patient'}
        </button>
      </form>
    </Modal>
  );
};
