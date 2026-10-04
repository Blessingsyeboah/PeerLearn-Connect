const TABS = [
  { id: 'discover', icon: '🔎', label: 'Find a tutor' },
  { id: 'teach', icon: '🎓', label: 'Teach' },
  { id: 'messages', icon: '💬', label: 'Messages' },
  { id: 'dash', icon: '📅', label: 'My dashboard' },
];

export default function Sidebar({ view, onChange, unread, onToggleTheme, onLogout }) {
  return (
    <nav role="tablist" aria-label="Main">
      <div className="logo"><i>✦</i>PeerLearn</div>
      {TABS.map((t) => (
        <button key={t.id} className="tab" role="tab" aria-selected={view === t.id} onClick={() => onChange(t.id)}>
          {t.icon} <span>{t.label}</span>
          {t.id === 'messages' && unread > 0 && <b>{unread}</b>}
        </button>
      ))}
      <button className="theme" onClick={onToggleTheme}>🌓 <span>Switch theme</span></button>
      <button className="theme" onClick={onLogout}>🚪 <span>Sign out</span></button>
    </nav>
  );
}
