import { createContext, useContext, useEffect, useState } from 'react';
import { api, tokenStore } from '../api';

const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [landing, setLanding] = useState('discover'); // first page after sign-up
  const [ready, setReady] = useState(!tokenStore.get());

  useEffect(() => {
    if (tokenStore.get()) api.me().then(setUser).catch(() => tokenStore.clear()).finally(() => setReady(true));
    const out = () => setUser(null);
    window.addEventListener('pl-logout', out);
    return () => window.removeEventListener('pl-logout', out);
  }, []);

  const finish = async (promise, page = 'discover') => {
    const { token, user } = await promise;
    tokenStore.set(token);
    setLanding(page);
    setUser(user);
  };

  const value = {
    user, ready, landing, setUser,
    login: (email, password) => finish(api.login(email, password)),
    register: (body, page) => finish(api.register(body), page),
    logout: () => { tokenStore.clear(); setUser(null); },
  };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
