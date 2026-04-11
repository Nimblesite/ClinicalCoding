import { useState, type ReactElement } from 'react';
import { useIcd10Lookup } from '../hooks/use-icd10-lookup';
import { useKeywordSearch } from '../hooks/use-icd10-search';
import { useSemanticSearch } from '../hooks/use-semantic-search';
import type { Icd10Code, SemanticSearchResult } from '../types/icd10';

type Mode = 'semantic' | 'keyword' | 'lookup';

interface ResultRow {
  readonly code: string;
  readonly title: string;
  readonly description: string;
  readonly source: string;
  readonly score?: number;
}

const toRowsFromIcd10 = (codes: Icd10Code[]): ResultRow[] =>
  codes.map((c) => ({
    code: c.Code,
    title: c.Title,
    description: c.Description,
    source: 'ICD-10-AM',
  }));

const toRowsFromSemantic = (results: SemanticSearchResult[]): ResultRow[] =>
  results.map((r) => ({
    code: r.code,
    title: r.title,
    description: r.description,
    source: r.source,
    score: r.score,
  }));

const copyToClipboard = async (value: string): Promise<void> => {
  await navigator.clipboard.writeText(value);
};

export const ClinicalCodingPage = (): ReactElement => {
  const [mode, setMode] = useState<Mode>('semantic');
  const [query, setQuery] = useState('');
  const [includeAchi, setIncludeAchi] = useState(false);
  const [selected, setSelected] = useState<ResultRow | null>(null);

  const keyword = useKeywordSearch(mode === 'keyword' ? query : '');
  const lookup = useIcd10Lookup(mode === 'lookup' ? query : '');
  const semantic = useSemanticSearch();

  const rows: ResultRow[] = ((): ResultRow[] => {
    if (mode === 'semantic') {
      return semantic.data !== undefined ? toRowsFromSemantic(semantic.data) : [];
    }
    if (mode === 'keyword') {
      return keyword.data !== undefined ? toRowsFromIcd10(keyword.data) : [];
    }
    return lookup.data !== undefined ? toRowsFromIcd10([lookup.data]) : [];
  })();

  const handleRun = (): void => {
    if (mode === 'semantic') {
      semantic.mutate({ text: query, includeAchi });
    }
  };

  return (
    <section className="page clinical-coding">
      <div className="page-header">
        <div>
          <h2 className="welcome-title">Diagnostic Coding Search</h2>
          <p className="page-description">Map clinical documentation to precise codes.</p>
        </div>
      </div>

      <div className="coding-console">
        <div className="mode-tabs">
          <button
            type="button"
            className={mode === 'semantic' ? 'active' : ''}
            onClick={() => {
              setMode('semantic');
            }}
          >
            AI Search
          </button>
          <button
            type="button"
            className={mode === 'keyword' ? 'active' : ''}
            onClick={() => {
              setMode('keyword');
            }}
          >
            Keyword Search
          </button>
          <button
            type="button"
            className={mode === 'lookup' ? 'active' : ''}
            onClick={() => {
              setMode('lookup');
            }}
          >
            Code Lookup
          </button>
        </div>

        <div className="search-input-row">
          {mode === 'semantic' ? (
            <textarea
              className="input"
              rows={3}
              placeholder="Describe symptoms or diagnosis…"
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
              }}
            />
          ) : (
            <input
              className="input"
              type="text"
              placeholder={mode === 'keyword' ? 'e.g. chest pain' : 'e.g. R07.4'}
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
              }}
            />
          )}
          {mode === 'semantic' && (
            <button type="button" className="btn btn-primary" onClick={handleRun}>
              Analyze
            </button>
          )}
        </div>

        <label className="input-label">
          <input
            type="checkbox"
            checked={includeAchi}
            onChange={(e) => {
              setIncludeAchi(e.target.checked);
            }}
          />{' '}
          Include ACHI procedure codes
        </label>
      </div>

      <div className="coding-results">
        <h3>
          {rows.length} {mode} results
        </h3>
        <ul className="results-list">
          {rows.map((r) => (
            <li
              key={`${r.source}-${r.code}`}
              className={selected?.code === r.code ? 'selected' : ''}
            >
              <button
                type="button"
                onClick={() => {
                  setSelected(r);
                }}
              >
                <span className="result-code">{r.code}</span>
                <span className="result-title">{r.title}</span>
                {r.score !== undefined && (
                  <span className="result-score">{Math.round(r.score * 100)}%</span>
                )}
              </button>
            </li>
          ))}
        </ul>

        {selected !== null && (
          <aside className="code-detail-panel">
            <h4>{selected.code}</h4>
            <p className="detail-title">{selected.title}</p>
            <p className="detail-source">{selected.source}</p>
            <p>{selected.description}</p>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => {
                void copyToClipboard(selected.code);
              }}
            >
              Copy code
            </button>
          </aside>
        )}
      </div>
    </section>
  );
};