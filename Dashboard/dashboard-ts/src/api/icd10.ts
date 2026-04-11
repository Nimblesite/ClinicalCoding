import type { AchiCode, Icd10Chapter, Icd10Code, SemanticSearchResult } from '../types/icd10';
import { apiFetch } from './client';
import { ICD10_API } from './config';

export const getChapters = async (): Promise<Icd10Chapter[]> =>
  apiFetch<Icd10Chapter[]>(`${ICD10_API}/api/icd10/chapters`);

export const lookupIcd10 = async (code: string): Promise<Icd10Code> =>
  apiFetch<Icd10Code>(`${ICD10_API}/api/icd10/code/${encodeURIComponent(code)}`);

export const lookupAchi = async (code: string): Promise<AchiCode> =>
  apiFetch<AchiCode>(`${ICD10_API}/api/achi/code/${encodeURIComponent(code)}`);

export const keywordSearchIcd10 = async (q: string): Promise<Icd10Code[]> =>
  apiFetch<Icd10Code[]>(`${ICD10_API}/api/icd10/search?q=${encodeURIComponent(q)}`);

export const semanticSearch = async (
  text: string,
  includeAchi: boolean,
): Promise<SemanticSearchResult[]> =>
  apiFetch<SemanticSearchResult[]>(`${ICD10_API}/api/icd10/semantic-search`, {
    method: 'POST',
    body: { text, includeAchi },
  });
