import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { createPatient, updatePatient } from '../api/clinical';
import type { Patient } from '../types/fhir';

interface UpdateArgs {
  readonly id: string | undefined;
  readonly patient: Patient;
}

export const useSavePatient = (): UseMutationResult<Patient, Error, UpdateArgs> => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, patient }: UpdateArgs) =>
      id === undefined ? createPatient(patient) : updatePatient(id, patient),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['patients'] });
    },
  });
};
