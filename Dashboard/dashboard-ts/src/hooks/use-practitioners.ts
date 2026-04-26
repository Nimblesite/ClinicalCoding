import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { getPractitioner, getPractitioners } from '../api/scheduling';
import type { Practitioner } from '../types/fhir';

export const usePractitioners = (): UseQueryResult<Practitioner[]> =>
  useQuery({ queryKey: ['practitioners'], queryFn: getPractitioners });

export const usePractitioner = (id: string | undefined): UseQueryResult<Practitioner> =>
  useQuery({
    queryKey: ['practitioners', id],
    queryFn: async () => getPractitioner(id ?? ''),
    enabled: id !== undefined,
  });
