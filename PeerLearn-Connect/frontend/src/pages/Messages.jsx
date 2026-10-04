import { useEffect, useRef, useState } from 'react';
import Avatar from '../components/Avatar';

export default function Messages({ chat, activeId, onSelect, notify }) {
  const [text, setText] = useState('');
  const box = useRef(null);
  const thread = chat.threads.find((t) => t.userId === activeId) ?? chat.threads[0];

  useEffect(() => { if (thread && !thread.loaded) chat.loadHistory(thread.userId); }, [thread?.userId, thread?.loaded]); // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(() => { if (box.current) box.current.scrollTop = box.current.scrollHeight; }, [thread?.messages.length, thread?.userId]);

  const send = async () => {
    const v = text.trim();
    if (!v) return;
    setText('');
    try { await chat.send(thread.userId, v); } catch (e) { notify(e.message); }
  };

  if (!thread) return (
    <section>
      <div className="row"><h2>Messages</h2></div>
      <div className="empty">No conversations yet. Open a tutor’s card and choose Message to start one.</div>
    </section>
  );

  return (
    <section>
      <div className="row"><h2>Messages</h2><span className="sub"><span className="dot" />Live chat</span></div>
      <div className="card chat">
        <div className="threads">
          {chat.threads.map((t) => (
            <button key={t.userId} className="thread" role="tab" aria-selected={t.userId === thread.userId} onClick={() => onSelect(t.userId)}>
              <Avatar name={t.name || '?'} />
              <div><b>{t.name}</b><div className="sub">{(t.lastText || 'Say hello').slice(0, 26)}</div></div>
            </button>
          ))}
        </div>
        <div className="pane">
          <div className="ph"><span className="dot" />{thread.name}</div>
          <div className="msgs" ref={box} aria-live="polite">
            {thread.messages.map((m, i) => <div key={i} className={`msg${m.mine ? ' me' : ''}`}>{m.text}</div>)}
          </div>
          <div className="compose">
            <input value={text} onChange={(e) => setText(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && send()} placeholder="Write a message" aria-label="Message" />
            <button className="btn" onClick={send}>Send</button>
          </div>
        </div>
      </div>
    </section>
  );
}
