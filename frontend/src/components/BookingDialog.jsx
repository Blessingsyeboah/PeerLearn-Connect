import { useEffect, useRef, useState } from 'react';
import { api } from '../api';
import { dayLabel, timeLabel } from '../utils';

const dayKey = (iso) => new Date(iso).toDateString();

export default function BookingDialog({ tutor, onClose, onBook, onInvalid }) {
  const ref = useRef(null);
  const [slots, setSlots] = useState([]);
  const [day, setDay] = useState(null);
  const [slot, setSlot] = useState(null);
  const [course, setCourse] = useState('');

  useEffect(() => {
    const d = ref.current;
    if (tutor && !d.open) {
      setDay(null); setSlot(null); setSlots([]); setCourse(tutor.courses[0]);
      d.showModal();
      api.slots(tutor.id).then(setSlots).catch((e) => onInvalid(e.message));
    }
    if (!tutor && d.open) d.close();
  }, [tutor]); // eslint-disable-line react-hooks/exhaustive-deps

  const days = [...new Set(slots.map((s) => dayKey(s.startsAt)))];
  const times = slots.filter((s) => dayKey(s.startsAt) === day);

  const submit = () => {
    if (!slot) return onInvalid('Choose a day and a time first');
    onBook({ tutorId: tutor.id, course, startsAt: slot });
  };

  return (
    <dialog ref={ref} onClose={onClose} aria-labelledby="dt">
      <div className="mh">
        <h2 id="dt">Book {tutor?.name.split(' ')[0]}</h2>
        <div style={{ opacity: 0.9 }}>{tutor?.year}</div>
      </div>
      <div className="mb">
        {tutor?.courses.length > 1 && (
          <label className="sub">Course{' '}
            <select value={course} onChange={(e) => setCourse(e.target.value)}>
              {tutor.courses.map((c) => <option key={c}>{c}</option>)}
            </select>
          </label>
        )}
        <div>
          <div className="sub">Pick a day</div>
          <div className="days">
            {days.map((d) => (
              <button key={d} className="pick" aria-pressed={day === d} onClick={() => { setDay(d); setSlot(null); }}>
                {dayLabel(slots.find((s) => dayKey(s.startsAt) === d).startsAt)}
              </button>
            ))}
          </div>
        </div>
        <div>
          <div className="sub">Pick a time</div>
          <div className="slots">
            {day ? times.map((s) => (
              <button key={s.startsAt} className="pick" aria-pressed={slot === s.startsAt} disabled={!s.available} onClick={() => setSlot(s.startsAt)}>
                {timeLabel(s.startsAt)}
              </button>
            )) : <span className="sub">Choose a day first</span>}
          </div>
        </div>
        <div className="actions">
          <button className="btn ghost" onClick={onClose}>Cancel</button>
          <button className="btn" onClick={submit}>Request session</button>
        </div>
      </div>
    </dialog>
  );
}
