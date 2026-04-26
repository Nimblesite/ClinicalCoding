/**
 * E2E tests for patient-related functionality.
 * TypeScript port of C# PatientE2ETests.cs
 */

import { expect, test } from '@playwright/test';
import { ClinicalUrl, generateTestToken, setupAuth } from './support/fixture';

test.describe('Patient E2E Tests', () => {
  /**
   * Dashboard loads and displays patient data from Clinical API.
   */
  test('Dashboard_DisplaysPatientData_FromClinicalApi', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    await setupAuth(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patients');
    await page.waitForSelector('text=TestPatient', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('TestPatient');
    expect(content).toContain('E2ETest');

    await page.close();
  });

  /**
   * Add Patient button opens modal and creates patient via API.
   */
  test('AddPatientButton_OpensModal_AndCreatesPatient', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.text()}`);
    });

    await setupAuth(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", {
      timeout: 10000,
    });
    await page.click("[data-testid='add-patient-btn']");
    await page.waitForSelector('.modal', { timeout: 5000 });

    const uniqueName = `E2ECreated${Date.now() % 100000}`;
    await page.fill("[data-testid='patient-given-name']", uniqueName);
    await page.fill("[data-testid='patient-family-name']", 'TestCreated');
    await page.selectOption("[data-testid='patient-gender']", 'male');
    await page.click("[data-testid='submit-patient']");

    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });

    // Verify via API
    const token = generateTestToken();
    const response = await page.request.get(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const responseText = await response.text();
    expect(responseText).toContain(uniqueName);

    await page.close();
  });

  /**
   * Patient Search button navigates to search and finds patients.
   */
  test('PatientSearchButton_NavigatesToSearch_AndFindsPatients', async ({ browser }) => {
    const page = await browser.newPage();

    await setupAuth(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patient Search');
    await page.waitForSelector("input[placeholder*='Search']", {
      timeout: 5000,
    });
    await page.fill("input[placeholder*='Search']", 'E2ETest');
    await page.waitForSelector('text=TestPatient', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('TestPatient');

    await page.close();
  });

  /**
   * Patient creation API works end-to-end.
   */
  test('PatientCreationApi_WorksEndToEnd', async ({ request }) => {
    const token = generateTestToken();

    const uniqueName = `ApiTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      data: JSON.stringify({
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'ApiCreated',
        Gender: 'female',
      }),
    });
    expect(createResponse.ok()).toBe(true);

    const listResponse = await request.get(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const listText = await listResponse.text();
    expect(listText).toContain(uniqueName);
  });

  /**
   * Edit Patient button opens edit page and updates patient via API.
   */
  test('EditPatientButton_OpensEditPage_AndUpdatesPatient', async ({ browser }) => {
    const token = generateTestToken();

    // Create patient via API first
    const uniqueName = `EditTest${Date.now() % 100000}`;
    const createResponse = await fetch(`${ClinicalUrl}/fhir/Patient/`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'ToBeEdited',
        Gender: 'female',
      }),
    });
    expect(createResponse.ok).toBe(true);
    const createdPatientJson = await createResponse.text();

    const patientIdMatch = /"Id"\s*:\s*"([^"]+)"/.exec(createdPatientJson);
    expect(patientIdMatch).not.toBeNull();
    const patientId = patientIdMatch![1];

    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.text()}`);
    });

    await setupAuth(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", {
      timeout: 10000,
    });
    await page.fill("input[placeholder*='Search']", uniqueName);
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });
    await page.click(`[data-testid='edit-patient-${patientId}']`);
    await page.waitForSelector("[data-testid='edit-patient-page']", {
      timeout: 5000,
    });

    const newFamilyName = `Edited${Date.now() % 100000}`;
    await page.fill("[data-testid='edit-family-name']", newFamilyName);
    await page.click("[data-testid='save-patient']");
    await page.waitForSelector("[data-testid='edit-success']", {
      timeout: 10000,
    });

    // Verify via API
    const updatedPatientResponse = await fetch(`${ClinicalUrl}/fhir/Patient/${patientId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const updatedPatientJson = await updatedPatientResponse.text();
    expect(updatedPatientJson).toContain(newFamilyName);

    await page.close();
  });

  /**
   * Patient update API works end-to-end.
   */
  test('PatientUpdateApi_WorksEndToEnd', async ({ request }) => {
    const token = generateTestToken();

    // Create patient
    const uniqueName = `UpdateApiTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      data: JSON.stringify({
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'Original',
        Gender: 'male',
      }),
    });
    expect(createResponse.ok()).toBe(true);
    const createdPatientJson = await createResponse.text();

    const patientIdMatch = /"Id"\s*:\s*"([^"]+)"/.exec(createdPatientJson);
    const patientId = patientIdMatch![1];

    // Update patient
    const updatedFamilyName = `Updated${Date.now() % 100000}`;
    const updateResponse = await request.put(`${ClinicalUrl}/fhir/Patient/${patientId}`, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      data: JSON.stringify({
        Active: true,
        GivenName: uniqueName,
        FamilyName: updatedFamilyName,
        Gender: 'male',
        Email: 'updated@test.com',
      }),
    });
    expect(updateResponse.ok()).toBe(true);

    // Verify update
    const getResponse = await request.get(`${ClinicalUrl}/fhir/Patient/${patientId}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const getText = await getResponse.text();
    expect(getText).toContain(updatedFamilyName);
    expect(getText).toContain('updated@test.com');
  });
});
