import { createHmac, randomUUID } from 'node:crypto';

function base64UrlEncode(buf: Buffer): string {
  return buf.toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

export function generateTestToken(
  userId = 'e2e-test-user',
  displayName = 'E2E Test User',
  email = 'e2etest@example.com',
): string {
  const signingKey = Buffer.alloc(32); // 32 zero bytes - dev mode key
  const header = base64UrlEncode(Buffer.from('{"alg":"HS256","typ":"JWT"}'));
  const exp = Math.floor(Date.now() / 1000) + 3600;
  const payloadObj = {
    sub: userId,
    name: displayName,
    email,
    jti: randomUUID(),
    exp,
    roles: ['admin', 'user'],
  };
  const payload = base64UrlEncode(Buffer.from(JSON.stringify(payloadObj)));
  const signingInput = `${header}.${payload}`;
  const signature = base64UrlEncode(createHmac('sha256', signingKey).update(signingInput).digest());
  return `${signingInput}.${signature}`;
}
