import { useEffect, useState } from 'react';
import { api } from '../api';
import { WEEK, DAY_NAMES } from '../utils';

export default function Teach({ notify, onSaved }) {
  const [form, setForm] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    api.profile()
      .then((p) => setForm({ isTutor: p.isTutor || p.availability.length === 0, year: p.year, bio: p.bio, courses: p.courses.join(', '), availability: p.availability }))
      .catch((e) => notify(e.message));
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  if (!form) return <section><div className="empty">Loading your tutor profile…</div></section>;

  const set = (k) => (e) => setForm({ ...form, [k]: e.target.type === 'checkbox' ? e.target.checked : e.target.value });
  const rules = (fn) => setForm((f) => ({ ...f, availability: fn(f.availability) }));
  const add = (day) => rules((r) => [...r, { day, start: '09:00', end: '12:00' }]);
  const edit = (i, k, v) => rules((r) => r.map((x, j) => (j === i ? { ...x, [k]: v } : x)));
  const remove = (i) => rules((r) => r.filter((_, j) => j !== i));

  const save = async () => {
    setSaving(true);
    try {
      const p = await api.saveProfile({
        isTutor: form.isTutor, year: form.year, bio: form.bio,
        courses: form.courses.split(',').map((c) => c.trim()).filter(Boolean),
        availability: form.availability,
      });
      notify(p.isTutor ? 'You’re live! Learners can now book your free slots.' : 'Saved. You’re hidden from tutor search.');
      onSaved(p);
    } catch (e) { notify(e.message); }
    setSaving(false);
  };

  return (
    <section>
      <div className="row"><h2>Teach on PeerLearn</h2></div>
      <div className="card teach">
        <label className="switch"><input type="checkbox" checked={form.isTutor} onChange={set('isTutor')} /> I’m available to tutor (show me in tutor search)</label>
        <label>Year and programme<input value={form.year} onChange={set('year')} placeholder="e.g. Computer Science, Year 3" /></label>
        <label>About you<textarea value={form.bio} onChange={set('bio')} placeholder="How do you like to help? What grades did you get?" /></label>
        <label>Courses you can teach (separate with commas)<input value={form.courses} onChange={set('courses')} placeholder="Calculus, Python" /></label>

        <div>
          <h3>Your weekly schedule</h3>
          <p className="sub">Add the times you’re free. Learners can book one-hour slots inside each window (times are in UTC).</p>
          {WEEK.map((d) => (
            <div className="dayrow" key={d}>
              <div className="dname">{DAY_NAMES[d]}</div>
              <div className="wins">
                {form.availability.map((r, i) => r.day === d && (
                  <div className="win" key={i}>
                    <input type="time" step="3600" value={r.start} onChange={(e) => edit(i, 'start', e.target.value)} aria-label={`${DAY_NAMES[d]} start`} />
                    <span>–</span>
                    <input type="time" step="3600" value={r.end} onChange={(e) => edit(i, 'end', e.target.value)} aria-label={`${DAY_NAMES[d]} end`} />
                    <button onClick={() => remove(i)} aria-label="Remove window">×</button>
                  </div>
                ))}
                <button className="chip" onClick={() => add(d)}>+ Add time</button>
              </div>
            </div>
          ))}
        </div>
        <button className="btn" onClick={save} disabled={saving}>{saving ? 'Saving…' : 'Save tutor profile'}</button>
      </div>
    </section>
  );
}
