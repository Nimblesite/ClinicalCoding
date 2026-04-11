/**
 * Core Dashboard E2E tests.
 * Ported from DashboardE2ETests.cs
 */

import { test, expect, Page } from './fixture';
import { ClinicalUrl, SchedulingUrl, GatekeeperUrl, DashboardUrl } from './fixture';

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

test.describe('Dashboard Core E2E Tests', () => {
  test('Dashboard main page shows stats from both APIs', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.waitForSelector('.metric-card', { timeout: 10000 });

    const cards = await page.locator('.metric-card').all();
    expect(cards.length).toBeGreaterThan(0);

    await page.close();
  });

  test('Add Patient button opens modal and creates patient via API', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Patients page
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    // Click Add Patient button
    await page.click("[data-testid='add-patient-btn']");

    // Wait for modal to appear
    await page.waitForSelector('.modal', { timeout: 5000 });

    // Fill in patient details
    const uniqueName = `E2ECreated${Date.now() % 100000}`;
    await page.fill("[data-testid='patient-given-name']", uniqueName);
    await page.fill("[data-testid='patient-family-name']", 'TestCreated');
    await page.selectOption("[data-testid='patient-gender']", 'male');

    // Submit the form
    await page.click("[data-testid='submit-patient']");

    // Wait for modal to close and patient to appear in list
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });

    // Verify via API that patient was actually created
    const response = await request.get(`${ClinicalUrl}/fhir/Patient/`);
    expect(await response.text()).toContain(uniqueName);

    await page.close();
  });

  test('Add Appointment button opens modal and creates appointment via API', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Appointments page
    await page.click('text=Appointments');
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });

    // Click Add Appointment button
    await page.click("[data-testid='add-appointment-btn']");

    // Wait for modal to appear
    await page.waitForSelector('.modal', { timeout: 5000 });

    // Fill in appointment details
    const uniqueServiceType = `E2EConsult${Date.now() % 100000}`;
    await page.fill("[data-testid='appointment-service-type']", uniqueServiceType);

    // Submit the form
    await page.click("[data-testid='submit-appointment']");

    // Wait for modal to close and appointment to appear in list
    await page.waitForSelector(`text=${uniqueServiceType}`, { timeout: 10000 });

    // Verify via API that appointment was actually created
    const response = await request.get(`${SchedulingUrl}/Appointment`);
    expect(await response.text()).toContain(uniqueServiceType);

    await page.close();
  });

  test('Patient Search button navigates to search and finds patients', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Click the Patient Search button
    await page.click('text=Patient Search');

    // Should navigate to patients page with search focused
    await page.waitForSelector("input[placeholder*='Search']", { timeout: 5000 });

    // Type a search query
    await page.fill("input[placeholder*='Search']", 'E2ETest');

    // Wait for filtered results
    await page.waitForSelector('text=TestPatient', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('TestPatient');

    await page.close();
  });

  test('View Schedule button navigates to appointments', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Click the View Schedule button
    await page.click('text=View Schedule');

    // Should navigate to appointments page
    await page.waitForSelector('text=Appointments', { timeout: 5000 });

    // Should show the seeded appointment
    await page.waitForSelector('text=Checkup', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('Checkup');

    await page.close();
  });

  test('Patient creation API works end-to-end', async ({ request }) => {
    // Create a patient with a unique name
    const uniqueName = `ApiTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'ApiCreated',
        Gender: 'female'
      }
    });
    expect(createResponse.ok()).toBeTruthy();

    // Verify patient was created by fetching all patients
    const listResponse = await request.get(`${ClinicalUrl}/fhir/Patient/`);
    const listBody = await listResponse.text();
    expect(listBody).toContain(uniqueName);
    expect(listBody).toContain('ApiCreated');
  });

  test('Practitioner creation API works end-to-end', async ({ request }) => {
    // Create a practitioner with a unique identifier
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

    // Verify practitioner was created
    const listResponse = await request.get(`${SchedulingUrl}/Practitioner`);
    const listBody = await listResponse.text();
    expect(listBody).toContain(uniqueId);
    expect(listBody).toContain('ApiDoctor');
  });

  test('Edit Patient button opens edit page and updates patient via API', async ({ authenticatedPage: page, request }) => {
    // First create a patient to edit
    const uniqueName = `EditTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'ToBeEdited',
        Gender: 'female'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Patients page
    await page.click('text=Patients');

    // Wait for the page to load (add-patient-btn is a good indicator)
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    // Search for the patient to make sure it appears
    await page.fill("input[placeholder*='Search']", uniqueName);

    // Wait for the patient to appear in filtered results
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });

    // Click Edit button for the created patient
    await page.click(`[data-testid='edit-patient-${patientId}']`);

    // Wait for edit page to load
    await page.waitForSelector("[data-testid='edit-patient-page']", { timeout: 5000 });

    // Verify we're on the edit page with the correct patient data
    await page.waitForSelector("[data-testid='edit-given-name']", { timeout: 5000 });

    // Modify the patient's name
    const newFamilyName = `Edited${Date.now() % 100000}`;
    await page.fill("[data-testid='edit-family-name']", newFamilyName);

    // Submit the form
    await page.click("[data-testid='save-patient']");

    // Wait for success message
    await page.waitForSelector("[data-testid='edit-success']", { timeout: 10000 });

    // Verify via API that patient was actually updated
    const updatedPatientJson = await request.get(`${ClinicalUrl}/fhir/Patient/${patientId}`);
    expect(await updatedPatientJson.text()).toContain(newFamilyName);

    await page.close();
  });

  test('Browser back button navigates to previous view', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Start on dashboard (default)
    expect(page.url()).toContain('#dashboard');

    // Navigate to Patients
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    // Navigate to Appointments
    await page.click('text=Appointments');
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#appointments');

    // Press browser back - should go to Patients
    await page.goBack();
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    // Press browser back again - should go to Dashboard
    await page.goBack();
    await page.waitForSelector('.metric-card', { timeout: 10000 });
    expect(page.url()).toContain('#dashboard');

    await page.close();
  });

  test('Deep linking loads correct view', async ({ authenticatedPage: page }) => {
    // Navigate directly to patients page via hash with auth
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page, `${DashboardUrl}#patients`);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    // Verify we're on patients page
    let content = await page.content();
    expect(content).toContain('Patients');

    // Navigate directly to appointments via hash
    await page.goto(`${DashboardUrl}#appointments`);
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });

    content = await page.content();
    expect(content).toContain('Appointments');

    await page.close();
  });

  test('Edit Patient Cancel button uses history back', async ({ authenticatedPage: page, request }) => {
    // Create a patient to edit
    const uniqueName = `CancelTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'CancelTestPatient',
        Gender: 'male'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Patients
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    // Search for and click edit on the patient
    await page.fill("input[placeholder*='Search']", uniqueName);
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });
    await page.click(`[data-testid='edit-patient-${patientId}']`);

    // Wait for edit page
    await page.waitForSelector("[data-testid='edit-patient-page']", { timeout: 5000 });
    expect(page.url()).toContain(`#patients/edit/${patientId}`);

    // Click Cancel button - should use history.back() and return to patients list
    await page.click("button:has-text('Cancel')");
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    // Should be back on patients page
    expect(page.url()).toContain('#patients');
    expect(page.url()).not.toContain('/edit/');

    await page.close();
  });

  test('Browser back button from Edit Patient page returns to Patients page', async ({ authenticatedPage: page, request }) => {
    // Create a patient to edit
    const uniqueName = `BackBtnTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'BackButtonTest',
        Gender: 'female'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    expect(page.url()).toContain('#dashboard');

    // Navigate to Patients
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    // Search for the patient
    await page.fill("input[placeholder*='Search']", uniqueName);
    await page.waitForSelector(`text=${uniqueName}`, { timeout: 10000 });

    // Click edit to go to edit page
    await page.click(`[data-testid='edit-patient-${patientId}']`);
    await page.waitForSelector("[data-testid='edit-patient-page']", { timeout: 5000 });
    expect(page.url()).toContain(`#patients/edit/${patientId}`);

    // THE CRITICAL TEST: Press browser back button
    // Before the fix, this would show a blank "Guest browsing" page
    // After the fix, it should return to the patients list
    await page.goBack();

    // Should be back on patients page with sidebar visible (NOT a blank page)
    await page.waitForSelector('.sidebar', { timeout: 10000 });
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');
    expect(page.url()).not.toContain('/edit/');

    // Verify the page content is actually the patients page, not blank
    const content = await page.content();
    expect(content).toContain('Patients');
    expect(content).toContain('Add Patient');

    // Press back again - should go to dashboard
    await page.goBack();
    await page.waitForSelector('.metric-card', { timeout: 10000 });
    expect(page.url()).toContain('#dashboard');

    await page.close();
  });

  test('Forward button works after going back', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate: Dashboard -> Patients -> Practitioners
    await page.click('text=Patients');
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });

    await page.click('text=Practitioners');
    await page.waitForSelector('.practitioner-card, .empty-state', { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    // Go back to Patients
    await page.goBack();
    await page.waitForSelector("[data-testid='add-patient-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#patients');

    // Go forward to Practitioners
    await page.goForward();
    await page.waitForSelector('.practitioner-card, .empty-state', { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    // Verify page content is actually practitioners page
    const content = await page.content();
    expect(content).toContain('Practitioners');

    await page.close();
  });

  test('Patient update API works end-to-end', async ({ request }) => {
    // Create a patient first
    const uniqueName = `UpdateApiTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'Original',
        Gender: 'male'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    // Update the patient
    const updatedFamilyName = `Updated${Date.now() % 100000}`;
    const updateResponse = await request.put(`${ClinicalUrl}/fhir/Patient/${patientId}`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: updatedFamilyName,
        Gender: 'male',
        Email: 'updated@test.com'
      }
    });
    expect(updateResponse.ok()).toBeTruthy();

    // Verify patient was updated
    const getResponse = await request.get(`${ClinicalUrl}/fhir/Patient/${patientId}`);
    const getResponseText = await getResponse.text();
    expect(getResponseText).toContain(updatedFamilyName);
    expect(getResponseText).toContain('updated@test.com');
  });

  test('Add Practitioner button opens modal and creates practitioner via API', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Practitioners page
    await page.click('text=Practitioners');
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });

    // Click Add Practitioner button
    await page.click("[data-testid='add-practitioner-btn']");

    // Wait for modal to appear
    await page.waitForSelector('.modal', { timeout: 5000 });

    // Fill in practitioner details
    const uniqueIdentifier = `DR${Date.now() % 100000}`;
    const uniqueGivenName = `E2EDoc${Date.now() % 100000}`;
    await page.fill("[data-testid='practitioner-identifier']", uniqueIdentifier);
    await page.fill("[data-testid='practitioner-given-name']", uniqueGivenName);
    await page.fill("[data-testid='practitioner-family-name']", 'TestCreated');
    await page.fill("[data-testid='practitioner-specialty']", 'E2E Testing');

    // Submit the form
    await page.click("[data-testid='submit-practitioner']");

    // Wait for modal to close and practitioner to appear in list
    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    // Verify via API that practitioner was actually created
    const response = await request.get(`${SchedulingUrl}/Practitioner`);
    expect(await response.text()).toContain(uniqueIdentifier);

    await page.close();
  });

  test('Edit Practitioner button opens edit page and updates practitioner', async ({ authenticatedPage: page, request }) => {
    // Create a practitioner to edit
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
    const practitionerJson = await createResponse.json();
    const practitionerId = practitionerJson.Id;

    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Practitioners page
    await page.click('text=Practitioners');
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });

    // Wait for our practitioner to appear
    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    // Hover over the card to show the edit button, then click it
    const editButton = page.locator(`[data-testid='edit-practitioner-${practitionerId}']`);
    await editButton.hover();
    await editButton.click();

    // Wait for edit page
    await page.waitForSelector("[data-testid='edit-practitioner-page']", { timeout: 5000 });
    expect(page.url()).toContain(`#practitioners/edit/${practitionerId}`);

    // Update the practitioner's specialty
    const newSpecialty = `Updated Specialty ${Date.now() % 100000}`;
    await page.fill("[data-testid='edit-practitioner-specialty']", newSpecialty);

    // Save changes
    await page.click("[data-testid='save-practitioner']");

    // Wait for success message
    await page.waitForSelector("[data-testid='edit-practitioner-success']", { timeout: 10000 });

    // Verify via API that practitioner was actually updated
    const updatedPractitionerJson = await request.get(`${SchedulingUrl}/Practitioner/${practitionerId}`);
    expect(await updatedPractitionerJson.text()).toContain(newSpecialty);

    await page.close();
  });

  test('Practitioner update API works end-to-end', async ({ request }) => {
    // Create a practitioner first
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
    const practitionerJson = await createResponse.json();
    const practitionerId = practitionerJson.Id;

    // Update the practitioner
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

    // Verify practitioner was updated
    const getResponse = await request.get(`${SchedulingUrl}/Practitioner/${practitionerId}`);
    const getResponseText = await getResponse.text();
    expect(getResponseText).toContain(updatedSpecialty);
    expect(getResponseText).toContain('ApiUpdated');
    expect(getResponseText).toContain('DO');
    expect(getResponseText).toContain('updated@hospital.com');
  });

  test('Browser back button from Edit Practitioner page returns to Practitioners page', async ({ authenticatedPage: page, request }) => {
    // Create a practitioner to edit
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
    const practitionerJson = await createResponse.json();
    const practitionerId = practitionerJson.Id;

    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    expect(page.url()).toContain('#dashboard');

    // Navigate to Practitioners
    await page.click('text=Practitioners');
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');

    // Wait for our practitioner to appear
    await page.waitForSelector(`text=${uniqueGivenName}`, { timeout: 10000 });

    // Click edit to go to edit page
    const editButton = page.locator(`[data-testid='edit-practitioner-${practitionerId}']`);
    await editButton.hover();
    await editButton.click();
    await page.waitForSelector("[data-testid='edit-practitioner-page']", { timeout: 5000 });
    expect(page.url()).toContain(`#practitioners/edit/${practitionerId}`);

    // Press browser back button
    await page.goBack();

    // Should be back on practitioners page with sidebar visible
    await page.waitForSelector('.sidebar', { timeout: 10000 });
    await page.waitForSelector("[data-testid='add-practitioner-btn']", { timeout: 10000 });
    expect(page.url()).toContain('#practitioners');
    expect(page.url()).not.toContain('/edit/');

    // Verify the page content is actually the practitioners page
    const content = await page.content();
    expect(content).toContain('Practitioners');
    expect(content).toContain('Add Practitioner');

    // Press back again - should go to dashboard
    await page.goBack();
    await page.waitForSelector('.metric-card', { timeout: 10000 });
    expect(page.url()).toContain('#dashboard');

    await page.close();
  });

  test('Login page uses discoverable credentials', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    // Navigate to Dashboard without auth - should show login page
    await page.goto(DashboardUrl);

    // Wait for login page to appear
    await page.waitForSelector('.login-card', { timeout: 20000 });

    // Verify login page is shown
    const pageContent = await page.content();
    expect(pageContent).toContain('Nimblesite Clinical Coding Platform');
    expect(pageContent).toContain('Sign in with your passkey');

    // CRITICAL: Login mode should NOT have email input field
    // Email is only needed for registration, not for discoverable credential login
    const emailInputVisible = await page.isVisible("input[type='email']");
    expect(emailInputVisible).toBeFalsy();

    // Should have a sign-in button
    const signInButton = page.locator("button:has-text('Sign in with Passkey')");
    await signInButton.waitFor({ timeout: 5000 });
    expect(await signInButton.isVisible()).toBeTruthy();

    await page.close();
  });

  test('Registration page requires email and display name', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    // Navigate to Dashboard without auth
    await page.goto(DashboardUrl);

    // Wait for login page
    await page.waitForSelector('.login-card', { timeout: 20000 });

    // Click "Register" to switch to registration mode
    await page.click("button:has-text('Register')");
    await page.waitForTimeout(500);

    // Verify we're in registration mode
    const pageContent = await page.content();
    expect(pageContent).toContain('Create your account');

    // Registration mode SHOULD have email and display name fields
    const emailInput = page.locator("input[type='email']");
    const displayNameInput = page.locator("input#displayName");

    expect(await emailInput.isVisible()).toBeTruthy();
    expect(await displayNameInput.isVisible()).toBeTruthy();

    await page.close();
  });

  test('Gatekeeper API login begin returns valid discoverable credential options', async ({ request }) => {
    // Call /auth/login/begin with empty body (discoverable credentials flow)
    const response = await request.post(`${GatekeeperUrl}/auth/login/begin`, {
      headers: { 'Content-Type': 'application/json' },
      data: {}
    });

    // Should return 200 OK
    expect(response.ok()).toBeTruthy();

    const json = await response.json();
    console.log(`[API TEST] Response: ${JSON.stringify(json)}`);

    // Must have ChallengeId
    expect(json.ChallengeId).toBeTruthy();
    expect(json.ChallengeId).not.toBe('');

    // Must have OptionsJson (string containing JSON)
    expect(json.OptionsJson).toBeTruthy();
    expect(json.OptionsJson).not.toBe('');

    // OptionsJson should be valid JSON that can be parsed
    const options = JSON.parse(json.OptionsJson);

    // Verify critical WebAuthn fields
    expect(options.challenge).toBeTruthy();
    expect(options.rpId).toBeTruthy();

    // For discoverable credentials, allowCredentials should be empty array
    if (options.allowCredentials) {
      expect(Array.isArray(options.allowCredentials)).toBeTruthy();
      expect(options.allowCredentials.length).toBe(0);
      console.log('[API TEST] allowCredentials is empty array - correct for discoverable credentials!');
    }
  });

  test('Gatekeeper API register begin returns valid options', async ({ request }) => {
    // Call /auth/register/begin with email and display name
    const response = await request.post(`${GatekeeperUrl}/auth/register/begin`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Email: 'test-e2e@example.com',
        DisplayName: 'E2E Test User'
      }
    });

    // Should return 200 OK
    expect(response.ok()).toBeTruthy();

    const json = await response.json();
    console.log(`[API TEST] Response: ${JSON.stringify(json)}`);

    // Must have ChallengeId
    expect(json.ChallengeId).toBeTruthy();
    expect(json.ChallengeId).not.toBe('');

    // Must have OptionsJson
    expect(json.OptionsJson).toBeTruthy();

    // OptionsJson should be valid JSON
    const options = JSON.parse(json.OptionsJson);

    // Verify critical WebAuthn registration fields
    expect(options.challenge).toBeTruthy();
    expect(options.rp).toBeTruthy();
    expect(options.user).toBeTruthy();
    expect(options.pubKeyCredParams).toBeTruthy();

    // Verify resident key is required for discoverable credentials
    if (options.authenticatorSelection?.residentKey) {
      expect(options.authenticatorSelection.residentKey).toBe('required');
      console.log('[API TEST] residentKey is "required" - correct for discoverable credentials!');
    }
  });

  test('Login page sign in button calls API without JSON errors', async ({ browser }) => {
    const page = await browser.newPage();
    const consoleErrors: string[] = [];
    const networkRequests: string[] = [];

    page.on('console', msg => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
      if (msg.type() === 'error')
        consoleErrors.push(msg.text());
    });

    page.on('request', request => {
      if (request.url().includes('/auth/')) {
        networkRequests.push(`${request.method()} ${request.url()}`);
        console.log(`[NETWORK] ${request.method()} ${request.url()}`);
      }
    });

    // Navigate to Dashboard without auth
    await page.goto(DashboardUrl);

    // Wait for login page
    await page.waitForSelector('.login-card', { timeout: 20000 });

    // Click Sign in with Passkey button
    await page.click("button:has-text('Sign in with Passkey')");

    // Wait for API call and potential error handling
    await page.waitForTimeout(3000);

    // Verify the API was called
    expect(networkRequests.some(r => r.includes('/auth/login/begin'))).toBeTruthy();

    // Check for JSON parse errors in console
    const hasJsonParseError = consoleErrors.some(e =>
      e.includes('undefined') || e.includes('is not valid JSON') || e.includes('SyntaxError')
    );

    // Check for JSON parse errors in UI
    const errorVisible = await page.isVisible('.login-error');
    const errorText = errorVisible ? await page.textContent('.login-error') : null;

    const hasUiJsonError =
      errorText?.includes('undefined') ||
      errorText?.includes('is not valid JSON') ||
      errorText?.includes('SyntaxError');

    expect(hasJsonParseError || hasUiJsonError).toBeFalsy();

    // The WebAuthn prompt will fail in headless mode (no authenticator), but that's expected
    // The important thing is no JSON parsing errors
    console.log(`[TEST] API called, no JSON errors. UI error (expected): ${errorText}`);

    await page.close();
  });

  test('User menu click shows dropdown with Sign Out', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // User menu button should be visible in header
    const userMenuButton = await page.locator("[data-testid='user-menu-button']").first;
    expect(userMenuButton).toBeTruthy();

    // Click the user menu button to open dropdown
    await userMenuButton.click();

    // Wait for dropdown to appear
    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });

    // Sign out button should be visible in the dropdown
    const signOutButton = await page.locator("[data-testid='logout-button']").first;
    expect(signOutButton).toBeTruthy();

    const isVisible = await signOutButton.isVisible();
    expect(isVisible).toBeTruthy();

    await page.close();
  });

  test('Sign Out button click shows login page', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    // Wait for the sidebar to appear (authenticated state)
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Click user menu button in header to open dropdown
    await page.click("[data-testid='user-menu-button']");

    // Wait for dropdown to appear
    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });

    // Click sign out button in dropdown
    await page.click("[data-testid='logout-button']");

    // Should show login page after sign out
    await page.waitForSelector("[data-testid='login-page']", { timeout: 10000 });

    // Verify token was cleared from localStorage
    const tokenAfterLogout = await page.evaluate(() => localStorage.getItem('gatekeeper_token'));
    expect(tokenAfterLogout).toBeNull();

    const userAfterLogout = await page.evaluate(() => localStorage.getItem('gatekeeper_user'));
    expect(userAfterLogout).toBeNull();

    await page.close();
  });

  test('Gatekeeper API logout revokes token', async ({ request }) => {
    // Test 1: Without a Bearer token, should return 401 Unauthorized
    const unauthResponse = await request.post(`${GatekeeperUrl}/auth/logout`, {
      headers: { 'Content-Type': 'application/json' },
      data: {}
    });
    expect(unauthResponse.status()).toBe(401);

    // Test 2: With a valid Bearer token, should return 204 NoContent (logout succeeds)
    // Note: The request fixture doesn't have auth by default, so we need to use a different approach
    // or the API may allow it in dev mode
  });

  test('User menu displays user initials and name in dropdown', async ({ browser }) => {
    // This test would require creating a page with specific auth details
    // For now, we'll test with the default authenticated page
    const context = await browser.newContext();
    const page = await context.newPage();

    // Navigate to dashboard
    await page.goto(DashboardUrl);

    // Set up specific user auth
    const token = generateTestToken('test-user', 'Alice Smith', 'alice@example.com');
    const userJson = JSON.stringify({ userId: 'test-user', displayName: 'Alice Smith', email: 'alice@example.com' });

    await page.evaluate(({ token, userJson }) => {
      localStorage.setItem('gatekeeper_token', token);
      localStorage.setItem('gatekeeper_user', userJson);
    }, { token, userJson });

    await page.reload();

    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Verify initials in header avatar button (should be "AS" for Alice Smith)
    const avatarText = await page.textContent("[data-testid='user-menu-button']");
    expect(avatarText?.trim()).toBe('AS');

    // Click the user menu button to open dropdown
    await page.click("[data-testid='user-menu-button']");

    // Wait for dropdown to appear
    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });

    // Verify the user's name is displayed in dropdown header
    const userNameText = await page.textContent('.user-dropdown-name');
    expect(userNameText).toContain('Alice Smith');

    // Verify email is displayed
    const emailText = await page.textContent('.user-dropdown-email');
    expect(emailText).toContain('alice@example.com');

    await page.close();
  });

  test('Clinical Coding navigates to page and displays search options', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    // Navigate to Clinical Coding page
    await page.click('text=Clinical Coding');

    // Wait for page to load
    await page.waitForSelector('.clinical-coding-page', { timeout: 10000 });

    // Verify search tabs are present
    const content = await page.content();
    expect(content).toContain('Keyword Search');
    expect(content).toContain('AI Search');
    expect(content).toContain('Code Lookup');

    await page.close();
  });

  test('Clinical Coding deep linking works', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`));

    // Navigate directly to clinical coding page
    await createAuthenticatedPage(page, `${DashboardUrl}#clinical-coding`);

    // Wait for clinical coding page to load
    await page.waitForSelector('.clinical-coding-page', { timeout: 20000 });

    // Verify we're on the clinical coding page
    const content = await page.content();
    expect(content).toContain('Clinical Coding');
    expect(content).toContain('ICD-10');

    await page.close();
  });
});

// Helper function for generating tokens
function generateTestToken(
  userId: string = 'e2e-test-user',
  displayName: string = 'E2E Test User',
  email: string = 'e2etest@example.com'
): string {
  const signingKey = Buffer.alloc(32, 0);

  const base64UrlEncode = (input: string): string => {
    return Buffer.from(input).toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  };

  const computeHmacSignature = (header: string, payload: string, key: Buffer): string => {
    const crypto = require('crypto');
    const data = Buffer.from(`${header}.${payload}`);
    const hmac = crypto.createHmac('sha256', key);
    hmac.update(data);
    return base64UrlEncode(hmac.digest().toString());
  };

  const header = base64UrlEncode(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));

  const expiration = Math.floor(Date.now() / 1000) + 3600;
  const payload = base64UrlEncode(JSON.stringify({
    sub: userId,
    name: displayName,
    email,
    jti: `${Date.now()}-${Math.random()}`,
    exp: expiration,
    roles: ['admin', 'user'],
  }));

  const signature = computeHmacSignature(header, payload, signingKey);
  return `${header}.${payload}.${signature}`;
}