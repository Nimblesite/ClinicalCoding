import { useCallback, useMemo, useState, type ReactElement, type ReactNode } from 'react';
import { logout as gatekeeperLogout } from '../api/gatekeeper';
import type { AuthSession } from '../types/auth';
import { AuthContext, type AuthContextValue } from './auth-context';
import { clearSession, getSession, saveSession } from './auth-storage';

interface AuthProviderProps {
  readonly children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps): ReactElement => {
  const [session, setSession] = useState<AuthSession | null>(() => getSession());

  const login = useCallback((next: AuthSession) => {
    saveSession(next);
    setSession(next);
  }, []);

  const logout = useCallback(async () => {
    await gatekeeperLogout();
    clearSession();
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      currentUser: session?.user ?? null,
      isAuthenticated: session !== null,
      login,
      logout,
    }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
