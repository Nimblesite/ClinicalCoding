import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, type ReactElement, type SyntheticEvent } from 'react';
import { createAppointment } from '../api/scheduling';
import type { Appointment } from '../types/fhir';
import { Modal } from './modal';

interface AddAppointmentModalProps {
  readonly open: boolean;
  readonly onClose: () => void;
}

export const AddAppointmentModal = ({ open, onClose }: AddAppointmentModalProps): ReactElement => {
  const qc = useQueryClient();
  const now = new Date();
  const later = new Date(now.getTime() + 30 * 60_000);
  const [serviceType, setServiceType] = useState('Checkup');
  const [patientRef, setPatientRef] = useState('Patient/1');
  const [practitionerRef, setPractitionerRef] = useState('Practitioner/1');
  const [start, setStart] = useState(now.toISOString().slice(0, 16));
  const [endVal, setEndVal] = useState(later.toISOString().slice(0, 16));

  const mutation = useMutation({
    mutationFn: async (a: Appointment) => createAppointment(a),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ['appointments'] });
      onClose();
    },
  });

  const handleSubmit = (e: SyntheticEvent<HTMLFormElement>): void => {
    e.preventDefault();
    mutation.mutate({
      ServiceCategory: 'General',
      ServiceType: serviceType,
      Priority: 'routine',
      PatientReference: patientRef,
      PractitionerReference: practitionerRef,
      StartTime: new Date(start).toISOString(),
      EndTime: new Date(endVal).toISOString(),
    });
  };

  return (
    <Modal open={open} title="Add appointment" onClose={onClose}>
      <form onSubmit={handleSubmit}>
        <label className="input-label" htmlFor="appointment-service-type">
          Service type
        </label>
        <input
          id="appointment-service-type"
          data-testid="appointment-service-type"
          className="input"
          type="text"
          required
          value={serviceType}
          onChange={(e) => {
            setServiceType(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="appointment-patient">
          Patient reference
        </label>
        <input
          id="appointment-patient"
          data-testid="appointment-patient"
          className="input"
          type="text"
          required
          value={patientRef}
          onChange={(e) => {
            setPatientRef(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="appointment-practitioner">
          Practitioner reference
        </label>
        <input
          id="appointment-practitioner"
          data-testid="appointment-practitioner"
          className="input"
          type="text"
          required
          value={practitionerRef}
          onChange={(e) => {
            setPractitionerRef(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="appointment-start">
          Start
        </label>
        <input
          id="appointment-start"
          data-testid="appointment-start"
          className="input"
          type="datetime-local"
          required
          value={start}
          onChange={(e) => {
            setStart(e.target.value);
          }}
        />
        <label className="input-label" htmlFor="appointment-end">
          End
        </label>
        <input
          id="appointment-end"
          data-testid="appointment-end"
          className="input"
          type="datetime-local"
          required
          value={endVal}
          onChange={(e) => {
            setEndVal(e.target.value);
          }}
        />
        {mutation.isError ? (
          <div className="alert alert-error">{mutation.error.message}</div>
        ) : null}
        <button
          type="submit"
          data-testid="submit-appointment"
          className="btn btn-primary"
          disabled={mutation.isPending}
        >
          {mutation.isPending ? 'Creating…' : 'Create appointment'}
        </button>
      </form>
    </Modal>
  );
};
