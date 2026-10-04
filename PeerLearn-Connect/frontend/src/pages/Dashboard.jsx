import { useState } from 'react';
import Avatar from '../components/Avatar';
import { dayLabel, timeLabel } from '../utils';

function Rating({ onRate }) {
  const [hover, setHover] = useState(0);
  return (
    <div className="rate" onMouseLeave={() => setHover(0)}>
      {[1, 2, 3, 4, 5].map((i) => (
        <button key={i} className={i <= hover ? 'on' : ''} aria-label={`${i} stars`} onMouseEnter={() => setHover(i)} onClick={() => onRate(i)}>★</button>
      ))}
    </div>
  );
}

export default function Dashboard({ data, user, onRespond, onRate }) {
  if (!data) return <section><div className="empty">Loading your dashboard…</div></section>;
  const { upcoming, requests, completed, toRate, notifications } = data;
  const other = (s) => (s.tutorId === user.id ? s.learnerName : s.tutorName);

  return (
    <section>
      <div className="row"><h2>Hi {user.name.split(' ')[0]}, here’s your week</h2></div>
      <div className="stats">
        <div className="stat" style={{ background: 'var(--grad)' }}><b>{upcoming.length}</b>Upcoming sessions</div>
        <div className="stat" style={{ background: 'var(--grad2)' }}><b>{requests.length}</b>Pending requests</div>
        <div className="stat" style={{ background: 'linear-gradient(135deg,#ff8a4c,#e93d9a)' }}><b>{completed}</b>Lessons completed</div>
      </div>

      <div className="cols">
        <div className="card">
          <h3>Upcoming sessions</h3>
          {upcoming.length ? upcoming.map((s) => (
            <div className="item" key={s.id}>
              <div className="when">{new Date(s.startsAt).getDate()}<small>{new Date(s.startsAt).toLocaleDateString(undefined, { month: 'short' })}</small></div>
              <div><b>{s.course} with {other(s)}</b><div className="sub">{dayLabel(s.startsAt)}, {timeLabel(s.startsAt)}{s.status === 'Pending' ? ' · awaiting confirmation' : ''}</div></div>
            </div>
          )) : <div className="sub">Nothing booked yet. Find a tutor to schedule your first session.</div>}
        </div>

        <div className="card">
          <h3>Tutoring requests for you</h3>
          {requests.length ? requests.map((r) => (
            <div className="item" key={r.id}>
              <div><b>{r.learnerName}</b><div className="sub">{r.course}, {dayLabel(r.startsAt)} {timeLabel(r.startsAt)}</div></div>
              <button className="mini ok" onClick={() => onRespond(r, true)}>Accept</button>
              <button className="mini no" onClick={() => onRespond(r, false)}>Decline</button>
            </div>
          )) : <div className="sub">No open requests. They will appear here.</div>}
        </div>

        <div className="card">
          <h3>Rate your last session</h3>
          {toRate ? (
            <div className="item">
              <Avatar name={toRate.tutorName} />
              <div><b>{toRate.course} with {toRate.tutorName}</b><Rating onRate={(n) => onRate(toRate, n)} /></div>
            </div>
          ) : <div className="sub">Nothing to rate right now.</div>}
        </div>

        <div className="card">
          <h3>Notifications</h3>
          {notifications.length ? notifications.map((n) => <div className="item" key={n.id}><span className="dot" /><div>{n.text}</div></div>)
            : <div className="sub">You’re all caught up.</div>}
        </div>
      </div>
    </section>
  );
}
