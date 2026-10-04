import { useEffect, useState } from 'react';
import { api } from '../api';
import TutorCard from '../components/TutorCard';

export default function Discover({ onBook, onMessage }) {
  const [query, setQuery] = useState('');
  const [course, setCourse] = useState(null);
  const [sort, setSort] = useState('rating');
  const [courses, setCourses] = useState([]);
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { api.courses().then(setCourses).catch(() => {}); }, []);

  useEffect(() => {
    setLoading(true);
    const t = setTimeout(() => {
      api.tutors({ q: query, course, sort }).then(setResults).catch(() => setResults([])).finally(() => setLoading(false));
    }, 250);
    return () => clearTimeout(t);
  }, [query, course, sort]);

  return (
    <section>
      <div className="hero">
        <h1>Learn from the student who just aced it.</h1>
        <p>Find a peer tutor for your hardest course, book a time that suits you both, and chat before you meet.</p>
        <div className="search">
          <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search a course or topic, e.g. Calculus" aria-label="Search tutors" />
        </div>
      </div>

      <div className="chips" role="group" aria-label="Filter by course">
        {[null, ...courses].map((c) => (
          <button key={c ?? 'all'} className="chip" aria-pressed={course === c} onClick={() => setCourse(c)}>{c ?? 'All courses'}</button>
        ))}
      </div>

      <div className="row">
        <h2>{loading ? 'Finding tutors…' : `${results.length} ${results.length === 1 ? 'tutor' : 'tutors'}${course ? ` for ${course}` : ''}`}</h2>
        <label className="sub">Sort by{' '}
          <select value={sort} onChange={(e) => setSort(e.target.value)}>
            <option value="rating">Top rated</option>
            <option value="soon">Available soonest</option>
            <option value="sessions">Most sessions</option>
          </select>
        </label>
      </div>

      <div className="grid">
        {results.map((t) => <TutorCard key={t.id} tutor={t} onBook={onBook} onMessage={onMessage} />)}
        {!loading && !results.length && <div className="empty">{query || course ? 'No tutors match your search. Try another course or clear the filters.' : 'No tutors are available yet. Check back soon, or open the Teach tab to become one yourself.'}</div>}
      </div>
    </section>
  );
}
