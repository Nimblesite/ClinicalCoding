export interface Icd10Chapter {
  readonly Id: string;
  readonly Code: string;
  readonly Title: string;
}

export interface Icd10Block {
  readonly Id: string;
  readonly Code: string;
  readonly Title: string;
  readonly ChapterCode: string;
}

export interface Icd10Code {
  readonly Id: string;
  readonly Code: string;
  readonly Title?: string;
  readonly Description?: string;
  readonly ShortDescription?: string;
  readonly LongDescription?: string;
  readonly BlockCode: string;
}

export interface AchiCode {
  readonly Id: string;
  readonly Code: string;
  readonly Description: string;
}

export interface SemanticSearchResult {
  readonly code: string;
  readonly title: string;
  readonly description: string;
  readonly score: number;
  readonly source: 'ICD-10-AM' | 'ACHI';
}
