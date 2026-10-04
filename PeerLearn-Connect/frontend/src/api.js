const KEY = 'pl_token';
export const tokenStore = {
  get: () => localStorage.getItem(KEY),
  set: (t) => localStorage.setItem(KEY, t),
  clear: () => localStorage.removeItem(KEY),
};

async function request(path, { method = 'GET', body } = {}) {
  const token = tokenStore.get();
  const res = await fetch(`/api${path}`, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token && { Authorization: `Bearer ${token}` }) },
    body: body && JSON.stringify(body),
  });
  if (res.status === 401 && token) {
    tokenStore.clear();
    window.dispatchEvent(new Event('pl-logout')); // expired token → back to login
  }
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || err.title || 'Something went wrong. Please try again.');
  }
  return res.status === 204 ? null : res.json();
}

const qs = (o) => new URLSearchParams(Object.entries(o).filter(([, v]) => v)).toString();

export const api = {
  login: (email, password) => request('/auth/login', { method: 'POST', body: { email, password } }),
  register: (body) => request('/auth/register', { method: 'POST', body }),
  me: () => request('/auth/me'),
  tutors: (params) => request(`/tutors?${qs(params)}`),
  courses: () => request('/tutors/courses'),
  slots: (tutorId) => request(`/tutors/${tutorId}/slots`),
  book: (body) => request('/sessions', { method: 'POST', body }),
  respond: (id, accept) => request(`/sessions/${id}/respond`, { method: 'POST', body: { accept } }),
  rate: (sessionId, rating) => request('/reviews', { method: 'POST', body: { sessionId, rating } }),
  dashboard: () => request('/dashboard'),
  profile: () => request('/profile'),
  saveProfile: (body) => request('/profile', { method: 'PUT', body }),
  threads: () => request('/messages/threads'),
  history: (userId) => request(`/messages/with/${userId}`),
};
