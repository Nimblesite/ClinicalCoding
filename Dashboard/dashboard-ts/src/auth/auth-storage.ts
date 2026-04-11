import type { AuthSession, AuthUser } from '../types/auth';

const TOKEN_KEY = 'gatekeeper_token';
const USER_KEY = 'gatekeeper_user';

export const getToken = (): string | null => globalThis.localStorage.getItem(TOKEN_KEY);

export const getUser = (): AuthUser | null => {
  const raw = globalThis.localStorage.getItem(USER_KEY);
  if (raw === null) return null;
  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
};

export const getSession = (): AuthSession | null => {
  const token = getToken();
  const user = getUser();
  if (token === null || user === null) return null;
  return { token, user };
};

export const saveSession = (session: AuthSession): void => {
  globalThis.localStorage.setItem(TOKEN_KEY, session.token);
  globalThis.localStorage.setItem(USER_KEY, JSON.stringify(session.user));
};

export const clearSession = (): void => {
  globalThis.localStorage.removeItem(TOKEN_KEY);
  globalThis.localStorage.removeItem(USER_KEY);
};
