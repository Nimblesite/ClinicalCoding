import type { ReactElement } from 'react';
import { Link } from 'react-router-dom';

export const NotFoundPage = (): ReactElement => (
  <main className="page">
    <h1>404</h1>
    <p>The page you requested does not exist.</p>
    <Link to="/">Back to dashboard</Link>
  </main>
);
