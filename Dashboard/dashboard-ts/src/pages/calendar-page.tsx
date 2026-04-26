import { useMemo, useState, type ReactElement } from 'react';
import { useAppointments } from '../hooks/use-appointments';
import { navigate } from '../router/router-hooks';

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'] as const;

const isSameDay = (a: Date, b: Date): boolean =>
  a.getFullYear() === b.getFullYear() &&
  a.getMonth() === b.getMonth() &&
  a.getDate() === b.getDate();

interface CalendarCell {
  readonly date: Date;
  readonly inMonth: boolean;
}

const buildCells = (year: number, month: number): readonly CalendarCell[] => {
  const first = new Date(year, month, 1);
  const last = new Date(year, month + 1, 0);
  const startDay = first.getDay();
  const totalDays = last.getDate();
  const cells: CalendarCell[] = [];
  for (let i = startDay - 1; i >= 0; i -= 1) {
    cells.push({ date: new Date(year, month, -i), inMonth: false });
  }
  for (let d = 1; d <= totalDays; d += 1) {
    cells.push({ date: new Date(year, month, d), inMonth: true });
  }
  let extra = 1;
  while (cells.length < 42) {
    cells.push({
      date: new Date(year, month, totalDays + extra),
      inMonth: false,
    });
    extra += 1;
  }
  return cells;
};

const monthLabel = (year: number, month: number): string =>
  new Date(year, month, 1).toLocaleString('default', {
    month: 'long',
    year: 'numeric',
  });

export const CalendarPage = (): ReactElement => {
  const { data } = useAppointments();
  const today = new Date();
  const [cursor, setCursor] = useState<{ year: number; month: number }>({
    year: today.getFullYear(),
    month: today.getMonth(),
  });
  const [selectedDay, setSelectedDay] = useState<Date | null>(today);

  const cells = useMemo(() => buildCells(cursor.year, cursor.month), [cursor]);
  const appointments = data ?? [];

  const appointmentsOnDay = (day: Date): typeof appointments =>
    appointments.filter((a) => isSameDay(new Date(a.StartTime), day));

  const selectedAppointments = selectedDay !== null ? appointmentsOnDay(selectedDay) : [];

  return (
    <section className="page calendar-page">
      <div className="page-header">
        <div>
          <h2>Schedule</h2>
          <p className="page-description">View and manage appointments by day.</p>
        </div>
        <div className="flex items-center gap-4">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => {
              setCursor(({ year, month }) =>
                month === 0 ? { year: year - 1, month: 11 } : { year, month: month - 1 },
              );
            }}
          >
            <span className="material-symbols-outlined">chevron_left</span>
          </button>
          <span className="text-lg font-semibold">{monthLabel(cursor.year, cursor.month)}</span>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => {
              setCursor(({ year, month }) =>
                month === 11 ? { year: year + 1, month: 0 } : { year, month: month + 1 },
              );
            }}
          >
            <span className="material-symbols-outlined">chevron_right</span>
          </button>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => {
              const now = new Date();
              setCursor({ year: now.getFullYear(), month: now.getMonth() });
              setSelectedDay(now);
            }}
          >
            Today
          </button>
        </div>
      </div>
      <div className="calendar-grid-container">
        <div className="calendar-grid">
          {DAY_NAMES.map((name) => (
            <div key={name} className="calendar-day-header">
              {name}
            </div>
          ))}
          {cells.map((cell) => {
            const isToday = isSameDay(cell.date, today);
            const isSelected = selectedDay !== null && isSameDay(cell.date, selectedDay);
            const dayAppts = appointmentsOnDay(cell.date);
            const classes = [
              'calendar-cell',
              cell.inMonth ? 'in-month' : 'out-month',
              isToday ? 'today' : '',
              dayAppts.length > 0 ? 'has-appointments' : '',
              isSelected ? 'selected' : '',
            ]
              .filter((c) => c !== '')
              .join(' ');
            return (
              <button
                key={cell.date.toISOString()}
                className={classes}
                type="button"
                onClick={() => {
                  setSelectedDay(cell.date);
                }}
              >
                <span className="calendar-cell-date">{cell.date.getDate()}</span>
                {dayAppts.length > 0 && (
                  <span className="calendar-cell-count">{dayAppts.length}</span>
                )}
              </button>
            );
          })}
        </div>
      </div>
      {selectedDay !== null && (
        <div className="calendar-details-panel">
          <h4>{selectedDay.toLocaleDateString()}</h4>
          {selectedAppointments.length === 0 ? (
            <p>No appointments scheduled.</p>
          ) : (
            <ul>
              {selectedAppointments.map((a) => (
                <li
                  key={a.Id ?? `${a.StartTime}-${a.PatientReference}`}
                  className="calendar-appointment-item"
                >
                  <div>
                    <strong>{a.ServiceType}</strong>
                    <p>
                      {new Date(a.StartTime).toLocaleTimeString()} — {a.PatientReference}
                    </p>
                  </div>
                  {a.Id !== undefined && (
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => {
                        if (a.Id !== undefined) navigate(`appointments/edit/${a.Id}`);
                      }}
                    >
                      Edit
                    </button>
                  )}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  );
};
