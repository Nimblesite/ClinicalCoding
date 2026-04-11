import { clearSession, getToken } from '../auth/auth-storage';
import { logger } from '../lib/logger';

export class ApiError extends Error {
  public readonly status: number;
  public readonly url: string;
  public readonly body: string;

  public constructor(message: string, status: number, url: string, body: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.url = url;
    this.body = body;
  }
}

interface ApiFetchInit {
  readonly method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  readonly body?: unknown;
  readonly headers?: Record<string, string>;
  readonly signal?: AbortSignal;
}

const buildHeaders = (init: ApiFetchInit): Headers => {
  const headers = new Headers();
  headers.set('Accept', 'application/json');
  if (init.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }
  const token = getToken();
  if (token !== null) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  if (init.headers !== undefined) {
    for (const [key, value] of Object.entries(init.headers)) {
      headers.set(key, value);
    }
  }
  return headers;
};

export const apiFetch = async <T>(url: string, init: ApiFetchInit = {}): Promise<T> => {
  const headers = buildHeaders(init);
  const fetchInit: RequestInit = {
    method: init.method ?? 'GET',
    headers,
  };
  if (init.body !== undefined) {
    fetchInit.body = JSON.stringify(init.body);
  }
  if (init.signal !== undefined) {
    fetchInit.signal = init.signal;
  }

  const response = await fetch(url, fetchInit);

  if (response.status === 401) {
    logger.warn('apiFetch.401', { url });
    clearSession();
    globalThis.location.reload();
    throw new ApiError('Authentication required', 401, url, '');
  }

  if (!response.ok) {
    const text = await response.text();
    logger.error('apiFetch.error', { url, status: response.status, body: text });
    throw new ApiError(`HTTP ${String(response.status)}`, response.status, url, text);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
};
