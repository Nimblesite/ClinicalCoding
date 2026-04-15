/**
 * E2E tests for calendar-related functionality.
 * Ported from CalendarE2ETests.cs
 */

import { test, expect, Page } from './support/fixture';
import { SchedulingUrl, DashboardUrl } from './support/fixture';

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

test.describe('Calendar E2E Tests', () => {
  test('Calendar page displays appointments in calendar grid', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page, `${DashboardUrl}#calendar`);
    page.on('console', msg => console.log(`[BROWSER ${msg.type()}] ${msg.text()}`));
    page.on('pageerror', err => console.log(`[PAGE ERROR] ${err}`));

    // Debug: Check auth state
    const hasToken = await page.evaluate(() => !!localStorage.getItem('gatekeeper_token'));
    const hasUser = await page.evaluate(() => !!localStorage.getItem('gatekeeper_user'));
    const currentUrl = page.url();
    console.log(`[DEBUG] Auth state - hasToken: ${hasToken}, hasUser: ${hasUser}, URL: ${currentUrl}`);

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    await page.waitForSelector('.calendar-grid-container', { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain('calendar-grid');
    expect(content).toContain('Sun');
    expect(content).toContain('Mon');
    expect(content).toContain('Today');

    await page.close();
  });

  test('Calendar page click on day shows appointment details', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    
    // Create appointment for today
    const today = new Date();
    const startTime = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 14, 0, 0).toISOString();
    const endTime = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 14, 30, 0).toISOString();
    const uniqueServiceType = `CalTest${Date.now() % 100000}`;

    const createResponse = await request.post(`${SchedulingUrl}/Appointment`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        ServiceCategory: 'General',
        ServiceType: uniqueServiceType,
        Priority: 'routine',
        Start: startTime,
        End: endTime,
        PatientReference: 'Patient/1',
        PractitionerReference: 'Practitioner/1'
      }
    });
    expect(createResponse.ok()).toBeTruthy();

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Schedule');
    await page.waitForSelector('.calendar-grid', { timeout: 10000 });
    await page.waitForSelector('.calendar-cell.today.has-appointments', { timeout: 10000 });

    const todayCell = page.locator('.calendar-cell.today').first();
    await todayCell.click();

    await page.waitForSelector('.calendar-details-panel h4', { timeout: 5000 });
    await page.waitForSelector(`text=${uniqueServiceType}`, { timeout: 10000 });

    const content = await page.content();
    expect(content).toContain(uniqueServiceType);

    await page.close();
  });

  test('Calendar page Edit button opens edit appointment page', async ({ authenticatedPage: page, request }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    
    // Create appointment for today
    const today = new Date();
    const startTime = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 15, 0, 0).toISOString();
    const endTime = new Date(today.getFullYear(), today.getMonth(), today.getDate(), 15, 30, 0).toISOString();
    const uniqueServiceType = `CalEdit${Date.now() % 100000}`;

    const createResponse = await request.post(`${SchedulingUrl}/Appointment`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        ServiceCategory: 'General',
        ServiceType: uniqueServiceType,
        Priority: 'routine',
        Start: startTime,
        End: endTime,
        PatientReference: 'Patient/1',
        PractitionerReference: 'Practitioner/1'
      }
    });
    expect(createResponse.ok()).toBeTruthy();

    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Schedule');
    await page.waitForSelector('.calendar-grid', { timeout: 10000 });
    await page.waitForSelector('.calendar-cell.today.has-appointments', { timeout: 10000 });

    const todayCell = page.locator('.calendar-cell.today').first();
    await todayCell.click();
    await page.waitForSelector(`text=${uniqueServiceType}`, { timeout: 10000 });

    const editButton = await page.locator(`.calendar-appointment-item:has-text('${uniqueServiceType}') button:has-text('Edit')`).first;
    expect(editButton).toBeTruthy();
    await editButton.click();

    await page.waitForSelector('text=Edit Appointment', { timeout: 5000 });

    const content = await page.content();
    expect(content).toContain('Edit Appointment');

    await page.close();
  });

  test('Calendar navigation buttons change month', async ({ authenticatedPage: page }) => {
    page.on('console', msg => console.log(`[BROWSER] ${msg.text()}`));
    await createAuthenticatedPage(page);
    await page.waitForSelector('.sidebar', { timeout: 20000 });
    await page.click('text=Schedule');
    await page.waitForSelector('.calendar-grid', { timeout: 10000 });

    const currentMonthYear = await page.textContent('.text-lg.font-semibold');
    expect(currentMonthYear).toBeTruthy();

    const headerControls = page.locator('.page-header .flex.items-center.gap-4');
    const nextButton = headerControls.locator('button.btn-secondary').nth(1);
    await nextButton.click();
    await page.waitForTimeout(300);

    const newMonthYear = await page.textContent('.text-lg.font-semibold');
    expect(newMonthYear).not.toEqual(currentMonthYear);

    const prevButton = headerControls.locator('button.btn-secondary').first;
    await prevButton.click();
    await page.waitForTimeout(300);
    await prevButton.click();
    await page.waitForTimeout(300);

    await page.click("button:has-text('Today')");
    await page.waitForTimeout(500);

    const todayContent = await page.content();
    expect(todayContent).toContain('today');

    await page.close();
  });

  test('Deep linking to calendar page works', async ({ authenticatedPage: page }) => {
    await createAuthenticatedPage(page, `${DashboardUrl}#calendar`);
    page.on('console', msg => console.log(`[BROWSER ${msg.type()}] ${msg.text()}`));
    page.on('pageerror', err => console.log(`[PAGE ERROR] ${err}`));

    // Debug: Check auth state and hash
    const hasToken = await page.evaluate(() => !!localStorage.getItem('gatekeeper_token'));
    const currentHash = await page.evaluate(() => window.location.hash);
    console.log(`[DEBUG] hasToken: ${hasToken}, hash: ${currentHash}, URL: ${page.url()}`);

    await page.waitForSelector('.calendar-grid', { timeout: 20000 });

    const content = await page.content();
    expect(content).toContain('Schedule');
    expect(content).toContain('calendar-grid');

    await page.close();
  });
});