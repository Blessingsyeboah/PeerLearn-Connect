import Avatar from './Avatar';
import { dayLabel, scheduleSummary } from '../utils';

export default function TutorCard({ tutor, onBook, onMessage }) {
  return (
    <article className="card tutor">
      <div className="top">
        <Avatar name={tutor.name} />
        <div><h3>{tutor.name}</h3><div className="sub">{tutor.year}</div></div>
      </div>
      <div className="tags">{tutor.courses.map((c) => <span key={c} className="tag">{c}</span>)}</div>
      <div className="sub schedule">🗓 {scheduleSummary(tutor.schedule)}</div>
      <div className="meta">
        <span><span className="stars">★</span> {tutor.rating || 'New'} · {tutor.sessions} sessions</span>
        <span><span className="dot" />Next: {dayLabel(tutor.nextSlot)}</span>
      </div>
      <div className="row2">
        <button className="btn" onClick={() => onBook(tutor)}>Book a session</button>
        <button className="btn ghost" onClick={() => onMessage(tutor)}>Message</button>
      </div>
    </article>
  );
}
