interface DashboardConfig {
  readonly CLINICAL_API_URL?: string;
  readonly SCHEDULING_API_URL?: string;
  readonly GATEKEEPER_API_URL?: string;
  readonly ICD10_API_URL?: string;
}

const getConfig = (): DashboardConfig => {
  const winConfig = (globalThis as { dashboardConfig?: DashboardConfig }).dashboardConfig;
  return winConfig ?? {};
};

export const CLINICAL_API = getConfig().CLINICAL_API_URL ?? 'http://localhost:5080';
export const SCHEDULING_API = getConfig().SCHEDULING_API_URL ?? 'http://localhost:5001';
export const GATEKEEPER_API = getConfig().GATEKEEPER_API_URL ?? 'http://localhost:5002';
export const ICD10_API = getConfig().ICD10_API_URL ?? 'http://localhost:5090';
