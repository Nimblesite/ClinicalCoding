import { test as base, type Page } from '@playwright/test';
import { generateTestToken } from './jwt';
import { DASHBOARD_URL, ICD10_URL } from './urls';

export interface AuthOptions {
  userId?: string;
  displayName?: string;
  email?: string;
  navigateTo?: string;
}

export async function createAuthenticatedPage(page: Page, opts: AuthOptions = {}): Promise<Page> {
  const userId = opts.userId ?? 'e2e-test-user';
  const displayName = opts.displayName ?? 'E2E Test User';
  const email = opts.email ?? 'e2etest@example.com';

  const token = generateTestToken(userId, displayName, email);
  const userJson = JSON.stringify({ userId, displayName, email });

  await page.addInitScript(
    ({ icd10Url }) => {
      (window as unknown as { dashboardConfig: { ICD10_API_URL: string } }).dashboardConfig = {
        ICD10_API_URL: icd10Url,
      };
    },
    { icd10Url: ICD10_URL },
  );

  await page.goto(DASHBOARD_URL);

  await page.evaluate(
    ({ t, u }) => {
      localStorage.setItem('gatekeeper_token', t);
      localStorage.setItem('gatekeeper_user', u);
    },
    { t: token, u: userJson },
  );

  const target = opts.navigateTo ?? DASHBOARD_URL;
  await page.reload();

  if (target !== DASHBOARD_URL && target.includes('#')) {
    const hash = target.slice(target.indexOf('#'));
    await page.evaluate((h) => {
      window.location.hash = h;
    }, hash);
    await page.waitForTimeout(500);
  }

  return page;
}

export const test = base;
export { expect } from '@playwright/test';
