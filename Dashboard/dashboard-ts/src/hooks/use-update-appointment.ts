import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { createAppointment, updateAppointment } from '../api/scheduling';
import type { Appointment } from '../types/fhir';

interface UpdateArgs {
  readonly id: string | undefined;
  readonly appointment: Appointment;
}

export const useSaveAppointment = (): UseMutationResult<Appointment, Error, UpdateArgs> => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, appointment }: UpdateArgs) =>
      id === undefined ? createAppointment(appointment) : updateAppointment(id, appointment),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['appointments'] });
    },
  });
};
