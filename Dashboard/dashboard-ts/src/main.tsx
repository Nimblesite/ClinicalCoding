import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { installErrorHandlers } from './lib/install-error-handlers';
import './styles/variables.css';
import './styles/base.css';
import './styles/components.css';
import './styles/login.css';

installErrorHandlers();

const rootElement = document.querySelector('#root');
if (rootElement === null) {
  throw new Error('Root element #root not found');
}

createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
