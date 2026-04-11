import { createContext } from 'react';
import type { AuthSession, AuthUser } from '../types/auth';

export interface AuthContextValue {
  readonly currentUser: AuthUser | null;
  readonly isAuthenticated: boolean;
  readonly login: (session: AuthSession) => void;
  readonly logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);