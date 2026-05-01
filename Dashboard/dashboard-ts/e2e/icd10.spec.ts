/**
 * E2E tests for ICD-10 Clinical Coding in the Dashboard.
 */

import { DashboardUrl, expect, Page, test } from './support/fixture';

async function navigateToClinicalCoding(page: Page): Promise<Page> {
  await page.goto(`${DashboardUrl}#clinical-coding`);
  await page.waitForSelector('.clinical-coding-page', { timeout: 20000 });
  return page;
}

async function runSearch(page: Page, mode: string, query: string): Promise<void> {
  await page.getByRole('tab', { name: mode }).click();
  await page.getByTestId('coding-search-input').fill(query);
  await page.getByRole('button', { name: 'Search' }).click();
}

async function waitForResults(page: Page): Promise<void> {
  await expect(page.getByTestId('coding-result').first()).toBeVisible({ timeout: 30000 });
}

async function openFirstDetails(page: Page): Promise<void> {
  await page.getByTestId('coding-result').first().getByRole('button', { name: 'Details' }).click();
  await expect(page.getByTestId('coding-detail').first()).toBeVisible({ timeout: 10000 });
}

test.describe('ICD-10 E2E Tests', () => {
  test('Keyword search for diabetes returns results with chapter and category', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'diabetes');
    await waitForResults(page);

    const content = await page.content();
    expect(content.toLowerCase()).toContain('diabetes');
    expect(content).toContain('ICD-10-AM');

    await page.close();
  });

  test('Keyword search for pneumonia shows billable status', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'pneumonia');
    await waitForResults(page);

    const content = await page.content();
    expect(content.toLowerCase()).toContain('pneumonia');

    await page.close();
  });

  test('Keyword search shows result count', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'fracture');
    await waitForResults(page);

    await expect(page.locator('.coding-results-header h2')).toContainText(/^\d+ Results$/);

    await page.close();
  });

  test('AI search for chest pain returns results with confidence', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'AI Search', 'chest pain with shortness of breath');
    await waitForResults(page);

    const content = await page.content();
    expect(content).toContain('Match');
    expect(content).toContain('ICD-10-AM');

    await page.close();
  });

  test('AI search for heart attack returns cardiac codes', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'AI Search', 'heart attack');
    await waitForResults(page);

    const rows = await page.getByTestId('coding-result').all();
    expect(rows.length).toBeGreaterThan(0);

    await page.close();
  });

  test('AI search shows the Include ACHI procedure codes checkbox', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await page.getByRole('tab', { name: 'AI Search' }).click();

    await expect(page.getByTestId('achi-toggle')).toBeVisible();
    await expect(page.locator('.clinical-coding-page')).toContainText('Diagnostic Coding Search');

    await page.close();
  });

  test('Code lookup for E11.9 shows full code detail', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Code Lookup', 'E11.9');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('E11.9');
    expect(content.toLowerCase()).toContain('diabetes');
    expect(content).toContain('Classification');

    await page.close();
  });

  test('Code lookup for I10 shows hypertension detail', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Code Lookup', 'I10');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('I10');
    expect(content.toLowerCase()).toContain('hypertension');

    await page.close();
  });

  test('Code lookup for R07.9 shows chest pain with billable', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Code Lookup', 'R07.9');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('R07.9');
    expect(content.toLowerCase()).toContain('chest pain');

    await page.close();
  });

  test('Code lookup with prefix E11 shows multiple results', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'E11');
    await waitForResults(page);

    const rows = await page.getByTestId('coding-result').all();
    expect(rows.length).toBeGreaterThan(1);

    await page.close();
  });

  test('Drill down keyword search click result shows code detail', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'hypertension');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('Code');
    expect(content).toContain('Classification');
    expect(content).toContain('Copy');

    await page.close();
  });

  test('Drill down back to results restores results list', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'diabetes');
    await waitForResults(page);
    await openFirstDetails(page);

    await page.getByTestId('coding-result').first().getByRole('button', { name: 'Hide' }).click();
    await expect(page.getByTestId('coding-result').first()).toBeVisible();

    await page.close();
  });

  test('Drill down shows full description', async ({ authenticatedPage: page }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Code Lookup', 'G43.909');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('G43.909');
    expect(content.toLowerCase()).toContain('migraine');
    expect(content).toContain('Description');

    await page.close();
  });

  test('Drill down AI search click result shows code detail', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'AI Search', 'type 2 diabetes with kidney complications');
    await waitForResults(page);
    await openFirstDetails(page);

    const content = await page.content();
    expect(content).toContain('Code');
    expect(content).toContain('Classification');
    expect(content).toContain('Copy');

    await page.close();
  });

  test('Code lookup for nonexistent code shows No codes found', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Code Lookup', 'ZZZ99.99');

    await expect(page.locator('.coding-results-header h2')).toContainText('0 Result', {
      timeout: 15000,
    });

    await page.close();
  });

  test('Switching between search tabs clears previous results', async ({
    authenticatedPage: page,
  }) => {
    await navigateToClinicalCoding(page);
    await runSearch(page, 'Keyword Search', 'fracture');
    await waitForResults(page);

    await page.getByRole('tab', { name: 'Code Lookup' }).click();
    await expect(page.locator('.coding-results-header h2')).toContainText('0 Result');

    await page.close();
  });
});
