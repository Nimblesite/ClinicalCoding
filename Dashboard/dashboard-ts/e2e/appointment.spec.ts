/**
 * E2E tests for appointment-related functionality.
 * Ported from AppointmentE2ETests.cs
 */

import { expect, Page, SchedulingUrl, test } from '../support/fixture';

/**
 * Create an authenticated page and optionally navigate to a specific URL
 */
async function createAuthenticatedPage(page: Page, navigateTo?: string): Promise<Page> {
  // Auth is already set up by the fixture
  if (navigateTo) {
    await page.goto(navigateTo);
  }
  return page;
}

test.describe('Appointment E2E Tests', () => {
  test('Dashboard displays appointment data from Scheduling API', async ({
    authenticatedPage: page,
  }) => {
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Appointments');
    await page.waitForSelector('text=Checkup', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('Checkup');

    await page.close();
  });

  test('Add Appointment button opens modal and creates appointment via API', async ({
    authenticatedPage: page,
    request,
  }) => {
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Appointments');
    await page.waitForSelector("[data-testid='add-appointment-btn']", { timeout: 10000 });
    await page.click("[data-testid='add-appointment-btn']");
    await page.waitForSelector('.modal', { timeout: 5000 });

    const uniqueServiceType = `E2EConsult${Date.now() % 100000}`;
    await page.fill("[data-testid='appointment-service-type']", uniqueServiceType);
    await page.click("[data-testid='submit-appointment']");

    await page.waitForSelector(`text=${uniqueServiceType}`, { timeout: 10000 });

    // Verify via API
    const response = await request.get(`${SchedulingUrl}/Appointment`);
    expect(response.ok()).toBeTruthy();
    const responseText = await response.text();
    expect(responseText).toContain(uniqueServiceType);

    await page.close();
  });

  test('View Schedule button navigates to appointments', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=View Schedule');
    await page.waitForSelector('text=Appointments', { timeout: 5000 });
    await page.waitForSelector('text=Checkup', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('Checkup');

    await page.close();
  });

  test('Edit Appointment button opens edit page and updates appointment via API', async ({
    authenticatedPage: page,
    request,
  }) => {
    page.on('console', (msg) => console.log(`[BROWSER] ${msg.text()}`));

    // Create appointment via API first
    const uniqueServiceType = `EditApptTest${Date.now() % 100000}`;
    const startTime = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString();
    const endTime = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000 + 30 * 60 * 1000).toISOString();

    const createResponse = await request.post(`${SchedulingUrl}/Appointment`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        ServiceCategory: 'General',
        ServiceType: uniqueServiceType,
        Priority: 'routine',
        Start: startTime,
        End: endTime,
        PatientReference: 'Patient/1',
        PractitionerReference: 'Practitioner/1',
      },
    });
    expect(createResponse.ok()).toBeTruthy();

    const createdAppointmentJson = await createResponse.text();
    const appointmentIdMatch = createdAppointmentJson.match(/"Id"\s*:\s*"([^"]+)"/);
    expect(appointmentIdMatch).toBeTruthy();
    const appointmentId = appointmentIdMatch![1];

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Appointments');
    await page.waitForSelector(`text=${uniqueServiceType}`, { timeout: 10000 });

    const editButton = await page.locator(`tr:has-text('${uniqueServiceType}') .btn-secondary`)
      .first;
    expect(editButton).toBeTruthy();
    await editButton.click();

    await page.waitForSelector('text=Edit Appointment', { timeout: 5000 });

    const newServiceType = `Edited${Date.now() % 100000}`;
    await page.fill('#appointment-service-type', newServiceType);
    await page.click("button:has-text('Save Changes')");

    await page.waitForSelector('text=Appointment updated successfully', { timeout: 10000 });

    // Verify via API
    const updatedAppointmentJson = await request.get(
      `${SchedulingUrl}/Appointment/${appointmentId}`,
    );
    expect(await updatedAppointmentJson.text()).toContain(newServiceType);

    await page.close();
  });
});
