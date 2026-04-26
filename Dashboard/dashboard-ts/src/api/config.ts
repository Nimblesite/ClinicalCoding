interface DashboardConfig {
  readonly CLINICAL_API_URL?: string;
  readonly SCHEDULING_API_URL?: string;
  readonly GATEKEEPER_API_URL?: string;
  readonly ICD10_API_URL?: string;
}

interface ViteEnv {
  readonly VITE_CLINICAL_API_URL?: string;
  readonly VITE_SCHEDULING_API_URL?: string;
  readonly VITE_GATEKEEPER_API_URL?: string;
  readonly VITE_ICD10_API_URL?: string;
}

const getRuntimeConfig = (): DashboardConfig => {
  const winConfig = (globalThis as { dashboardConfig?: DashboardConfig }).dashboardConfig;
  return winConfig ?? {};
};

const getViteEnv = (): ViteEnv => {
  const meta = import.meta as unknown as { env?: ViteEnv };
  return meta.env ?? {};
};

const runtime = getRuntimeConfig();
const env = getViteEnv();

export const CLINICAL_API =
  runtime.CLINICAL_API_URL ?? env.VITE_CLINICAL_API_URL ?? 'http://localhost:5080';
export const SCHEDULING_API =
  runtime.SCHEDULING_API_URL ?? env.VITE_SCHEDULING_API_URL ?? 'http://localhost:5001';
export const GATEKEEPER_API =
  runtime.GATEKEEPER_API_URL ?? env.VITE_GATEKEEPER_API_URL ?? 'http://localhost:5002';
export const ICD10_API = runtime.ICD10_API_URL ?? env.VITE_ICD10_API_URL ?? 'http://localhost:5090';
