import { useState } from 'react';
import { useAuth } from '../context/AuthContext';

const ROLES = [['learn', '🔎 Find a tutor'], ['teach', '🎓 Be a tutor'], ['both', '✨ Both']];

export default function Login() {
  const { login, register } = useAuth();
  const [mode, setMode] = useState('login');
  const [role, setRole] = useState('learn');
  const [form, setForm] = useState({ name: '', email: '', password: '' });
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const set = (k) => (e) => setForm({ ...form, [k]: e.target.value });

  const submit = async (e) => {
    e.preventDefault();
    setBusy(true); setError('');
    try {
      if (mode === 'login') await login(form.email, form.password);
      else await register(form, role === 'learn' ? 'discover' : 'teach'); // tutors set their schedule first
    } catch (err) { setError(err.message); }
    setBusy(false);
  };

  return (
    <div className="login">
      <aside className="login-art">
        <div className="logo"><i>✦</i>PeerLearn</div>
        <h1>Stuck on a course? Someone in your year has been there.</h1>
        <p>Book a peer tutor, chat first, and meet when it suits you both. Or share what you know on your own schedule.</p>
      </aside>
      <form className="login-form card" onSubmit={submit}>
        <h2>{mode === 'login' ? 'Welcome back' : 'Create your account'}</h2>
        {mode === 'register' && <label>Full name<input value={form.name} onChange={set('name')} required autoComplete="name" /></label>}
        <label>Email<input type="email" value={form.email} onChange={set('email')} required autoComplete="email" /></label>
        <label>Password<input type="password" value={form.password} onChange={set('password')} required minLength={8} autoComplete={mode === 'login' ? 'current-password' : 'new-password'} /></label>
        {mode === 'register' && (
          <div>
            <div className="sub">What would you like to do?</div>
            <div className="roles" role="group" aria-label="Your goal">
              {ROLES.map(([id, label]) => <button type="button" key={id} className="role" aria-pressed={role === id} onClick={() => setRole(id)}>{label}</button>)}
            </div>
            <div className="sub" style={{ marginTop: 6 }}>You can switch or do both any time from the Teach tab.</div>
          </div>
        )}
        {error && <div className="error" role="alert">{error}</div>}
        <button className="btn" disabled={busy}>{busy ? 'Please wait…' : mode === 'login' ? 'Sign in' : 'Create account'}</button>
        <button type="button" className="link" onClick={() => { setMode(mode === 'login' ? 'register' : 'login'); setError(''); }}>
          {mode === 'login' ? 'New here? Create an account' : 'Have an account? Sign in'}
        </button>
      </form>
    </div>
  );
}
