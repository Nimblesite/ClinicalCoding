/**
 * CORS tests for Dashboard frontend to backend API communication.
 * Ported from DashboardApiCorsTests.cs
 */

import { test, expect } from './support/fixture';
import { ClinicalUrl, SchedulingUrl, GatekeeperUrl, DashboardUrl } from './support/fixture';

test.describe('Dashboard API CORS Tests', () => {
  // Dashboard origin - this is where the frontend runs
  const DashboardOrigin = DashboardUrl;

  test('Clinical API Patients endpoint allows CORS from Dashboard', async ({ request }) => {
    // Simulate browser preflight request
    const response = await request.fetch(`${ClinicalUrl}/fhir/Patient`, {
      method: 'OPTIONS',
      headers: {
        'Origin': DashboardOrigin,
        'Access-Control-Request-Method': 'GET',
        'Access-Control-Request-Headers': 'Accept',
      }
    });

    // CORS headers must be present
    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
    expect(allowOrigin === DashboardOrigin || allowOrigin === '*').toBeTruthy();
  });

  test('Clinical API GET Patients returns data with CORS headers', async ({ request }) => {
    // Simulate browser request with Origin header
    // Note: Clinical API uses /fhir/Patient/ (with trailing slash) for list
    const response = await request.get(`${ClinicalUrl}/fhir/Patient/`, {
      headers: {
        'Origin': DashboardOrigin,
        'Accept': 'application/json',
      }
    });

    // Must succeed AND have CORS header
    expect(response.ok()).toBeTruthy();

    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
  });

  test('Clinical API GET Encounters returns data with CORS headers', async ({ request }) => {
    // First create a patient to get encounters for
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        Active: true,
        GivenName: 'Test',
        FamilyName: 'Patient',
        Gender: 'other'
      }
    });
    expect(createResponse.ok()).toBeTruthy();
    const patientJson = await createResponse.json();
    const patientId = patientJson.Id;

    // Now test the encounters endpoint with CORS
    const response = await request.get(`${ClinicalUrl}/fhir/Patient/${patientId}/Encounter`, {
      headers: {
        'Origin': DashboardOrigin,
        'Accept': 'application/json',
      }
    });

    expect(response.ok()).toBeTruthy();

    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
  });

  test('Scheduling API Appointments endpoint allows CORS from Dashboard', async ({ request }) => {
    // Simulate browser preflight request
    // Note: Scheduling API doesn't use /fhir/ prefix
    const response = await request.fetch(`${SchedulingUrl}/Appointment`, {
      method: 'OPTIONS',
      headers: {
        'Origin': DashboardOrigin,
        'Access-Control-Request-Method': 'GET',
        'Access-Control-Request-Headers': 'Accept',
      }
    });

    // CORS headers must be present
    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
    expect(allowOrigin === DashboardOrigin || allowOrigin === '*').toBeTruthy();
  });

  test('Scheduling API GET Appointments returns data with CORS headers', async ({ request }) => {
    // Scheduling API doesn't use /fhir/ prefix
    const response = await request.get(`${SchedulingUrl}/Appointment`, {
      headers: {
        'Origin': DashboardOrigin,
        'Accept': 'application/json',
      }
    });

    expect(response.ok()).toBeTruthy();

    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
  });

  test('Scheduling API GET Practitioners returns data with CORS headers', async ({ request }) => {
    // Scheduling API doesn't use /fhir/ prefix
    const response = await request.get(`${SchedulingUrl}/Practitioner`, {
      headers: {
        'Origin': DashboardOrigin,
        'Accept': 'application/json',
      }
    });

    expect(response.ok()).toBeTruthy();

    const allowOrigin = response.headers()['access-control-allow-origin'];
    expect(allowOrigin).toBeTruthy();
  });

  test('Clinical API Create Patient works end to end with CORS', async ({ request }) => {
    // Create a patient with unique name
    const uniqueName = `IntTest${Date.now() % 100000}`;
    const createResponse = await request.post(`${ClinicalUrl}/fhir/Patient/`, {
      headers: {
        'Content-Type': 'application/json',
        'Origin': DashboardOrigin,
      },
      data: {
        Active: true,
        GivenName: uniqueName,
        FamilyName: 'IntegrationCreated',
        Gender: 'female'
      }
    });

    // Create patient
    expect(createResponse.ok()).toBeTruthy();

    // Verify - Fetch all patients and confirm the new one is there
    const listResponse = await request.get(`${ClinicalUrl}/fhir/Patient/`, {
      headers: {
        'Origin': DashboardOrigin,
      }
    });
    const listBody = await listResponse.text();

    expect(listBody).toContain(uniqueName);
    expect(listBody).toContain('IntegrationCreated');
  });

  test('Scheduling API Create Practitioner works end to end with CORS', async ({ request }) => {
    // Create a practitioner with unique identifier
    const uniqueId = `DR${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Practitioner`, {
      headers: {
        'Content-Type': 'application/json',
        'Origin': DashboardOrigin,
      },
      data: {
        Identifier: uniqueId,
        Active: true,
        NameGiven: 'IntDoctor',
        NameFamily: 'TestDoc',
        Qualification: 'MD',
        Specialty: 'Testing',
        TelecomEmail: 'inttest@hospital.org',
        TelecomPhone: '+1-555-8888'
      }
    });

    // Create practitioner
    expect(createResponse.ok()).toBeTruthy();

    // Verify - Fetch all practitioners and confirm the new one is there
    const listResponse = await request.get(`${SchedulingUrl}/Practitioner`, {
      headers: {
        'Origin': DashboardOrigin,
      }
    });
    const listBody = await listResponse.text();

    expect(listBody).toContain(uniqueId);
    expect(listBody).toContain('IntDoctor');
  });

  test('Scheduling API Create Appointment works end to end with CORS', async ({ request }) => {
    // Create an appointment with unique service type
    const uniqueService = `Consult${Date.now() % 100000}`;
    const createResponse = await request.post(`${SchedulingUrl}/Appointment`, {
      headers: {
        'Content-Type': 'application/json',
        'Origin': DashboardOrigin,
      },
      data: {
        ServiceCategory: 'General',
        ServiceType: uniqueService,
        Start: '2025-12-25T10:00:00Z',
        End: '2025-12-25T11:00:00Z',
        PatientReference: 'Patient/test',
        PractitionerReference: 'Practitioner/test',
        Priority: 'routine'
      }
    });

    // Create appointment
    expect(createResponse.ok()).toBeTruthy();

    // Verify - Fetch all appointments and confirm the new one is there
    const listResponse = await request.get(`${SchedulingUrl}/Appointment`, {
      headers: {
        'Origin': DashboardOrigin,
      }
    });
    const listBody = await listResponse.text();

    expect(listBody).toContain(uniqueService);
  });
});