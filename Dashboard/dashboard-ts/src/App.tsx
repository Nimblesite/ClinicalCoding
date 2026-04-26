import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactElement } from 'react';
import { AuthProvider } from './auth/auth-provider';
import { HashRouterProvider } from './router/hash-router';
import { Routes } from './routes';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 0, retry: 1 },
  },
});

export const App = (): ReactElement => (
  <QueryClientProvider client={queryClient}>
    <AuthProvider>
      <HashRouterProvider>
        <Routes />
      </HashRouterProvider>
    </AuthProvider>
  </QueryClientProvider>
);
