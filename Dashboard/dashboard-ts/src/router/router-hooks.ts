import { useContext } from 'react';
import { RouteContext, type ParsedRoute } from './hash-router';

export const useRoute = (): ParsedRoute => useContext(RouteContext);

export const navigate = (path: string): void => {
  const clean = path.replaceAll(/^\/+/g, '');
  const target = `#${clean}`;
  if (globalThis.location.hash === target) {
    globalThis.dispatchEvent(new HashChangeEvent('hashchange'));
    return;
  }
  globalThis.location.hash = clean;
};