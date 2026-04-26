import { beforeEach, describe, expect, it } from 'vitest';
import { clearSession, getSession, getToken, getUser, saveSession } from './auth-storage';

describe('auth-storage', () => {
  beforeEach(() => {
    globalThis.localStorage.clear();
  });

  it('returns null when nothing stored', () => {
    expect(getToken()).toBeNull();
    expect(getUser()).toBeNull();
    expect(getSession()).toBeNull();
  });

  it('saves and reads back a session', () => {
    saveSession({
      token: 't',
      user: { userId: 'u', displayName: 'd', email: 'e@x.com' },
    });
    expect(getToken()).toBe('t');
    expect(getUser()?.email).toBe('e@x.com');
    expect(getSession()?.token).toBe('t');
  });

  it('clearSession removes both keys', () => {
    saveSession({ token: 't', user: { userId: 'u', displayName: 'd', email: 'e@x.com' } });
    clearSession();
    expect(getSession()).toBeNull();
  });

  it('returns null on malformed user JSON', () => {
    globalThis.localStorage.setItem('gatekeeper_user', 'not-json');
    expect(getUser()).toBeNull();
  });
});
