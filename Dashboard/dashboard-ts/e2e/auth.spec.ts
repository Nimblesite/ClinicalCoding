/**
 * E2E tests for authentication (login, logout, WebAuthn).
 * TypeScript port of C# AuthE2ETests.cs
 */

import { test, expect, Page, APIRequestContext } from '@playwright/test';
import { DashboardUrl, GatekeeperUrl, setupAuth, generateTestToken } from './support/fixture';

test.describe('Auth E2E Tests', () => {
  /**
   * Login page uses discoverable credentials (no email required).
   */
  test('LoginPage_DoesNotRequireEmailForSignIn', async ({ page }) => {
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    await page.goto(DashboardUrl);
    await page.evaluate(() => {
      localStorage.removeItem('gatekeeper_token');
      localStorage.removeItem('gatekeeper_user');
    });
    await page.reload();
    await page.waitForSelector('.login-card', { timeout: 20000 });

    const pageContent = await page.content();
    expect(pageContent).toContain('Nimblesite Clinical Coding Platform');
    expect(pageContent).toContain('Sign in with your passkey');

    const emailInputVisible = await page.isVisible("input[type='email']");
    expect(emailInputVisible).toBe(false);

    const signInButton = page.locator("button:has-text('Sign in with Passkey')");
    await signInButton.waitFor({ timeout: 5000 });
    expect(await signInButton.isVisible()).toBe(true);
  });

  /**
   * Registration page requires email and display name.
   */
  test('LoginPage_RegistrationRequiresEmailAndDisplayName', async ({ page }) => {
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    await page.goto(DashboardUrl);
    await page.evaluate(() => {
      localStorage.removeItem('gatekeeper_token');
      localStorage.removeItem('gatekeeper_user');
    });
    await page.reload();
    await page.waitForSelector('.login-card', { timeout: 20000 });

    await page.click("button:has-text('Register')");
    await page.waitForTimeout(500);

    const pageContent = await page.content();
    expect(pageContent).toContain('Create your account');

    const emailInput = page.locator("input[type='email']");
    const displayNameInput = page.locator("input#displayName");

    expect(await emailInput.isVisible()).toBe(true);
    expect(await displayNameInput.isVisible()).toBe(true);
  });

  /**
   * Gatekeeper API /auth/login/begin returns valid response for discoverable credentials.
   */
  test('GatekeeperApi_LoginBegin_ReturnsValidDiscoverableCredentialOptions', async ({ request }) => {
    const response = await request.post(`${GatekeeperUrl}/auth/login/begin`, {
      headers: { 'Content-Type': 'application/json' },
      data: '{}',
    });

    expect(response.ok()).toBe(true);

    const json = await response.json();
    expect(json).toHaveProperty('ChallengeId');
    expect(json.ChallengeId).toBeTruthy();

    expect(json).toHaveProperty('OptionsJson');
    const optionsJsonStr = json.OptionsJson as string;
    expect(optionsJsonStr).toBeTruthy();

    const options = JSON.parse(optionsJsonStr);
    expect(options).toHaveProperty('challenge');
    expect(options).toHaveProperty('rpId');
  });

  /**
   * Gatekeeper API /auth/register/begin returns valid response.
   */
  test('GatekeeperApi_RegisterBegin_ReturnsValidOptions', async ({ request }) => {
    const response = await request.post(`${GatekeeperUrl}/auth/register/begin`, {
      headers: { 'Content-Type': 'application/json' },
      data: JSON.stringify({
        Email: 'test-e2e@example.com',
        DisplayName: 'E2E Test User',
      }),
    });

    expect(response.ok()).toBe(true);

    const json = await response.json();
    expect(json).toHaveProperty('ChallengeId');
    expect(json).toHaveProperty('OptionsJson');

    const options = JSON.parse(json.OptionsJson as string);
    expect(options).toHaveProperty('challenge');
    expect(options).toHaveProperty('rp');
    expect(options).toHaveProperty('user');
  });

  /**
   * Dashboard sign-in flow calls API and handles response correctly.
   */
  test('LoginPage_SignInButton_CallsApiWithoutJsonErrors', async ({ page }) => {
    const consoleErrors: string[] = [];
    const networkRequests: string[] = [];

    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    page.on('request', (request) => {
      if (request.url().includes('/auth/')) {
        networkRequests.push(`${request.method()} ${request.url()}`);
      }
    });

    await page.goto(DashboardUrl);
    await page.evaluate(() => {
      localStorage.removeItem('gatekeeper_token');
      localStorage.removeItem('gatekeeper_user');
    });
    await page.reload();
    await page.waitForSelector('.login-card', { timeout: 20000 });

    await page.click("button:has-text('Sign in with Passkey')");
    await page.waitForTimeout(3000);

    expect(networkRequests.some((r) => r.includes('/auth/login/begin'))).toBe(true);

    const hasJsonParseError = consoleErrors.some(
      (e) => e.includes('undefined') || e.includes('is not valid JSON') || e.includes('SyntaxError')
    );
    expect(hasJsonParseError).toBe(false);
  });

  /**
   * User menu click shows dropdown with Sign Out.
   */
  test('UserMenu_ClickShowsDropdownWithSignOut', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    // Setup auth
    await setupAuth(page);

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    const userMenuButton = await page.$("[data-testid='user-menu-button']");
    expect(userMenuButton).not.toBeNull();
    await userMenuButton!.click();

    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });

    const signOutButton = await page.$("[data-testid='logout-button']");
    expect(signOutButton).not.toBeNull();
    expect(await signOutButton!.isVisible()).toBe(true);

    await page.close();
  });

  /**
   * Sign Out button click shows login page.
   */
  test('SignOutButton_ClickShowsLoginPage', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    // Setup auth
    await setupAuth(page);

    await page.waitForSelector('.sidebar', { timeout: 20000 });

    await page.click("[data-testid='user-menu-button']");
    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });
    await page.click("[data-testid='logout-button']");

    await page.waitForSelector("[data-testid='login-page']", { timeout: 10000 });

    const tokenAfterLogout = await page.evaluate(() =>
      localStorage.getItem('gatekeeper_token')
    );
    expect(tokenAfterLogout).toBeNull();

    await page.close();
  });

  /**
   * Gatekeeper API logout revokes token.
   */
  test('GatekeeperApi_Logout_RevokesToken', async ({ request }) => {
    const token = generateTestToken();

    const logoutResponse = await request.post(`${GatekeeperUrl}/auth/logout`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      data: '{}',
    });
    expect(logoutResponse.status()).toBe(204);

    const unauthResponse = await request.post(`${GatekeeperUrl}/auth/logout`, {
      headers: { 'Content-Type': 'application/json' },
      data: '{}',
    });
    expect(unauthResponse.status()).toBe(401);
  });

  /**
   * User menu displays user initials and name in dropdown.
   */
  test('UserMenu_DisplaysUserInitialsAndNameInDropdown', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    // Generate valid test token for custom user
    const testToken = generateTestToken(
      'test-user',
      'Alice Smith',
      'alice@example.com'
    );

    // Set custom user data BEFORE loading
    await page.goto(DashboardUrl);
    await page.evaluate(
      ({ token }: { token: string }) => {
        localStorage.setItem('gatekeeper_token', token);
        localStorage.setItem(
          'gatekeeper_user',
          JSON.stringify({
            userId: 'test-user',
            displayName: 'Alice Smith',
            email: 'alice@example.com',
          })
        );
      },
      { token: testToken }
    );

    // Reload to pick up custom user data
    await page.reload();
    await page.waitForSelector('.sidebar', { timeout: 20000 });

    const avatarText = await page.textContent("[data-testid='user-menu-button']");
    expect(avatarText?.trim()).toBe('AS');

    await page.click("[data-testid='user-menu-button']");
    await page.waitForSelector("[data-testid='user-dropdown']", { timeout: 5000 });

    const userNameText = await page.textContent('.user-dropdown-name');
    expect(userNameText).toContain('Alice Smith');

    const emailText = await page.textContent('.user-dropdown-email');
    expect(emailText).toContain('alice@example.com');

    await page.close();
  });

  /**
   * First-time sign-in must work WITHOUT browser refresh.
   */
  test('FirstTimeSignIn_TransitionsToDashboard_WithoutRefresh', async ({ browser }) => {
    const page = await browser.newPage();
    page.on('console', (msg) => {
      console.log(`[BROWSER] ${msg.type()}: ${msg.text()}`);
    });

    await page.goto(DashboardUrl);
    await page.evaluate(() => {
      localStorage.removeItem('gatekeeper_token');
      localStorage.removeItem('gatekeeper_user');
    });
    await page.reload();
    await page.waitForSelector("[data-testid='login-page']", { timeout: 20000 });

    // Wait for React to mount and set the __triggerLogin hook
    await page.waitForFunction(
      () => typeof (window as { __triggerLogin?: unknown }).__triggerLogin === 'function',
      { timeout: 10000 }
    );

    // Generate a valid test token
    const devToken = generateTestToken(
      'test-user-123',
      'Test User',
      'test@example.com'
    );
    await page.evaluate(
      ({ token }: { token: string }) => {
        console.log('[TEST] Setting token and triggering login');
        localStorage.setItem('gatekeeper_token', token);
        localStorage.setItem(
          'gatekeeper_user',
          JSON.stringify({
            userId: 'test-user-123',
            displayName: 'Test User',
            email: 'test@example.com',
          })
        );
        (window as { __triggerLogin?: (user: unknown) => void }).__triggerLogin!({
          userId: 'test-user-123',
          displayName: 'Test User',
          email: 'test@example.com',
        });
        console.log('[TEST] Login triggered, waiting for React state update');
      },
      { token: devToken }
    );

    // Wait longer for React state update and re-render
    await page.waitForTimeout(2000);

    await page.waitForSelector('.sidebar', { timeout: 10000 });
    const loginPageStillVisible = await page.isVisible("[data-testid='login-page']");
    expect(loginPageStillVisible).toBe(false);
    expect(await page.isVisible('.sidebar')).toBe(true);

    await page.close();
  });
});