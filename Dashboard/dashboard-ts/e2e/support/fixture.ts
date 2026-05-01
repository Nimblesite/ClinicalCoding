/**
 * E2E Test Fixture - Exports from existing infrastructure
 * Provides authenticated page fixture and API URL constants
 */

import { test as base, expect, type APIRequestContext, type Page } from '@playwright/test';
import { fetchDevToken, generateTestToken } from './jwt';
import { CLINICAL_URL, DASHBOARD_URL, GATEKEEPER_URL, ICD10_URL, SCHEDULING_URL } from './urls';

// Re-export URLs for test files
export const ClinicalUrl = CLINICAL_URL;
export const SchedulingUrl = SCHEDULING_URL;
export const GatekeeperUrl = GATEKEEPER_URL;
export const Icd10Url = ICD10_URL;
export const DashboardUrl = DASHBOARD_URL;

/**
 * Create an authenticated page
 */
export async function createAuthenticatedPage(
  page: Page,
  navigateTo?: string,
  userId: string = 'e2e-test-user',
  displayName: string = 'E2E Test User',
  email: string = 'e2etest@example.com',
): Promise<Page> {
  const token = generateTestToken(userId, displayName, email);
  const userJson = JSON.stringify({ userId, displayName, email });

  // Navigate first to establish origin
  await page.goto(DASHBOARD_URL);

  // Set auth state in localStorage
  await page.evaluate(
    ({ token, userJson }: { token: string; userJson: string }) => {
      localStorage.setItem('gatekeeper_token', token);
      localStorage.setItem('gatekeeper_user', userJson);
    },
    { token, userJson },
  );

  // Reload to pick up auth state
  await page.reload();

  const target = navigateTo ?? `${DASHBOARD_URL}#dashboard`;

  if (target.includes('#')) {
    const hash = target.slice(target.indexOf('#'));
    await page.evaluate((h) => {
      window.location.hash = h;
    }, hash);
    await page.waitForTimeout(500);
  }

  return page;
}

/**
 * Extended test fixture with authentication helpers
 */
export const test = base.extend<{
  authenticatedPage: Page;
  request: APIRequestContext;
}>({
  request: async ({ playwright }, use) => {
    const token = await fetchDevToken();
    const request = await playwright.request.newContext({
      extraHTTPHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });

    await use(request);
    await request.dispose();
  },

  authenticatedPage: async ({ browser }, use) => {
    const page = await browser.newPage();

    // Setup auth before each test
    await createAuthenticatedPage(page);

    await use(page);

    await page.close();
  },
});

export { expect };

export { generateTestToken } from './jwt';

export async function setupAuth(page: Page): Promise<void> {
  await createAuthenticatedPage(page);
}
