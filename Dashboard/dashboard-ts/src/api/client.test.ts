import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError, apiFetch } from './client';

const originalFetch = globalThis.fetch;

describe('apiFetch', () => {
  beforeEach(() => {
    globalThis.localStorage.clear();
  });
  afterEach(() => {
    globalThis.fetch = originalFetch;
    vi.restoreAllMocks();
  });

  it('returns parsed JSON on 200', async () => {
    globalThis.fetch = vi
      .fn()
      .mockResolvedValue(
        new Response(JSON.stringify({ ok: true }), {
          status: 200,
          headers: { 'content-type': 'application/json' },
        }),
      );
    const result = await apiFetch<{ ok: boolean }>('http://x/y');
    expect(result.ok).toBe(true);
  });

  it('throws ApiError on non-2xx', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(new Response('boom', { status: 500 }));
    await expect(apiFetch('http://x/y')).rejects.toBeInstanceOf(ApiError);
  });

  it('attaches Authorization header when token present', async () => {
    globalThis.localStorage.setItem('gatekeeper_token', 'tok-123');
    const fetchMock = vi.fn().mockResolvedValue(new Response('{}', { status: 200 }));
    globalThis.fetch = fetchMock;
    await apiFetch('http://x/y');
    const call = fetchMock.mock.calls[0] as [string, RequestInit];
    const headers = call[1].headers as Headers;
    expect(headers.get('Authorization')).toBe('Bearer tok-123');
  });

  it('does NOT attach Content-Type on GET (no body)', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response('{}', { status: 200 }));
    globalThis.fetch = fetchMock;
    await apiFetch('http://x/y');
    const headers = (fetchMock.mock.calls[0] as [string, RequestInit])[1].headers as Headers;
    expect(headers.get('Content-Type')).toBeNull();
  });

  it('attaches Content-Type when body present', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response('{}', { status: 200 }));
    globalThis.fetch = fetchMock;
    await apiFetch('http://x/y', { method: 'POST', body: { a: 1 } });
    const headers = (fetchMock.mock.calls[0] as [string, RequestInit])[1].headers as Headers;
    expect(headers.get('Content-Type')).toBe('application/json');
  });
});
