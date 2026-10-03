import { useCallback, useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { api, tokenStore } from '../api';

/** Chat threads + live delivery through the SignalR hub at /hubs/chat. */
export default function useChat(user) {
  const [threads, setThreads] = useState([]);
  const [unread, setUnread] = useState(0);
  const viewing = useRef(false);
  const conn = useRef(null);

  const update = useCallback((id, name, fn) => setThreads((ts) => {
    if (!ts.some((t) => t.userId === id)) return [fn({ userId: id, name, messages: [], loaded: false, lastText: '' }), ...ts];
    return ts.map((t) => (t.userId === id ? fn(t) : t));
  }), []);

  useEffect(() => {
    api.threads().then((list) => setThreads(list.map((t) => ({ ...t, messages: [], loaded: false }))));

    const c = new HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => tokenStore.get() })
      .withAutomaticReconnect().build();
    c.on('ReceiveMessage', (m) => {
      const mine = m.fromId === user.id;
      update(mine ? m.toId : m.fromId, mine ? m.toName : m.fromName, (t) => ({
        ...t, lastText: m.text,
        messages: t.loaded ? [...t.messages, { mine, text: m.text }] : t.messages,
      }));
      if (!mine && !viewing.current) setUnread((n) => n + 1);
    });
    c.start().catch(console.error);
    conn.current = c;
    return () => { c.stop(); };
  }, [user.id, update]);

  return {
    threads, unread,
    setViewing: (v) => { viewing.current = v; if (v) setUnread(0); },
    send: (toId, text) => conn.current?.invoke('SendMessage', toId, text),
    startChat: (id, name) => setThreads((ts) => (ts.some((t) => t.userId === id) ? ts : [{ userId: id, name, messages: [], loaded: true, lastText: '' }, ...ts])),
    loadHistory: (id) => api.history(id).then((list) =>
      update(id, '', (t) => ({ ...t, loaded: true, messages: list.map((m) => ({ mine: m.fromId === user.id, text: m.text })) }))),
  };
}
