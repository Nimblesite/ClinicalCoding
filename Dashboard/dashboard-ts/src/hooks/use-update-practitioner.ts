import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { apiFetch } from '../api/client';
import { SCHEDULING_API } from '../api/config';
import { createPractitioner } from '../api/scheduling';
import type { Practitioner } from '../types/fhir';

interface UpdateArgs {
  readonly id: string | undefined;
  readonly practitioner: Practitioner;
}

const updatePractitioner = async (id: string, p: Practitioner): Promise<Practitioner> =>
  apiFetch<Practitioner>(`${SCHEDULING_API}/Practitioner/${id}`, {
    method: 'PUT',
    body: p,
  });

export const useSavePractitioner = (): UseMutationResult<Practitioner, Error, UpdateArgs> => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, practitioner }: UpdateArgs) =>
      id === undefined ? createPractitioner(practitioner) : updatePractitioner(id, practitioner),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['practitioners'] });
    },
  });
};
