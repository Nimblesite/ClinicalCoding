/**
 * E2E tests for browser navigation (back/forward, deep linking).
 * Ported from NavigationE2ETests.cs
 */

import { ClinicalUrl, DashboardUrl, expect, Page, test } from './support/fixture';

/**
 * Create an authenticated page and optionally navigate to a specific URL
 */
async function createAuthenticatedPage(page: Page, navigateTo?: string): Promise<Page> {
  if (navigateTo) {
    await page.goto(navigateTo);
  }
  return page;
}

test.describe('Navigation E2E Tests', () => {
  test('Browser back button navigates to previous view', async ({ authenticatedPage: page }) => {
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    expect(page.url()).toContain('#dashboard');

    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    await page.click('text=Appointments');
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#appointments');

    await page.goBack();
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    await page.goBack();
    await page.waitForSelector('.metric-card', { timeout: 10000 });
    expect(page.url()).toContain('#dashboard');

    await page.close();
  });

  test('Deep linking works - navigating directly to a hash URL loads correct view', async ({
    authenticatedPage: page,
  }) => {
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page, `${DashboardUrl}#patients`);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    let content = await page.content();
    expect(content).toContain('Patients');

    await page.goto(`${DashboardUrl}#appointments`);
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });

    content = await page.content();
    expect(content).toContain('Appointments');

    await page.close();
  });

  test('Edit Patient Cancel button uses history back', async ({
    authenticatedPage: page,
    request,
  }) => {
    const uniqueName = `CancelTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'CancelTestPatient',
        Gender: 'male',
      },
    });
    expect(createResponse.ok()).toBeTruthy();
    const createdJson = await createResponse.json();
    const patientId = createdJson.Id;

    await createAuthenticatedPage(page);
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    await page.fill("input[placeholder*='Search']", uniqueName);
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });
    await page.click(`[data-testid='edit-patient-${patientId}']`);
    await page.waitForSelector("[data-testid='edit-patient-page']", { timeout: 5000 });

    await page.click("button:has-text('Cancel')");
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    expect(page.url()).toContain('#patients');
    expect(page.url()).not.toContain('/edit/');

    await page.close();
  });

  test('Browser back button from Edit Patient page returns to Patients page', async ({
    authenticatedPage: page,
    request,
  }) => {
    const uniqueName = `BackBtnTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'BackButtonTest',
        Gender: 'female',
      },
    });
    expect(createResponse.ok()).toBeTruthy();
    const createdJson = await createResponse.json();
    const patientId = createdJson.Id;

    await createAuthenticatedPage(page);
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    await page.fill("input[placeholder*='Search']", uniqueName);
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });
    await page.click(`[data-testid='edit-patient-${patientId}']`);
    await page.waitForSelector("[data-testid='edit-patient-page']", { timeout: 5000 });

    await page.goBack();

    await page.waitForSelector('.sidebar', { timeout: 10000 });
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    const content = await page.content();
    expect(content).toContain('Patients');
    expect(content).toContain('Add Patient');

    await page.goBack();
    await page.waitForSelector('.metric-card', { timeout: 10000 });
    expect(page.url()).toContain('#dashboard');

    await page.close();
  });

  test('Forward button works after going back', async ({ authenticatedPage: page }) => {
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    await page.click('text=Practitioners');
    await page.waitForSelector('.practitioner-card, .empty-state', { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    await page.goBack();
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    await page.goForward();
    await page.waitForSelector('.practitioner-card, .empty-state', { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    const content = await page.content();
    expect(content).toContain('Practitioners');

    await page.close();
  });
});
