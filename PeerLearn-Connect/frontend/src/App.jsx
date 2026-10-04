import { useEffect, useState } from 'react';
import Sidebar from './components/Sidebar';
import Toast from './components/Toast';
import BookingDialog from './components/BookingDialog';
import Discover from './pages/Discover';
import Teach from './pages/Teach';
import Messages from './pages/Messages';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import { useAuth } from './context/AuthContext';
import { api } from './api';
import useChat from './hooks/useChat';
import useDashboard from './hooks/useDashboard';
import useToast from './hooks/useToast';
import useTheme from './hooks/useTheme';

function Shell({ user, landing, onLogout, onProfile }) {
  const [view, setView] = useState(landing);
  const [booking, setBooking] = useState(null);
  const [activeChat, setActiveChat] = useState(null);
  const chat = useChat(user);
  const dash = useDashboard();
  const [toast, notify] = useToast();
  const toggleTheme = useTheme();

  const go = (v) => {
    setView(v);
    chat.setViewing(v === 'messages');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };
  useEffect(() => { if (view === 'dash') dash.reload(); }, [view]); // eslint-disable-line react-hooks/exhaustive-deps

  const attempt = async (fn, okMessage) => {
    try { await fn(); if (okMessage) notify(okMessage); return true; }
    catch (e) { notify(e.message); return false; }
  };

  const book = async (payload) => {
    const name = booking.name.split(' ')[0];
    if (await attempt(() => api.book(payload), `Session requested. ${name} will confirm soon.`)) setBooking(null);
  };
  const respond = async (s, accept) => {
    await attempt(() => api.respond(s.id, accept), accept ? `Accepted ${s.learnerName}’s request` : 'Request declined');
    dash.reload();
  };
  const rate = async (s, n) => {
    await attempt(() => api.rate(s.id, n), 'Rating saved');
    dash.reload();
  };
  const message = (tutor) => { chat.startChat(tutor.id, tutor.name); setActiveChat(tutor.id); go('messages'); };

  return (
    <div className="app">
      <Sidebar view={view} onChange={go} unread={chat.unread} onToggleTheme={toggleTheme} onLogout={onLogout} />
      <main>
        {view === 'discover' && <Discover onBook={setBooking} onMessage={message} />}
        {view === 'teach' && <Teach notify={notify} onSaved={onProfile} />}
        {view === 'messages' && <Messages chat={chat} activeId={activeChat} onSelect={setActiveChat} notify={notify} />}
        {view === 'dash' && <Dashboard data={dash.data} user={user} onRespond={respond} onRate={rate} />}
      </main>
      <BookingDialog tutor={booking} onClose={() => setBooking(null)} onBook={book} onInvalid={notify} />
      <Toast {...toast} />
    </div>
  );
}

export default function App() {
  const { user, ready, logout, landing, setUser } = useAuth();
  if (!ready) return null;
  return user
    ? <Shell key={user.id} user={user} landing={landing} onLogout={logout} onProfile={(p) => setUser((u) => ({ ...u, isTutor: p.isTutor }))} />
    : <Login />;
}