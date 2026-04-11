import type { Patient } from '../types/fhir';
import { apiFetch } from './client';
import { CLINICAL_API } from './config';

export const getPatients = async (): Promise<Patient[]> =>
  apiFetch<Patient[]>(`${CLINICAL_API}/fhir/Patient`);

export const getPatient = async (id: string): Promise<Patient> =>
  apiFetch<Patient>(`${CLINICAL_API}/fhir/Patient/${id}`);

export const createPatient = async (patient: Patient): Promise<Patient> =>
  apiFetch<Patient>(`${CLINICAL_API}/fhir/Patient/`, { method: 'POST', body: patient });

export const updatePatient = async (id: string, patient: Patient): Promise<Patient> =>
  apiFetch<Patient>(`${CLINICAL_API}/fhir/Patient/${id}`, { method: 'PUT', body: patient });

export const deletePatient = async (id: string): Promise<void> =>
  apiFetch<void>(`${CLINICAL_API}/fhir/Patient/${id}`, { method: 'DELETE' });
