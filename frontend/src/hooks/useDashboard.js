import { useCallback, useEffect, useState } from 'react';
import { api } from '../api';

export default function useDashboard() {
  const [data, setData] = useState(null);
  const reload = useCallback(() => api.dashboard().then(setData).catch(() => {}), []);
  useEffect(() => { reload(); }, [reload]);
  return { data, reload };
}
