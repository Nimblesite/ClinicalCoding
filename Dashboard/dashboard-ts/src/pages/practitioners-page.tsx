import { useState, type ReactElement } from 'react';
import { AddPractitionerModal } from '../components/add-practitioner-modal';
import { usePractitioners } from '../hooks/use-practitioners';

export const PractitionersPage = (): ReactElement => {
  const { data, isLoading, isError, error } = usePractitioners();
  const [modalOpen, setModalOpen] = useState(false);
  if (isLoading) return <div className="page">Loading practitioners…</div>;
  if (isError) return <div className="page alert alert-error">{error.message}</div>;
  const rows = data ?? [];
  return (
    <section className="page">
      <div className="page-header">
        <h2>Practitioners</h2>
        <button
          type="button"
          data-testid="add-practitioner-btn"
          className="btn btn-primary"
          onClick={() => {
            setModalOpen(true);
          }}
        >
          Add Practitioner
        </button>
      </div>
      {rows.length === 0 ? (
        <div className="empty-state">No practitioners yet.</div>
      ) : (
        <div className="practitioner-grid">
          {rows.map((p) => (
            <div
              key={p.Id ?? p.Identifier}
              className="practitioner-card"
              data-testid="practitioner-row"
            >
              <div className="avatar">{p.NameGiven.slice(0, 1)}</div>
              <div className="practitioner-info">
                <p className="practitioner-name">
                  {p.NameGiven} {p.NameFamily}
                </p>
                <p className="practitioner-meta">
                  {p.Qualification} · {p.Specialty ?? ''}
                </p>
                <p className="practitioner-contact">{p.TelecomEmail ?? ''}</p>
              </div>
              {p.Id !== undefined && (
                <a
                  className="btn btn-secondary"
                  data-testid={`edit-practitioner-${p.Id}`}
                  href={`#practitioners/edit/${p.Id}`}
                >
                  Edit
                </a>
              )}
            </div>
          ))}
        </div>
      )}
      <AddPractitionerModal
        open={modalOpen}
        onClose={() => {
          setModalOpen(false);
        }}
      />
    </section>
  );
};
