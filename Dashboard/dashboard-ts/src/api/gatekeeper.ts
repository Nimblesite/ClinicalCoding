import { base64UrlDecode, base64UrlEncode } from '../auth/webauthn';
import type { AuthSession } from '../types/auth';
import { apiFetch } from './client';
import { GATEKEEPER_API } from './config';

interface BeginLoginResponse {
  readonly ChallengeId: string;
  readonly OptionsJson: string;
}

type BeginRegisterResponse = BeginLoginResponse;

interface CompleteResponse {
  readonly Token: string;
  readonly UserId: string;
  readonly DisplayName: string;
  readonly Email: string;
}

interface PublicKeyOptions {
  challenge: string;
  rpId?: string;
  allowCredentials?: unknown;
  pubKeyCredParams?: unknown;
  timeout?: number;
  user?: { id: string; name: string; displayName: string };
  rp?: { id?: string; name: string };
  excludeCredentials?: unknown;
  authenticatorSelection?: unknown;
  attestation?: string;
}

const decodeChallenge = (optionsJson: string): PublicKeyOptions => {
  const opts = JSON.parse(optionsJson) as PublicKeyOptions;
  return opts;
};

const toCompleteResponse = (resp: CompleteResponse): AuthSession => ({
  token: resp.Token,
  user: { userId: resp.UserId, displayName: resp.DisplayName, email: resp.Email },
});

export const loginWithPasskey = async (): Promise<AuthSession> => {
  const begin = await apiFetch<BeginLoginResponse>(`${GATEKEEPER_API}/auth/login/begin`, {
    method: 'POST',
    body: {},
  });
  const opts = decodeChallenge(begin.OptionsJson);
  const publicKey: PublicKeyCredentialRequestOptions = {
    challenge: base64UrlDecode(opts.challenge),
    timeout: 120_000,
    userVerification: 'preferred',
  };
  const cred = (await navigator.credentials.get({ publicKey })) as PublicKeyCredential | null;
  if (cred === null) throw new Error('Passkey ceremony cancelled');
  const assertion = cred.response as AuthenticatorAssertionResponse;
  const assertionResponse = {
    id: cred.id,
    rawId: base64UrlEncode(cred.rawId),
    type: cred.type,
    response: {
      authenticatorData: base64UrlEncode(assertion.authenticatorData),
      clientDataJSON: base64UrlEncode(assertion.clientDataJSON),
      signature: base64UrlEncode(assertion.signature),
      userHandle: assertion.userHandle === null ? null : base64UrlEncode(assertion.userHandle),
    },
  };
  const complete = await apiFetch<CompleteResponse>(`${GATEKEEPER_API}/auth/login/complete`, {
    method: 'POST',
    body: {
      ChallengeId: begin.ChallengeId,
      OptionsJson: begin.OptionsJson,
      AssertionResponse: assertionResponse,
    },
  });
  return toCompleteResponse(complete);
};

export const registerWithPasskey = async (
  email: string,
  displayName: string,
): Promise<AuthSession> => {
  const begin = await apiFetch<BeginRegisterResponse>(`${GATEKEEPER_API}/auth/register/begin`, {
    method: 'POST',
    body: { Email: email, DisplayName: displayName },
  });
  const opts = decodeChallenge(begin.OptionsJson);
  if (opts.user === undefined || opts.rp === undefined) {
    throw new Error('Register options missing user/rp');
  }
  const publicKey: PublicKeyCredentialCreationOptions = {
    challenge: base64UrlDecode(opts.challenge),
    rp: { id: opts.rp.id ?? globalThis.location.hostname, name: opts.rp.name },
    user: {
      id: base64UrlDecode(opts.user.id),
      name: opts.user.name,
      displayName: opts.user.displayName,
    },
    pubKeyCredParams: [{ alg: -7, type: 'public-key' }],
    timeout: 120_000,
  };
  const cred = (await navigator.credentials.create({ publicKey })) as PublicKeyCredential | null;
  if (cred === null) throw new Error('Passkey registration cancelled');
  const att = cred.response as AuthenticatorAttestationResponse;
  const attestationResponse = {
    id: cred.id,
    rawId: base64UrlEncode(cred.rawId),
    type: cred.type,
    response: {
      attestationObject: base64UrlEncode(att.attestationObject),
      clientDataJSON: base64UrlEncode(att.clientDataJSON),
    },
  };
  const complete = await apiFetch<CompleteResponse>(`${GATEKEEPER_API}/auth/register/complete`, {
    method: 'POST',
    body: {
      ChallengeId: begin.ChallengeId,
      OptionsJson: begin.OptionsJson,
      AttestationResponse: attestationResponse,
    },
  });
  return toCompleteResponse(complete);
};

export const logout = async (): Promise<void> => {
  try {
    await apiFetch<unknown>(`${GATEKEEPER_API}/auth/logout`, { method: 'POST' });
  } catch {
    // best effort
  }
};
