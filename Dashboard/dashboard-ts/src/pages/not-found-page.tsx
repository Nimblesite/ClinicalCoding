import type { ReactElement } from 'react';

export const NotFoundPage = (): ReactElement => (
  <main className="page">
    <h1>404</h1>
    <p>The page you requested does not exist.</p>
    <a href="#dashboard">Back to dashboard</a>
  </main>
);
