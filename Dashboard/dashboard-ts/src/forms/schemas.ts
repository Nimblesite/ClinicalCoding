import { z } from 'zod';

export const patientSchema = z.object({
  GivenName: z.string().min(1, 'Given name is required'),
  FamilyName: z.string().min(1, 'Family name is required'),
  Gender: z.enum(['male', 'female', 'other']),
  Active: z.boolean(),
  BirthDate: z.string().optional(),
});
export type PatientFormValues = z.infer<typeof patientSchema>;

export const appointmentSchema = z.object({
  ServiceCategory: z.string().min(1),
  ServiceType: z.string().min(1, 'Service type required'),
  Priority: z.enum(['routine', 'urgent', 'asap', 'stat']),
  PatientReference: z.string().min(1, 'Patient required'),
  PractitionerReference: z.string().min(1, 'Practitioner required'),
  StartTime: z.string().min(1, 'Start required'),
  EndTime: z.string().min(1, 'End required'),
});
export type AppointmentFormValues = z.infer<typeof appointmentSchema>;
