import { createHmac } from 'node:crypto';
import { GATEKEEPER_URL } from './urls';

function base64UrlEncode(buf: Buffer): string {
  return buf.toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

let cachedDevToken: string | undefined;

export async function fetchDevToken(): Promise<string> {
  if (cachedDevToken !== undefined) {
    return cachedDevToken;
  }
  const response = await fetch(`${GATEKEEPER_URL}/auth/dev-token`);
  if (!response.ok) {
    throw new Error(
      `Gatekeeper /auth/dev-token returned ${response.status.toString()}. Is dev mode active?`,
    );
  }
  const json = (await response.json()) as { Token: string };
  cachedDevToken = json.Token;
  return cachedDevToken;
}

export function generateTestToken(
  userId = 'e2e-test-user',
  displayName = 'E2E Test User',
  email = 'e2etest@example.com',
): string {
  const signingKey = Buffer.alloc(32, 0);
  const header = base64UrlEncode(Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })));
  const exp = Math.floor(Date.now() / 1000) + 3600;
  const payloadObj = {
    sub: userId,
    name: displayName,
    email,
    roles: ['admin', 'user'],
    jti: `${Date.now().toString()}-${Math.random().toString()}`,
    exp,
  };
  const payload = base64UrlEncode(Buffer.from(JSON.stringify(payloadObj)));
  const signingInput = `${header}.${payload}`;
  const signature = base64UrlEncode(createHmac('sha256', signingKey).update(signingInput).digest());
  return `${signingInput}.${signature}`;
}
