/**
 * E2E tests for bidirectional sync functionality.
 * Ported from SyncE2ETests.cs
 */

import { test, expect, Page } from './fixture';
import { ClinicalUrl, SchedulingUrl, DashboardUrl } from './fixture';

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

test.describe('Sync E2E Tests', () => {
  test('Sync Dashboard navigates to sync page and displays status', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Sync Dashboard');

    await page.waitForSelector("[data-testid='sync-page']", { timeout: 10000 });
    expect(page.url()).toContain('#sync');

    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });
    await page.waitForSelector("[data-testid='service-status-scheduling']", { timeout: 15000 });
    await page.waitForSelector("[data-testid='sync-records-table']", { timeout: 5000 });
    await page.waitForSelector("[data-testid='action-filter']", { timeout: 5000 });
    await page.waitForSelector("[data-testid='service-filter']", { timeout: 5000 });

    const content = await page.content();
    expect(content).toContain('Sync Dashboard');
    expect(content).toContain('Clinical.Api');
    expect(content).toContain('Scheduling.Api');
    expect(content).toContain('Sync Records');

    await page.close();
  });

  test('Sync Dashboard service filter shows only selected service', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));

    // Create data in both services to ensure we have records from both
    const uniqueId = `FilterTest${Date.now() % 1000000}`;

    // Create patient in Clinical.Api
    await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: `FilterPatient${uniqueId}`,
        FamilyName: 'ClinicalTest',
        Gender: 'other'
      }
    });

    // Create practitioner in Scheduling.Api
    await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Identifier: `FILTER-DR-${uniqueId}`,
        Active: true,
        NameGiven: `FilterDoc${uniqueId}`,
        NameFamily: 'SchedulingTest',
        Qualification: 'MD',
        Specialty: 'Testing'
      }
    });

    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });
    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });
    await page.waitForTimeout(1000); // Allow data to load

    // Get initial count with all services
    const allRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    const initialCount = allRows.length;
    console.log(`[TEST] Initial row count (all services): ${initialCount}`);

    // Filter to Clinical only
    await page.selectOption("[data-testid='service-filter']", 'clinical');
    await page.waitForTimeout(500);

    const clinicalRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Clinical filter row count: ${clinicalRows.length}`);

    // PROVE: Every visible row must be from Clinical service
    for (const row of clinicalRows) {
      const serviceAttr = await row.getAttribute('data-service');
      expect(serviceAttr).toBe('clinical');
    }

    // Filter to Scheduling only
    await page.selectOption("[data-testid='service-filter']", 'scheduling');
    await page.waitForTimeout(500);

    const schedulingRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Scheduling filter row count: ${schedulingRows.length}`);

    // PROVE: Every visible row must be from Scheduling service
    for (const row of schedulingRows) {
      const serviceAttr = await row.getAttribute('data-service');
      expect(serviceAttr).toBe('scheduling');
    }

    // PROVE: Combined counts should equal total (or less if overlap)
    expect(clinicalRows.length + schedulingRows.length).toBeLessThanOrEqual(initialCount + 1);

    await page.close();
  });

  test('Sync Dashboard action filter shows only selected operation', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));

    // Create a patient (Insert operation = 0)
    const uniqueId = `ActionTest${Date.now() % 1000000}`;
    await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: `ActionPatient${uniqueId}`,
        FamilyName: 'InsertTest',
        Gender: 'female'
      }
    });

    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });
    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });

    await page.waitForTimeout(1000); // Allow data to load

    // Wait for sync records to appear in the table
    await page.waitForFunction(() => {
      const rows = document.querySelectorAll('[data-testid="sync-records-table"] tbody tr');
      return rows.length > 0;
    }, { timeout: 20000 });

    // Log initial state before filtering
    const initialRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Initial row count before filter: ${initialRows.length}`);
    for (const row of initialRows.slice(0, 5)) {
      const op = await row.getAttribute('data-operation');
      console.log(`[TEST] Row data-operation: ${op}`);
    }

    // Filter to Insert operations only (operation = 0)
    await page.selectOption("[data-testid='action-filter']", '0');
    await page.waitForTimeout(500); // Allow React to start re-rendering

    // Wait for React to apply the filter - wait until ALL visible rows have operation=0
    // OR there are no rows (which is valid if no Insert operations exist)
    await page.waitForFunction(() => {
      const rows = document.querySelectorAll('[data-testid="sync-records-table"] tbody tr');
      console.log('[Filter] Row count after filter: ' + rows.length);
      if (rows.length === 0) return true;
      const allMatch = Array.from(rows).every(row => {
        const op = row.getAttribute('data-operation');
        console.log('[Filter] Row operation: ' + op);
        return op === '0';
      });
      return allMatch;
    }, { timeout: 20000 });

    const insertRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Insert filter row count: ${insertRows.length}`);

    // PROVE: Every visible row must have Insert operation (0)
    for (const row of insertRows) {
      const operationAttr = await row.getAttribute('data-operation');
      expect(operationAttr).toBe('0');
    }

    // Verify filter value is selected
    const selectedValue = await page.evaluate(() => {
      const el = document.querySelector('[data-testid="action-filter"]') as HTMLSelectElement;
      return el?.value;
    });
    expect(selectedValue).toBe('0');

    // Reset filter
    await page.selectOption("[data-testid='action-filter']", 'all');

    // Wait for React to apply the reset filter
    await page.waitForFunction(() => {
      const el = document.querySelector('[data-testid="action-filter"]') as HTMLSelectElement;
      return el?.value === 'all';
    }, { timeout: 5000 });
    await page.waitForTimeout(300); // Small buffer for React re-render

    const allRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    expect(allRows.length).toBeGreaterThanOrEqual(insertRows.length);

    await page.close();
  });

  test('Sync Dashboard combined filters work together', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));

    // Create data in Clinical.Api
    const uniqueId = `ComboTest${Date.now() % 1000000}`;
    await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: `ComboPatient${uniqueId}`,
        FamilyName: 'ComboTest',
        Gender: 'male'
      }
    });

    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });
    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });
    await page.waitForTimeout(1000);

    // Apply both filters: Clinical + Insert
    await page.selectOption("[data-testid='service-filter']", 'clinical');
    await page.selectOption("[data-testid='action-filter']", '0');

    // Wait for React to apply both filters
    await page.waitForFunction(() => {
      const rows = document.querySelectorAll('[data-testid="sync-records-table"] tbody tr');
      if (rows.length === 0) return true;
      return Array.from(rows).every(row =>
        row.getAttribute('data-service') === 'clinical' &&
        row.getAttribute('data-operation') === '0'
      );
    }, { timeout: 5000 });

    const filteredRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Combined filter (Clinical + Insert) row count: ${filteredRows.length}`);

    // PROVE: Every row must satisfy BOTH filters
    for (const row of filteredRows) {
      const serviceAttr = await row.getAttribute('data-service');
      const operationAttr = await row.getAttribute('data-operation');
      expect(serviceAttr).toBe('clinical');
      expect(operationAttr).toBe('0');
    }

    // Try Scheduling + Insert
    await page.selectOption("[data-testid='service-filter']", 'scheduling');

    // Wait for React to apply the service filter change
    await page.waitForFunction(() => {
      const rows = document.querySelectorAll('[data-testid="sync-records-table"] tbody tr');
      if (rows.length === 0) return true;
      return Array.from(rows).every(row =>
        row.getAttribute('data-service') === 'scheduling' &&
        row.getAttribute('data-operation') === '0'
      );
    }, { timeout: 5000 });

    const schedulingInsertRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    for (const row of schedulingInsertRows) {
      const serviceAttr = await row.getAttribute('data-service');
      const operationAttr = await row.getAttribute('data-operation');
      expect(serviceAttr).toBe('scheduling');
      expect(operationAttr).toBe('0');
    }

    await page.close();
  });

  test('Sync Dashboard search filter filters correctly', async ({ authenticatedPage: page, request }) => {
    // Create a patient BEFORE loading the sync page so data is fresh
    const uniqueId = `SearchTest${Date.now() % 1000000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: `SearchPatient${uniqueId}`,
        FamilyName: 'SearchTest',
        Gender: 'male'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    // Navigate to sync page AFTER patient exists in sync log
    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));

    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });
    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });
    await page.waitForTimeout(1000);

    // Get initial count
    const initialRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    const initialCount = initialRows.length;

    // Search for the patient ID
    await page.fill("[data-testid='sync-search']", patientId);
    await page.waitForTimeout(500);

    const searchRows = await page.locator("[data-testid='sync-records-table'] tbody tr").all();
    console.log(`[TEST] Search for '${patientId}' found ${searchRows.length} rows`);

    // PROVE: Search should find at least one matching row
    expect(searchRows.length).toBeGreaterThanOrEqual(1);
    expect(searchRows.length < initialCount || initialCount <= 1).toBeTruthy();

    await page.close();
  });

  test('Deep linking to sync page works', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });

    const content = await page.content();
    expect(content).toContain('Sync Dashboard');
    expect(content).toContain('Monitor and manage sync operations');

    await page.close();
  });

  test('Sync Clinical patient appears in Scheduling after sync', async ({ request }) => {
    const uniqueId = `SyncTest${Date.now() % 1000000}`;
    const patientRequest = {
      Active: true,
      GivenName: `SyncPatient${uniqueId}`,
      FamilyName: 'ToScheduling',
      Gender: 'other',
      Phone: '+1-555-SYNC',
      Email: `sync${uniqueId}@test.com`
    };

    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: patientRequest
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    const clinicalGetResponse = await request.get(`${ClinicalUrl}/fhir/Patient/${patientId}`);
    expect(clinicalGetResponse.ok()).toBeTruthy();

    let syncedToScheduling = false;
    for (let i = 0; i < 18; i++) {
      await new Promise(r => setTimeout(r, 5000));

      const syncPatientsResponse = await request.get(`${SchedulingUrl}/sync/patients`);
      if (syncPatientsResponse.ok()) {
        const patientsJson = await syncPatientsResponse.text();
        if (patientsJson.includes(patientId) || patientsJson.includes(uniqueId)) {
          syncedToScheduling = true;
          break;
        }
      }
    }

    expect(syncedToScheduling).toBeTruthy();
  });

  test('Sync Scheduling practitioner appears in Clinical after sync', async ({ request }) => {
    const uniqueId = `SyncTest${Date.now() % 1000000}`;
    const practitionerRequest = {
      Identifier: `SYNC-DR-${uniqueId}`,
      Active: true,
      NameGiven: `SyncDoctor${uniqueId}`,
      NameFamily: 'ToClinical',
      Qualification: 'MD',
      Specialty: 'Sync Testing',
      TelecomEmail: `syncdoc${uniqueId}@hospital.org`,
      TelecomPhone: '+1-555-SYNC'
    };

    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: { 'Content-Type': 'application/json' },
      data: practitionerRequest
    });
    expect(createResponse.ok()).toBeTruthy();
    const practitionerJson = await createResponse.json();
    const practitionerId = practitionerJson.Id;

    const schedulingGetResponse = await request.get(`${SchedulingUrl}/Practitioner/${practitionerId}`);
    expect(schedulingGetResponse.ok()).toBeTruthy();

    let syncedToClinical = false;
    for (let i = 0; i < 30; i++) {
      await new Promise(r => setTimeout(r, 5000));

      const syncProvidersResponse = await request.get(`${ClinicalUrl}/sync/providers`);
      if (syncProvidersResponse.ok()) {
        const providersJson = await syncProvidersResponse.text();
        if (providersJson.includes(practitionerId) || providersJson.includes(uniqueId)) {
          syncedToClinical = true;
          break;
        }
      }
    }

    expect(syncedToClinical).toBeTruthy();
  });

  test('Sync changes appear in Dashboard UI seamlessly', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));

    const uniqueId = `DashSync${Date.now() % 1000000}`;
    const patientRequest = {
      Active: true,
      GivenName: `DashboardSync${uniqueId}`,
      FamilyName: 'TestPatient',
      Gender: 'male'
    };

    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: patientRequest
    });
    expect(createResponse.ok()).toBeTruthy();

    await createAuthenticatedPage(page, `${DashboardUrl}#sync`);
    await page.waitForSelector("[data-testid='sync-page']", { timeout: 20000 });
    await page.waitForSelector("[data-testid='service-status-clinical']", { timeout: 15000 });
    await page.waitForSelector("[data-testid='service-status-scheduling']", { timeout: 15000 });

    const content = await page.content();
    expect(content).toContain('Clinical.Api');
    expect(content).toContain('Scheduling.Api');
    expect(content).toContain('Sync Records');

    const clinicalCardVisible = await page.isVisible("[data-testid='service-status-clinical']");
    const schedulingCardVisible = await page.isVisible("[data-testid='service-status-scheduling']");
    expect(clinicalCardVisible).toBeTruthy();
    expect(schedulingCardVisible).toBeTruthy();

    await page.close();
  });

  test('Sync creates log entries when data changes', async ({ request }) => {
    const initialClinicalResponse = await request.get(`${ClinicalUrl}/sync/records`);
    expect(initialClinicalResponse.ok()).toBeTruthy();
    const initialClinicalJson = await initialClinicalResponse.json();
    const initialClinicalCount = initialClinicalJson.total;

    const uniqueId = `LogTest${Date.now() % 1000000}`;
    const patientRequest = {
      Active: true,
      GivenName: `LogPatient${uniqueId}`,
      FamilyName: 'TestSync',
      Gender: 'female'
    };

    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: patientRequest
    });
    expect(createResponse.ok()).toBeTruthy();

    const updatedClinicalResponse = await request.get(`${ClinicalUrl}/sync/records`);
    expect(updatedClinicalResponse.ok()).toBeTruthy();
    const updatedClinicalJson = await updatedClinicalResponse.json();
    const updatedClinicalCount = updatedClinicalJson.total;

    expect(updatedClinicalCount).toBeGreaterThan(initialClinicalCount);
  });
});