export interface Patient {
  readonly Id?: string;
  readonly GivenName: string;
  readonly FamilyName: string;
  readonly Gender: 'male' | 'female' | 'other';
  readonly Active: boolean;
  readonly BirthDate?: string;
}

export interface Practitioner {
  readonly Id?: string;
  readonly Identifier: string;
  readonly NameGiven: string;
  readonly NameFamily: string;
  readonly Qualification: string;
  readonly Specialty?: string;
  readonly TelecomEmail?: string;
  readonly TelecomPhone?: string;
}

export interface Appointment {
  readonly Id?: string;
  readonly ServiceCategory: string;
  readonly ServiceType: string;
  readonly Priority: 'routine' | 'urgent' | 'asap' | 'stat';
  readonly PatientReference: string;
  readonly PractitionerReference: string;
  readonly Start: string;
  readonly End: string;
  readonly Status?: string;
}

export interface Encounter {
  readonly Id?: string;
  readonly PatientReference: string;
  readonly Status: string;
}

export interface Condition {
  readonly Id?: string;
  readonly PatientReference: string;
  readonly Code: string;
  readonly DisplayText: string;
}
