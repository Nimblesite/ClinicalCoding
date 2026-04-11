import type { FormEvent } from 'react';

export interface AuthUser {
  readonly userId: string;
  readonly displayName: string;
  readonly email: string;
}

export interface AuthSession {
  readonly token: string;
  readonly user: AuthUser;
}

/** Form submit event type alias for form handlers */
export type SubmitEvent<T extends HTMLFormElement = HTMLFormElement> = FormEvent<T>;
