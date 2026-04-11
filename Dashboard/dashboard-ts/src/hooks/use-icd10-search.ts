import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { keywordSearchIcd10 } from '../api/icd10';
import type { Icd10Code } from '../types/icd10';

export const useKeywordSearch = (query: string): UseQueryResult<Icd10Code[]> =>
  useQuery({
    queryKey: ['icd10', 'keyword', query],
    queryFn: async () => keywordSearchIcd10(query),
    enabled: query.trim().length >= 2,
  });
