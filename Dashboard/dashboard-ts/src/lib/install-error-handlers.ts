import { logger } from './logger';

export const installErrorHandlers = (): void => {
  globalThis.addEventListener('error', (event) => {
    logger.error('window.error', {
      message: event.message,
      filename: event.filename,
      lineno: event.lineno,
      colno: event.colno,
      error: event.error,
    });
  });

  globalThis.addEventListener('unhandledrejection', (event) => {
    logger.error('unhandledrejection', { reason: event.reason });
  });
};
