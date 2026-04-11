import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { getAppointment, getAppointments } from '../api/scheduling';
import type { Appointment } from '../types/fhir';

export const useAppointments = (): UseQueryResult<Appointment[]> =>
  useQuery({ queryKey: ['appointments'], queryFn: getAppointments });

export const useAppointment = (id: string | undefined): UseQueryResult<Appointment> =>
  useQuery({
    queryKey: ['appointments', id],
    queryFn: async () => getAppointment(id ?? ''),
    enabled: id !== undefined,
  });
