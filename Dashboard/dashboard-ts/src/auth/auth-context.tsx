import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactElement,
  type ReactNode,
} from 'react';
import { logout as gatekeeperLogout } from '../api/gatekeeper';
import type { AuthSession, AuthUser } from '../types/auth';
import { clearSession, getSession, saveSession } from './auth-storage';

interface AuthContextValue {
  readonly currentUser: AuthUser | null;
  readonly isAuthenticated: boolean;
  readonly login: (session: AuthSession) => void;
  readonly logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

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

export const useAuth = (): AuthContextValue => {
  const ctx = useContext(AuthContext);
  if (ctx === null) {
    throw new Error('useAuth must be used inside <AuthProvider>');
  }
  return ctx;
};
