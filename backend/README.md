# PeerLearn Connect – Frontend (React + Vite)

Start the backend first (see `../backend/PeerLearn.Api/README.md`), then:

    npm install
    npm run dev      # http://localhost:5173

Sign in or create an account.

- `src/api.js` – fetch wrapper that adds the JWT; all REST calls live here
- `src/context/AuthContext.jsx` – login, register, logout, current user
- `src/hooks/useChat.js` – SignalR connection + chat threads
- `src/hooks/useDashboard.js` – dashboard data
