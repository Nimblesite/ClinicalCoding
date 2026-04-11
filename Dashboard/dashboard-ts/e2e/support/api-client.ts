import { generateTestToken } from './jwt';

export interface ApiResponse {
  status: number;
  ok: boolean;
  headers: Headers;
  body: string;
}

function authHeaders(extra?: Record<string, string>): Record<string, string> {
  return {
    Authorization: `Bearer ${generateTestToken()}`,
    'Content-Type': 'application/json',
    ...extra,
  };
}

export async function apiGet(url: string, extraHeaders?: Record<string, string>): Promise<ApiResponse> {
  const res = await fetch(url, { method: 'GET', headers: authHeaders(extraHeaders) });
  const body = await res.text();
  return { status: res.status, ok: res.ok, headers: res.headers, body };
}

export async function apiGetString(url: string): Promise<string> {
  const res = await apiGet(url);
  if (!res.ok) {
    throw new Error(`GET ${url} failed: ${String(res.status)} ${res.body}`);
  }
  return res.body;
}

export async function apiPost(
  url: string,
  body: unknown,
  extraHeaders?: Record<string, string>,
): Promise<ApiResponse> {
  const payload = typeof body === 'string' ? body : JSON.stringify(body);
  const res = await fetch(url, { method: 'POST', headers: authHeaders(extraHeaders), body: payload });
  const text = await res.text();
  return { status: res.status, ok: res.ok, headers: res.headers, body: text };
}

export async function apiPostEnsure(url: string, body: unknown): Promise<ApiResponse> {
  const res = await apiPost(url, body);
  if (!res.ok) {
    throw new Error(`POST ${url} failed: ${String(res.status)} ${res.body}`);
  }
  return res;
}

export async function apiPut(url: string, body: unknown): Promise<ApiResponse> {
  const payload = typeof body === 'string' ? body : JSON.stringify(body);
  const res = await fetch(url, { method: 'PUT', headers: authHeaders(), body: payload });
  const text = await res.text();
  return { status: res.status, ok: res.ok, headers: res.headers, body: text };
}

export async function apiPutEnsure(url: string, body: unknown): Promise<ApiResponse> {
  const res = await apiPut(url, body);
  if (!res.ok) {
    throw new Error(`PUT ${url} failed: ${String(res.status)} ${res.body}`);
  }
  return res;
}

export function extractId(json: string): string {
  const match = /"Id"\s*:\s*"([^"]+)"/.exec(json);
  if (match === null) {
    throw new Error(`Could not extract Id from: ${json.slice(0, 200)}`);
  }
  // index 1 is the captured group; guaranteed by the regex
  return match[1] as string;
}
