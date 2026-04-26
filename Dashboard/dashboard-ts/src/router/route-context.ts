import { createContext } from 'react';

export interface ParsedRoute {
  readonly name: string;
  readonly params: readonly string[];
  readonly raw: string;
}

export const RouteContext = createContext<ParsedRoute>({
  name: 'dashboard',
  params: [],
  raw: 'dashboard',
});
