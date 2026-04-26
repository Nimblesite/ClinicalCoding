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

export const ClinicalCodingPage = (): ReactElement => {
  const [mode, setMode] = useState<Mode>('semantic');
  const [query, setQuery] = useState('');
  const [includeAchi, setIncludeAchi] = useState(false);
  const [expandedKey, setExpandedKey] = useState<string | null>(null);
  const [copiedKey, setCopiedKey] = useState<string | null>(null);

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

  const isBusy =
    (mode === 'semantic' && semantic.isPending) ||
    (mode === 'keyword' && keyword.isFetching) ||
    (mode === 'lookup' && lookup.isFetching);

  const runSearch = (): void => {
    if (query.trim() === '') return;
    if (mode === 'semantic') {
      semantic.mutate({ text: query, includeAchi });
    }
  };

  const handleCopy = (key: string, code: string): void => {
    void navigator.clipboard.writeText(code).then(() => {
      setCopiedKey(key);
      setTimeout(() => {
        setCopiedKey((current) => (current === key ? null : current));
      }, 1500);
    });
  };

  const toggleExpanded = (key: string): void => {
    setExpandedKey((current) => (current === key ? null : key));
  };

  const resultCount = rows.length;
  const resultLabel = mode === 'lookup' ? 'Result' : 'Results';

  return (
    <section className="page clinical-coding">
      <div className="page-header">
        <div>
          <h2 className="welcome-title">Diagnostic Coding Search</h2>
          <p className="page-description">
            Map clinical documentation to ICD-10-AM and ACHI codes.
          </p>
        </div>
      </div>

      <div className="coding-console">
        <div className="mode-tabs" role="tablist">
          <button
            type="button"
            role="tab"
            aria-selected={mode === 'semantic'}
            data-testid="coding-mode-ai"
            className={mode === 'semantic' ? 'active' : ''}
            onClick={() => {
              setMode('semantic');
            }}
          >
            AI Search
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={mode === 'keyword'}
            data-testid="coding-mode-keyword"
            className={mode === 'keyword' ? 'active' : ''}
            onClick={() => {
              setMode('keyword');
            }}
          >
            Keyword Search
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={mode === 'lookup'}
            data-testid="coding-mode-lookup"
            className={mode === 'lookup' ? 'active' : ''}
            onClick={() => {
              setMode('lookup');
            }}
          >
            Code Lookup
          </button>
        </div>

        <div className="search-shell">
          {mode === 'semantic' ? (
            <textarea
              data-testid="coding-search-input"
              className="search-field"
              rows={12}
              placeholder="Describe symptoms or diagnosis, e.g. 'acute myocardial infarction of anterior wall'"
              value={query}
              disabled={isBusy}
              onChange={(e) => {
                setQuery(e.target.value);
              }}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                  e.preventDefault();
                  runSearch();
                }
              }}
            />
          ) : (
            <input
              data-testid="coding-search-input"
              className="search-field"
              type="text"
              placeholder={mode === 'keyword' ? 'e.g. chest pain' : 'e.g. R07.4'}
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
              }}
              onKeyDown={(e) => {
                if (e.key === 'Enter') runSearch();
              }}
            />
          )}
          {mode === 'semantic' && (
            <button
              type="button"
              className="search-submit"
              onClick={runSearch}
              disabled={isBusy || query.trim() === ''}
            >
              {isBusy ? (
                <>
                  <span className="spinner" aria-hidden="true" /> Searching
                </>
              ) : (
                'Search'
              )}
            </button>
          )}
        </div>

        <label className="achi-toggle">
          <input
            type="checkbox"
            data-testid="achi-toggle"
            checked={includeAchi}
            onChange={(e) => {
              setIncludeAchi(e.target.checked);
            }}
          />
          <span>Include ACHI procedure codes</span>
        </label>
      </div>

      <div className="coding-results">
        <div className="coding-results-header">
          <h2>{isBusy ? 'Searching' : `${String(resultCount)} ${resultLabel}`}</h2>
        </div>

        {isBusy ? (
          <div className="coding-loading">
            <span className="spinner spinner-lg" aria-hidden="true" />
            <p>Searching the ICD-10-AM index…</p>
          </div>
        ) : null}

        {!isBusy && resultCount === 0 ? (
          <div className="coding-empty">
            Enter a clinical description and press Search to see matching codes.
          </div>
        ) : null}

        {!isBusy && resultCount > 0 ? (
          <div className="results-list">
            {rows.map((r) => {
              const key = `${r.source}-${r.code}`;
              const expanded = expandedKey === key;
              const copied = copiedKey === key;
              const pct = r.score !== undefined ? Math.round(r.score * 100) : undefined;
              return (
                <article
                  key={key}
                  className={`result-card${expanded ? ' expanded' : ''}`}
                  data-testid="coding-result"
                >
                  <div className="coding-result-content">
                    <div className="coding-result-code-section">
                      <span className={`code-badge${r.source === 'ACHI' ? ' secondary' : ''}`}>
                        {r.code}
                      </span>
                      <span className="code-type-label">{r.source}</span>
                    </div>

                    <div className="coding-result-body">
                      <h3 className="coding-result-title">{r.title}</h3>
                      {r.description !== '' && r.description !== r.title && (
                        <p className="coding-result-description">{r.description}</p>
                      )}
                    </div>

                    <div className="coding-result-actions">
                      {pct !== undefined && (
                        <div className="ai-match-score">
                          <div className="ai-match-row">
                            <span className="ai-match-label">Match</span>
                            <span className="ai-match-value">{pct}%</span>
                          </div>
                          <div className="ai-match-bar">
                            <div className="ai-match-fill" style={{ width: `${String(pct)}%` }} />
                          </div>
                        </div>
                      )}
                      <div className="coding-result-buttons">
                        <button
                          type="button"
                          className="btn-outline"
                          onClick={() => {
                            toggleExpanded(key);
                          }}
                          aria-expanded={expanded}
                        >
                          {expanded ? 'Hide' : 'Details'}
                        </button>
                        <button
                          type="button"
                          className={`btn-outline${copied ? ' copied' : ''}`}
                          data-testid="coding-copy"
                          onClick={() => {
                            handleCopy(key, r.code);
                          }}
                        >
                          {copied ? 'Copied' : 'Copy'}
                        </button>
                      </div>
                    </div>
                  </div>

                  {expanded ? (
                    <div className="coding-result-details" data-testid="coding-detail">
                      <dl>
                        <div>
                          <dt>Code</dt>
                          <dd>{r.code}</dd>
                        </div>
                        <div>
                          <dt>Classification</dt>
                          <dd>{r.source}</dd>
                        </div>
                        <div>
                          <dt>Title</dt>
                          <dd>{r.title}</dd>
                        </div>
                        {r.description !== '' && r.description !== r.title && (
                          <div>
                            <dt>Description</dt>
                            <dd>{r.description}</dd>
                          </div>
                        )}
                        {pct !== undefined && (
                          <div>
                            <dt>Match score</dt>
                            <dd>{String(pct)}%</dd>
                          </div>
                        )}
                      </dl>
                    </div>
                  ) : null}
                </article>
              );
            })}
          </div>
        ) : null}
      </div>
    </section>
  );
};
