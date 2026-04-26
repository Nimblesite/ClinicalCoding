import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { lookupIcd10 } from '../api/icd10';
import type { Icd10Code } from '../types/icd10';

export const useIcd10Lookup = (code: string): UseQueryResult<Icd10Code> =>
  useQuery({
    queryKey: ['icd10', 'lookup', code],
    queryFn: async () => lookupIcd10(code),
    enabled: code.trim().length >= 2,
  });
