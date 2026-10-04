import { useState, useRef, useCallback } from 'react';

export default function useToast() {
  const [toast, setToast] = useState({ text: '', show: false });
  const timer = useRef();
  const notify = useCallback((text) => {
    setToast({ text, show: true });
    clearTimeout(timer.current);
    timer.current = setTimeout(() => setToast((t) => ({ ...t, show: false })), 2400);
  }, []);
  return [toast, notify];
}
