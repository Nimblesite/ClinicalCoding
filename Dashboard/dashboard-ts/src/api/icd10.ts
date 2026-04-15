import type { AchiCode, Icd10Chapter, Icd10Code, SemanticSearchResult } from '../types/icd10';
import { apiFetch } from './client';
import { ICD10_API } from './config';

interface RagSearchResponseItem {
  readonly Code: string;
  readonly Description: string;
  readonly LongDescription?: string;
  readonly Confidence: number;
  readonly CodeType: string;
}

interface RagSearchResponse {
  readonly Results: readonly RagSearchResponseItem[];
}

export const getChapters = async (): Promise<Icd10Chapter[]> =>
  apiFetch<Icd10Chapter[]>(`${ICD10_API}/api/icd10/chapters`);

export const lookupIcd10 = async (code: string): Promise<Icd10Code> =>
  apiFetch<Icd10Code>(`${ICD10_API}/api/icd10/codes/${encodeURIComponent(code)}`);

export const lookupAchi = async (code: string): Promise<AchiCode> =>
  apiFetch<AchiCode>(`${ICD10_API}/api/achi/codes/${encodeURIComponent(code)}`);

export const keywordSearchIcd10 = async (q: string): Promise<Icd10Code[]> =>
  apiFetch<Icd10Code[]>(`${ICD10_API}/api/icd10/codes?q=${encodeURIComponent(q)}`);

export const semanticSearch = async (
  text: string,
  includeAchi: boolean,
): Promise<SemanticSearchResult[]> => {
  const response = await apiFetch<RagSearchResponse>(`${ICD10_API}/api/search`, {
    method: 'POST',
    body: { Query: text, Limit: 20, IncludeAchi: includeAchi },
  });
  return response.Results.map(
    (r): SemanticSearchResult => ({
      code: r.Code,
      title: r.Description,
      description: r.LongDescription ?? r.Description,
      score: r.Confidence,
      source: r.CodeType === 'ACHI' ? 'ACHI' : 'ICD-10-AM',
    }),
  );
};
