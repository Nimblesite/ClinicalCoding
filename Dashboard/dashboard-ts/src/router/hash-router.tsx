import { useEffect, useMemo, useState, type ReactElement, type ReactNode } from 'react';
import { RouteContext, type ParsedRoute } from './route-context';

const readHash = (): string => {
  const hash = globalThis.location.hash;
  return hash.startsWith('#') ? hash.slice(1) : hash;
};

const parse = (raw: string): ParsedRoute => {
  const clean = raw.replaceAll(/^\/+/g, '');
  if (clean.length === 0) {
    return { name: 'dashboard', params: [], raw: 'dashboard' };
  }
  const [name = 'dashboard', ...params] = clean.split('/');
  return { name, params, raw: clean };
};

interface HashRouterProviderProps {
  readonly children: ReactNode;
}

export const HashRouterProvider = ({ children }: HashRouterProviderProps): ReactElement => {
  const [raw, setRaw] = useState<string>(readHash());
  useEffect((): (() => void) => {
    const onHashChange = (): void => {
      setRaw(readHash());
    };
    globalThis.addEventListener('hashchange', onHashChange);
    return (): void => {
      globalThis.removeEventListener('hashchange', onHashChange);
    };
  }, []);
  const value = useMemo<ParsedRoute>(() => parse(raw), [raw]);
  return <RouteContext.Provider value={value}>{children}</RouteContext.Provider>;
};
