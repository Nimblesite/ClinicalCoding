import { useContext } from 'react';
import { AuthContext, type AuthContextValue } from './auth-context';

export const useAuth = (): AuthContextValue => {
  const ctx = useContext(AuthContext);
  if (ctx === null) {
    throw new Error('useAuth must be used inside <AuthProvider>');
  }
  return ctx;
};
