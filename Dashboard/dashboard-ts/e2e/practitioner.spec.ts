/**
 * E2E tests for practitioner-related functionality.
 * Ported from PractitionerE2ETests.cs
 */

import { test, expect, Page } from './support/fixture';
import { SchedulingUrl } from './support/fixture';

/**
 * Create an authenticated page and optionally navigate to a specific URL
 */
async function createAuthenticatedPage(
  page: Page,
  navigateTo?: string
): Promise<Page> {
  if (navigateTo) {
    await page.goto(navigateTo);
  }
  return page;
}

test.describe('Practitioner E2E Tests', () => {
  test('Dashboard displays practitioner data from Scheduling API', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Practitioners');
    await page.waitForSelector('text=DrTest', { timeout: 10000 });
    await page.waitForSelector('.practitioner-card', { timeout: 5000 });

    const content = await page.content();
    expect(content).toContain('DrTest');
    expect(content).toContain('E2EPractitioner');
    expect(content).toContain('Johnson');
    expect(content).toContain('MD');
    expect(content).toContain('General Practice');

    await page.close();
  });

  test('Practitioners page loads from Scheduling API with FHIR compliant data', async ({ authenticatedPage: page, request }) => {
    const apiResponse = await request.get(`${SchedulingUrl}/Practitioner`);
    expect(apiResponse.ok()).toBeTruthy();
    const apiResponseText = await apiResponse.text();

    expect(apiResponseText).toContain('DR001');
    expect(apiResponseText).toContain('E2EPractitioner');
    expect(apiResponseText).toContain('MD');

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Practitioners');
    await page.waitForSelector('.practitioner-card', { timeout: 10000 });

    const cards = await page.locator('.practitioner-card').all();
    expect(cards.length).toBeGreaterThanOrEqual(3);

    await page.close();
  });

  test('Practitioner creation API works end-to-end', async ({ request }) => {
    const uniqueId = `DR${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: uniqueId,
        Active: true,
        NameGiven: 'ApiDoctor',
        NameFamily: 'TestDoc',
        Qualification: 'MD',
        Specialty: 'Testing',
        TelecomEmail: 'test@hospital.org',
        TelecomPhone: '+1-555-9999'
      }
    });
    expect(createResponse.ok()).toBeTruthy();

    const listResponse = await request.get(`${SchedulingUrl}/Practitioner`);
    const listResponseText = await listResponse.text();
    expect(listResponseText).toContain(uniqueId);
  });

  test('Add Practitioner button opens modal and creates practitioner via API', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Practitioners');
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });
    await page.click("[data-testid='add-practitioner-btn']");
    await page.waitForSelector('.modal', { timeout: 5000 });

    const uniqueIdentifier = `DR${Date.now() % 100000}`;
    const uniqueGivenName = `E2EDoc${Date.now() % 100000}`;
    await page.fill("[data-testid='practitioner-identifier']", uniqueIdentifier);
    await page.fill("[data-testid='practitioner-given-name']", uniqueGivenName);
    await page.fill("[data-testid='practitioner-family-name']", 'TestCreated');
    await page.fill("[data-testid='practitioner-specialty']", 'E2E Testing');
    await page.click("[data-testid='submit-practitioner']");

    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    // Verify via API
    const response = await request.get(`${SchedulingUrl}/Practitioner`);
    expect(await response.text()).toContain(uniqueIdentifier);

    await page.close();
  });

  test('Edit Practitioner button opens edit page and updates practitioner', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    
    // Create practitioner via API
    const uniqueIdentifier = `DREdit${Date.now() % 100000}`;
    const uniqueGivenName = `EditTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: uniqueIdentifier,
        NameFamily: 'OriginalFamily',
        NameGiven: uniqueGivenName,
        Qualification: 'MD',
        Specialty: 'Original Specialty'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    
    const createdJson = await createResponse.text();
    const practitionerIdMatch = createdJson.match(/"Id"\s*:\s*"([^"]+)"/);
    const practitionerId = practitionerIdMatch![1];

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Practitioners');
    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    const editButton = page.locator(`[data-testid='edit-practitioner-${practitionerId}']`);
    await editButton.hover();
    await editButton.click();

    await page.waitForSelector("[data-testid='edit-practitioner-page']", { timeout: 5000 });

    const newSpecialty = `Updated Specialty ${Date.now() % 100000}`;
    await page.fill("[data-testid='edit-practitioner-specialty']", newSpecialty);
    await page.click("[data-testid='save-practitioner']");
    await page.waitForSelector("[data-testid='edit-practitioner-success']", { timeout: 10000 });

    // Verify via API
    const updatedPractitionerJson = await request.get(`${SchedulingUrl}/Practitioner/${practitionerId}`);
    expect(await updatedPractitionerJson.text()).toContain(newSpecialty);

    await page.close();
  });

  test('Practitioner update API works end-to-end', async ({ request }) => {
    const uniqueIdentifier = `DRApi${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: uniqueIdentifier,
        NameFamily: 'ApiOriginal',
        NameGiven: 'TestDoc',
        Qualification: 'MD',
        Specialty: 'Original'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    
    const createdPractitionerJson = await createResponse.text();
    const practitionerIdMatch = createdPractitionerJson.match(/"Id"\s*:\s*"([^"]+)"/);
    const practitionerId = practitionerIdMatch![1];

    const updatedSpecialty = `ApiUpdated${Date.now() % 100000}`;
    const updateResponse = await request.put(`${SchedulingUrl}/Practitioner/${practitionerId}`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: uniqueIdentifier,
        Active: true,
        NameFamily: 'ApiUpdated',
        NameGiven: 'TestDoc',
        Qualification: 'DO',
        Specialty: updatedSpecialty,
        TelecomEmail: 'updated@hospital.com',
        TelecomPhone: '555-1234'
      }
    });
    expect(updateResponse.ok()).toBeTruthy();

    const getResponse = await request.get(`${SchedulingUrl}/Practitioner/${practitionerId}`);
    const getResponseText = await getResponse.text();
    expect(getResponseText).toContain(updatedSpecialty);
    expect(getResponseText).toContain('ApiUpdated');
  });

  test('Browser back button from Edit Practitioner page returns to Practitioners page', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    
    // Create practitioner via API
    const uniqueIdentifier = `DRBack${Date.now() % 100000}`;
    const uniqueGivenName = `BackTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: uniqueIdentifier,
        NameFamily: 'BackButtonTest',
        NameGiven: uniqueGivenName,
        Qualification: 'MD',
        Specialty: 'Testing'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    
    const createdJson = await createResponse.text();
    const practitionerIdMatch = createdJson.match(/"Id"\s*:\s*"([^"]+)"/);
    const practitionerId = practitionerIdMatch![1];

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Practitioners');
    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    const editButton = page.locator(`[data-testid='edit-practitioner-${practitionerId}']`);
    await editButton.hover();
    await editButton.click();
    await page.waitForSelector("[data-testid='edit-practitioner-page']", { timeout: 5000 });

    await page.goBack();

    await page.waitForSelector('.sidebar', { timeout: 10000 });
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    await page.close();
  });
});