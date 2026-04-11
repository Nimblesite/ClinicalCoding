type LogPayload = Record<string, unknown> | undefined;

const isDev = (): boolean => import.meta.env.DEV;

export const logger = {
  info: (message: string, payload?: LogPayload): void => {
    if (isDev()) {
      // eslint-disable-next-line no-console -- info is dev-only
      console.log(`[info] ${message}`, payload ?? {});
    }
  },
  warn: (message: string, payload?: LogPayload): void => {
    console.warn(`[warn] ${message}`, payload ?? {});
  },
  error: (message: string, payload?: LogPayload): void => {
    console.error(`[error] ${message}`, payload ?? {});
  },
} as const;
