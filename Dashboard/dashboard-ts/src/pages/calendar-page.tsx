import type { ReactElement } from 'react';
import { useAppointments } from '../hooks/use-appointments';

export const CalendarPage = (): ReactElement => {
  const { data, isLoading } = useAppointments();
  if (isLoading) return <div className="page">Loading calendar…</div>;
  const byDay = new Map<string, typeof data>();
  for (const a of data ?? []) {
    const day = a.Start.slice(0, 10);
    const list = byDay.get(day) ?? [];
    list.push(a);
    byDay.set(day, list);
  }
  const days = [...byDay.keys()].sort();
  return (
    <section className="page">
      <h2>Calendar</h2>
      {days.map((day: string) => (
        <div key={day} className="calendar-day-group">
          <h3>{day}</h3>
          <ul>
            {(byDay.get(day) ?? []).map((a) => (
              <li key={a.Id ?? `${a.Start}-${a.PatientReference}`}>
                {new Date(a.Start).toLocaleTimeString()} — {a.ServiceType} ({a.PatientReference})
              </li>
            ))}
          </ul>
        </div>
      ))}
    </section>
  );
};