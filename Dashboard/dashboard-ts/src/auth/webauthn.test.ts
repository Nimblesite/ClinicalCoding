import { describe, expect, it } from 'vitest';
import { base64UrlDecode, base64UrlEncode } from './webauthn';

describe('base64url roundtrip', () => {
  it('encodes and decodes empty buffer', () => {
    const encoded = base64UrlEncode(new ArrayBuffer(0));
    expect(encoded).toBe('');
    const decoded = base64UrlDecode(encoded);
    expect(decoded.byteLength).toBe(0);
  });

  it('roundtrips arbitrary bytes', () => {
    const bytes = new Uint8Array([1, 2, 3, 250, 251, 252, 0, 255]);
    const encoded = base64UrlEncode(bytes.buffer);
    const decoded = new Uint8Array(base64UrlDecode(encoded));
    expect(decoded).toEqual(bytes);
  });

  it('produces url-safe characters only', () => {
    const bytes = new Uint8Array([255, 254, 253, 252]);
    const encoded = base64UrlEncode(bytes.buffer);
    expect(encoded).not.toMatch(/[+/=]/);
  });
});
