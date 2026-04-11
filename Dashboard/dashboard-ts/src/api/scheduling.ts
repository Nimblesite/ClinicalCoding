import type { Appointment, Practitioner } from '../types/fhir';
import { apiFetch } from './client';
import { SCHEDULING_API } from './config';

export const getPractitioners = async (): Promise<Practitioner[]> =>
  apiFetch<Practitioner[]>(`${SCHEDULING_API}/Practitioner`);

export const getPractitioner = async (id: string): Promise<Practitioner> =>
  apiFetch<Practitioner>(`${SCHEDULING_API}/Practitioner/${id}`);

export const createPractitioner = async (p: Practitioner): Promise<Practitioner> =>
  apiFetch<Practitioner>(`${SCHEDULING_API}/Practitioner`, { method: 'POST', body: p });

export const getAppointments = async (): Promise<Appointment[]> =>
  apiFetch<Appointment[]>(`${SCHEDULING_API}/Appointment`);

export const getAppointment = async (id: string): Promise<Appointment> =>
  apiFetch<Appointment>(`${SCHEDULING_API}/Appointment/${id}`);

export const createAppointment = async (a: Appointment): Promise<Appointment> =>
  apiFetch<Appointment>(`${SCHEDULING_API}/Appointment`, { method: 'POST', body: a });

export const updateAppointment = async (id: string, a: Appointment): Promise<Appointment> =>
  apiFetch<Appointment>(`${SCHEDULING_API}/Appointment/${id}`, { method: 'PUT', body: a });
