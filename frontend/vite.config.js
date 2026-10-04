import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Forwards API + SignalR calls to the ASP.NET Core backend (http://localhost:5000).
export default defineConfig({
  plugins: [react()],
  server: { proxy: { '/api': 'http://localhost:5000', '/hubs': { target: 'http://localhost:5000', ws: true } } },
});
