export const GRADIENTS = ['#6d3df5,#e93d9a', '#17c3a2,#6d3df5', '#ff8a4c,#e93d9a', '#6d3df5,#17c3a2', '#e93d9a,#ff8a4c', '#17c3a2,#ff8a4c'];

export const gradientFor = (name = '') =>
  GRADIENTS[[...name].reduce((a, c) => a + c.charCodeAt(0), 0) % GRADIENTS.length];

const sameDay = (a, b) => a.toDateString() === b.toDateString();

export function dayLabel(iso) {
  const d = new Date(iso);
  if (sameDay(d, new Date())) return 'Today';
  if (sameDay(d, new Date(Date.now() + 864e5))) return 'Tomorrow';
  return d.toLocaleDateString(undefined, { weekday: 'short', day: 'numeric', month: 'short' });
}

export const timeLabel = (iso) => new Date(iso).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });

// Weekly schedule helpers. Day numbers match the backend: 0 = Sunday ... 6 = Saturday.
export const WEEK = [1, 2, 3, 4, 5, 6, 0];
export const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export function scheduleSummary(rules = []) {
  return WEEK.map((d) => {
    const r = rules.filter((x) => x.day === d).sort((a, b) => a.start.localeCompare(b.start));
    return r.length ? `${DAY_NAMES[d]} ${r.map((x) => `${x.start}–${x.end}`).join(', ')}` : null;
  }).filter(Boolean).join(' · ');
}
