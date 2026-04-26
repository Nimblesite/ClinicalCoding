export const base64UrlEncode = (buffer: ArrayBuffer): string => {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (const byte of bytes) {
    binary += String.fromCharCode(byte);
  }
  return globalThis.btoa(binary).replaceAll('+', '-').replaceAll('/', '_').replaceAll('=', '');
};

export const base64UrlDecode = (value: string): ArrayBuffer => {
  const padded = value.replaceAll('-', '+').replaceAll('_', '/');
  const padLen = (4 - (padded.length % 4)) % 4;
  const padStr = padded + '='.repeat(padLen);
  const binary = globalThis.atob(padStr);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes.buffer;
};
