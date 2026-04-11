import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { getPatient, getPatients } from '../api/clinical';
import type { Patient } from '../types/fhir';

export const usePatients = (): UseQueryResult<Patient[]> =>
  useQuery({ queryKey: ['patients'], queryFn: getPatients });

export const usePatient = (id: string | undefined): UseQueryResult<Patient> =>
  useQuery({
    queryKey: ['patients', id],
    queryFn: async () => getPatient(id ?? ''),
    enabled: id !== undefined,
  });
