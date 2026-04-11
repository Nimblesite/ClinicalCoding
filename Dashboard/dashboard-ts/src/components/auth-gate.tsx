import type { ReactElement } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../auth/use-auth';

interface AuthGateProps {
  readonly children: ReactElement;
}

export const AuthGate = ({ children }: AuthGateProps): ReactElement => {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
};