import { useMutation, type UseMutationResult } from '@tanstack/react-query';
import { semanticSearch } from '../api/icd10';
import type { SemanticSearchResult } from '../types/icd10';

interface SemanticArgs {
  readonly text: string;
  readonly includeAchi: boolean;
}

export const useSemanticSearch = (): UseMutationResult<
  SemanticSearchResult[],
  Error,
  SemanticArgs
> =>
  useMutation({
    mutationFn: async ({ text, includeAchi }: SemanticArgs) => semanticSearch(text, includeAchi),
  });
