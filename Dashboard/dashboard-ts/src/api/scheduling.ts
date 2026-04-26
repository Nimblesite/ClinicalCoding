import type { Appointment, Practitioner } from '../types/fhir';
import { apiFetch } from './client';
import { SCHEDULING_API } from './config';

interface AppointmentRequestBody {
  readonly Status: string;
  readonly ServiceCategory: string;
  readonly ServiceType: string;
  readonly ReasonCode: string;
  readonly Priority: string;
  readonly Description: string;
  readonly Start: string;
  readonly End: string;
  readonly PatientReference: string;
  readonly PractitionerReference: string;
  readonly Comment: string;
}

const toRequestBody = (a: Appointment): AppointmentRequestBody => ({
  Status: a.Status ?? 'booked',
  ServiceCategory: a.ServiceCategory,
  ServiceType: a.ServiceType,
  ReasonCode: a.ReasonCode ?? '',
  Priority: a.Priority,
  Description: a.Description ?? '',
  Start: a.StartTime,
  End: a.EndTime,
  PatientReference: a.PatientReference,
  PractitionerReference: a.PractitionerReference,
  Comment: a.Comment ?? '',
});

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
  apiFetch<Appointment>(`${SCHEDULING_API}/Appointment`, {
    method: 'POST',
    body: toRequestBody(a),
  });

export const updateAppointment = async (id: string, a: Appointment): Promise<Appointment> =>
  apiFetch<Appointment>(`${SCHEDULING_API}/Appointment/${id}`, {
    method: 'PUT',
    body: toRequestBody(a),
  });
