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

interface ResultCardProps {
  readonly copied: boolean;
  readonly expanded: boolean;
  readonly onCopy: (key: string, code: string) => void;
  readonly onToggle: (key: string) => void;
  readonly row: ResultRow;
  readonly rowKey: string;
}

interface ModeOption {
  readonly label: string;
  readonly mode: Mode;
  readonly testId: string;
}

interface ModeTabsProps {
  readonly mode: Mode;
  readonly onModeChange: (mode: Mode) => void;
}

interface SearchControlsProps {
  readonly includeAchi: boolean;
  readonly isBusy: boolean;
  readonly mode: Mode;
  readonly onIncludeAchiChange: (includeAchi: boolean) => void;
  readonly onQueryChange: (query: string) => void;
  readonly onSearch: () => void;
  readonly query: string;
}

interface ResultsPanelProps {
  readonly copiedKey: string | null;
  readonly expandedKey: string | null;
  readonly isBusy: boolean;
  readonly onCopy: (key: string, code: string) => void;
  readonly onToggle: (key: string) => void;
  readonly resultLabel: string;
  readonly rows: ResultRow[];
}

const MODE_OPTIONS: readonly ModeOption[] = [
  { mode: 'semantic', label: 'AI Search', testId: 'coding-mode-ai' },
  { mode: 'keyword', label: 'Keyword Search', testId: 'coding-mode-keyword' },
  { mode: 'lookup', label: 'Code Lookup', testId: 'coding-mode-lookup' },
];

const toRowsFromIcd10 = (codes: Icd10Code[]): ResultRow[] =>
  codes.map((c) => ({
    code: c.Code,
    title: c.Title ?? c.ShortDescription ?? c.Code,
    description: c.Description ?? c.LongDescription ?? c.ShortDescription ?? '',
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

const getRows = (
  mode: Mode,
  semanticRows: SemanticSearchResult[] | undefined,
  keywordRows: Icd10Code[] | undefined,
  lookupRow: Icd10Code | undefined,
): ResultRow[] => {
  if (mode === 'semantic') {
    return semanticRows === undefined ? [] : toRowsFromSemantic(semanticRows);
  }
  if (mode === 'keyword') {
    return keywordRows === undefined ? [] : toRowsFromIcd10(keywordRows);
  }
  return lookupRow === undefined ? [] : toRowsFromIcd10([lookupRow]);
};

const ResultCard = ({
  copied,
  expanded,
  onCopy,
  onToggle,
  row,
  rowKey,
}: ResultCardProps): ReactElement => {
  const detailDescription = row.description === '' ? row.title : row.description;
  const hasPreviewDescription = row.description !== '' && row.description !== row.title;
  const scorePercent = row.score === undefined ? undefined : Math.round(row.score * 100);
  const scoreLabel = scorePercent === undefined ? '' : `${String(scorePercent)}%`;

  return (
    <article className={`result-card${expanded ? ' expanded' : ''}`} data-testid="coding-result">
      <div className="coding-result-content">
        <div className="coding-result-code-section">
          <span className={`code-badge${row.source === 'ACHI' ? ' secondary' : ''}`}>
            {row.code}
          </span>
          <span className="code-type-label">{row.source}</span>
        </div>

        <div className="coding-result-body">
          <h3 className="coding-result-title">{row.title}</h3>
          {hasPreviewDescription ? (
            <p className="coding-result-description">{row.description}</p>
          ) : null}
        </div>

        <div className="coding-result-actions">
          {scorePercent === undefined ? null : (
            <div className="ai-match-score">
              <div className="ai-match-row">
                <span className="ai-match-label">Match</span>
                <span className="ai-match-value">{scoreLabel}</span>
              </div>
              <div className="ai-match-bar">
                <div className="ai-match-fill" style={{ width: scoreLabel }} />
              </div>
            </div>
          )}
          <div className="coding-result-buttons">
            <button
              type="button"
              className="btn-outline"
              onClick={() => {
                onToggle(rowKey);
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
                onCopy(rowKey, row.code);
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
              <dd>{row.code}</dd>
            </div>
            <div>
              <dt>Classification</dt>
              <dd>{row.source}</dd>
            </div>
            <div>
              <dt>Title</dt>
              <dd>{row.title}</dd>
            </div>
            <div>
              <dt>Description</dt>
              <dd>{detailDescription}</dd>
            </div>
            {scorePercent === undefined ? null : (
              <div>
                <dt>Match score</dt>
                <dd>{scoreLabel}</dd>
              </div>
            )}
          </dl>
        </div>
      ) : null}
    </article>
  );
};

const ModeTabs = ({ mode, onModeChange }: ModeTabsProps): ReactElement => (
  <div className="mode-tabs" role="tablist">
    {MODE_OPTIONS.map((option) => (
      <button
        key={option.mode}
        type="button"
        role="tab"
        aria-selected={mode === option.mode}
        data-testid={option.testId}
        className={mode === option.mode ? 'active' : ''}
        onClick={() => {
          onModeChange(option.mode);
        }}
      >
        {option.label}
      </button>
    ))}
  </div>
);

const SearchControls = ({
  includeAchi,
  isBusy,
  mode,
  onIncludeAchiChange,
  onQueryChange,
  onSearch,
  query,
}: SearchControlsProps): ReactElement => (
  <>
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
            onQueryChange(e.target.value);
          }}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault();
              onSearch();
            }
          }}
        />
      ) : (
        <input
          data-testid="coding-search-input"
          className="search-field"
          type="text"
          placeholder={
            mode === 'keyword'
              ? 'Search by code or diagnosis, e.g. chest pain'
              : 'Enter exact ICD-10 code, e.g. R07.4'
          }
          value={query}
          onChange={(e) => {
            onQueryChange(e.target.value);
          }}
          onKeyDown={(e) => {
            if (e.key === 'Enter') onSearch();
          }}
        />
      )}
      <button
        type="button"
        className="search-submit"
        onClick={onSearch}
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
    </div>

    <label className="achi-toggle">
      <input
        type="checkbox"
        data-testid="achi-toggle"
        checked={includeAchi}
        onChange={(e) => {
          onIncludeAchiChange(e.target.checked);
        }}
      />
      <span>Include ACHI procedure codes</span>
    </label>
  </>
);

const ResultsPanel = ({
  copiedKey,
  expandedKey,
  isBusy,
  onCopy,
  onToggle,
  resultLabel,
  rows,
}: ResultsPanelProps): ReactElement => {
  const resultCount = rows.length;

  return (
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
            return (
              <ResultCard
                key={key}
                copied={copiedKey === key}
                expanded={expandedKey === key}
                onCopy={onCopy}
                onToggle={onToggle}
                row={r}
                rowKey={key}
              />
            );
          })}
        </div>
      ) : null}
    </div>
  );
};

export const ClinicalCodingPage = (): ReactElement => {
  const [mode, setMode] = useState<Mode>('semantic');
  const [query, setQuery] = useState('');
  const [includeAchi, setIncludeAchi] = useState(false);
  const [expandedKey, setExpandedKey] = useState<string | null>(null);
  const [copiedKey, setCopiedKey] = useState<string | null>(null);

  const keyword = useKeywordSearch(mode === 'keyword' ? query : '');
  const lookup = useIcd10Lookup(mode === 'lookup' ? query : '');
  const semantic = useSemanticSearch();

  const rows = getRows(mode, semantic.data, keyword.data, lookup.data);

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

  const resultLabel = mode === 'lookup' ? 'Result' : 'Results';

  return (
    <section className="page clinical-coding clinical-coding-page">
      <div className="page-header">
        <div>
          <h2 className="welcome-title">Diagnostic Coding Search</h2>
          <p className="page-description">
            Map clinical documentation to ICD-10-AM and ACHI codes.
          </p>
        </div>
      </div>

      <div className="coding-console">
        <ModeTabs mode={mode} onModeChange={setMode} />
        <SearchControls
          includeAchi={includeAchi}
          isBusy={isBusy}
          mode={mode}
          onIncludeAchiChange={setIncludeAchi}
          onQueryChange={setQuery}
          onSearch={runSearch}
          query={query}
        />
      </div>
      <ResultsPanel
        copiedKey={copiedKey}
        expandedKey={expandedKey}
        isBusy={isBusy}
        onCopy={handleCopy}
        onToggle={toggleExpanded}
        resultLabel={resultLabel}
        rows={rows}
      />
    </section>
  );
};
