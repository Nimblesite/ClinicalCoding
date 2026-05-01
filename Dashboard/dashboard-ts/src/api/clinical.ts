import type { Patient } from '../types/fhir';
import { apiFetch } from './client';
import { CLINICAL_API } from './config';

type ClinicalPatientResponse = Omit<Patient, 'Active'> & {
  readonly Active: boolean | number;
};

const normalizePatient = (patient: ClinicalPatientResponse): Patient => ({
  ...patient,
  Active: patient.Active === true || patient.Active === 1,
});

export const getPatients = async (): Promise<Patient[]> =>
  apiFetch<ClinicalPatientResponse[]>(`${CLINICAL_API}/fhir/Patient`).then((patients) =>
    patients.map((patient) => normalizePatient(patient)),
  );

export const getPatient = async (id: string): Promise<Patient> =>
  apiFetch<ClinicalPatientResponse>(`${CLINICAL_API}/fhir/Patient/${id}`).then(normalizePatient);

export const createPatient = async (patient: Patient): Promise<Patient> =>
  apiFetch<ClinicalPatientResponse>(`${CLINICAL_API}/fhir/Patient/`, {
    method: 'POST',
    body: patient,
  }).then(normalizePatient);

export const updatePatient = async (id: string, patient: Patient): Promise<Patient> =>
  apiFetch<ClinicalPatientResponse>(`${CLINICAL_API}/fhir/Patient/${id}`, {
    method: 'PUT',
    body: patient,
  }).then(normalizePatient);

export const deletePatient = async (id: string): Promise<unknown> =>
  apiFetch<unknown>(`${CLINICAL_API}/fhir/Patient/${id}`, { method: 'DELETE' });
