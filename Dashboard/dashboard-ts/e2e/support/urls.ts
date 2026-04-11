/* eslint-disable @typescript-eslint/no-unnecessary-condition -- env lookups */
const env = process.env;

export const CLINICAL_URL = env.E2E_CLINICAL_URL ?? 'http://localhost:5080';
export const SCHEDULING_URL = env.E2E_SCHEDULING_URL ?? 'http://localhost:5001';
export const GATEKEEPER_URL = env.E2E_GATEKEEPER_URL ?? 'http://localhost:5002';
export const ICD10_URL = env.E2E_ICD10_URL ?? 'http://localhost:5090';
export const DASHBOARD_URL = env.E2E_DASHBOARD_URL ?? 'http://localhost:5173';
