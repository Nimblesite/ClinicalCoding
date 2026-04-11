import { apiPost } from './api-client';
import { CLINICAL_URL, SCHEDULING_URL } from './urls';

async function globalSetup(): Promise<void> {
  await apiPost(`${CLINICAL_URL}/fhir/Patient/`, {
    Active: true,
    GivenName: 'E2ETest',
    FamilyName: 'TestPatient',
    Gender: 'other',
  });
  await apiPost(`${SCHEDULING_URL}/Practitioner`, {
    Identifier: 'DR001',
    Active: true,
    NameGiven: 'E2EPractitioner',
    NameFamily: 'DrTest',
    Qualification: 'MD',
    Specialty: 'General Practice',
    TelecomEmail: 'drtest@hospital.org',
    TelecomPhone: '+1-555-0123',
  });
  await apiPost(`${SCHEDULING_URL}/Practitioner`, {
    Identifier: 'DR002',
    Active: true,
    NameGiven: 'Sarah',
    NameFamily: 'Johnson',
    Qualification: 'DO',
    Specialty: 'Cardiology',
    TelecomEmail: 'sjohnson@hospital.org',
    TelecomPhone: '+1-555-0124',
  });
  await apiPost(`${SCHEDULING_URL}/Practitioner`, {
    Identifier: 'DR003',
    Active: true,
    NameGiven: 'Michael',
    NameFamily: 'Chen',
    Qualification: 'MD',
    Specialty: 'Neurology',
    TelecomEmail: 'mchen@hospital.org',
    TelecomPhone: '+1-555-0125',
  });
  await apiPost(`${SCHEDULING_URL}/Appointment`, {
    ServiceCategory: 'General',
    ServiceType: 'Checkup',
    Start: '2025-12-20T10:00:00Z',
    End: '2025-12-20T11:00:00Z',
    PatientReference: 'Patient/1',
    PractitionerReference: 'Practitioner/1',
    Priority: 'routine',
  });
}

export default globalSetup;
