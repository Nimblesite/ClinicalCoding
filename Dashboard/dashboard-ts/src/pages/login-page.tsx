import { useEffect, useState, type ReactElement } from 'react';
import { loginWithPasskey, registerWithPasskey } from '../api/gatekeeper';
import { getToken } from '../auth/auth-storage';
import { useAuth } from '../auth/use-auth';
import { logger } from '../lib/logger';

interface TriggerLoginUser {
  readonly userId: string;
  readonly displayName: string;
  readonly email: string;
}

declare global {
  interface Window {
    __triggerLogin?: (user: TriggerLoginUser) => void;
  }
}

export const LoginPage = (): ReactElement => {
  const { login } = useAuth();
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    globalThis.window.__triggerLogin = (user: TriggerLoginUser): void => {
      const token = getToken() ?? '';
      login({ token, user });
    };
    return (): void => {
      delete globalThis.window.__triggerLogin;
    };
  }, [login]);

  const handleLogin = async (): Promise<void> => {
    setBusy(true);
    setError(null);
    try {
      const session = await loginWithPasskey();
      login(session);
    } catch (err) {
      logger.error('login.failed', { err });
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setBusy(false);
    }
  };

  const handleRegister = async (e: React.SyntheticEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const session = await registerWithPasskey(email, displayName);
      login(session);
    } catch (err) {
      logger.error('register.failed', { err });
      setError(err instanceof Error ? err.message : 'Register failed');
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="login-shell" data-testid="login-page">
      <section className="login-card">
        <div className="login-brand">
          <div className="login-brand-mark">
            <span className="material-symbols-outlined">health_metrics</span>
          </div>
          <div>
            <h1>Nimblesite</h1>
            <p>Clinical Coding Platform</p>
          </div>
        </div>
        <div className="login-tabs">
          <button
            type="button"
            className={mode === 'login' ? 'active' : ''}
            onClick={() => {
              setMode('login');
            }}
          >
            Sign in
          </button>
          <button
            type="button"
            className={mode === 'register' ? 'active' : ''}
            onClick={() => {
              setMode('register');
            }}
          >
            Register
          </button>
        </div>
        {error !== null && <div className="alert alert-error">{error}</div>}
        {mode === 'login' ? (
          <div className="login-pane">
            <p className="login-subtitle">Sign in with your passkey</p>
            <button
              type="button"
              disabled={busy}
              className="btn btn-primary"
              onClick={() => {
                void handleLogin();
              }}
            >
              {busy ? 'Signing in…' : 'Sign in with Passkey'}
            </button>
          </div>
        ) : (
          <form
            onSubmit={(e) => {
              void handleRegister(e);
            }}
          >
            <h2 className="login-heading">Create your account</h2>
            <label className="input-label" htmlFor="email">
              Email
            </label>
            <input
              id="email"
              className="input"
              type="email"
              required
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
              }}
            />
            <label className="input-label" htmlFor="displayName">
              Display Name
            </label>
            <input
              id="displayName"
              className="input"
              type="text"
              required
              value={displayName}
              onChange={(e) => {
                setDisplayName(e.target.value);
              }}
            />
            <button type="submit" disabled={busy} className="btn btn-primary">
              {busy ? 'Registering…' : 'Create Passkey'}
            </button>
          </form>
        )}
      </section>
    </main>
  );
};
