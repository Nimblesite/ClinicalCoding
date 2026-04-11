export interface AuthUser {
  readonly userId: string;
  readonly displayName: string;
  readonly email: string;
}

export interface AuthSession {
  readonly token: string;
  readonly user: AuthUser;
}