<div align="center">

# ✦ PeerLearn Connect

**A peer-to-peer academic tutoring platform where students learn from each other, and teach each other.**

![React](https://img.shields.io/badge/Frontend-React%20%2B%20Vite-6d3df5?style=flat-square)
![ASP.NET Core](https://img.shields.io/badge/Backend-ASP.NET%20Core-e93d9a?style=flat-square)
![MongoDB](https://img.shields.io/badge/Database-MongoDB%20Atlas-17c3a2?style=flat-square)
![SignalR](https://img.shields.io/badge/Realtime-SignalR-ff8a4c?style=flat-square)
![JWT](https://img.shields.io/badge/Auth-JWT-1d1633?style=flat-square)

*DCIT 318 · Programming II*

</div>

---

## Table of contents

1. [Overview](#1-overview)
2. [Features](#2-features)
3. [Tech stack](#3-tech-stack)
4. [Architecture](#4-architecture)
5. [Project structure](#5-project-structure)
6. [Getting started](#6-getting-started)
7. [Configuration reference](#7-configuration-reference)
8. [How it works](#8-how-it-works)
9. [API reference](#9-api-reference)
10. [Data model](#10-data-model)
11. [Frontend guide](#11-frontend-guide)
12. [Security](#12-security)
13. [Troubleshooting](#13-troubleshooting)
14. [Team workflow](#14-team-workflow)
15. [Known limitations and roadmap](#15-known-limitations-and-roadmap)

---

## 1. Overview

Students often struggle with a course but don't know who to ask, and traditional tutoring can be expensive or hard to reach. **PeerLearn Connect** lets students find a peer who has already mastered a course, see exactly when that peer is free, book a session, chat first, and rate the experience afterwards.

Every account can do both things. A student can **find a tutor**, **be a tutor**, or **do both** with the same login.

| Audience | What they get |
|----------|---------------|
| **Learners** | Search tutors by course, see real schedules, book a free slot, chat in real time, leave ratings |
| **Tutors** | Set their own weekly schedule, accept or decline requests, build a rating over time |
| **Departments and learning communities** | An affordable, collaborative alternative to paid tutoring |

---

## 2. Features

- **Tutor discovery:** search by name or course, filter by course, and sort by rating, soonest availability or number of sessions. Only tutors who have switched tutoring on, listed courses, set a schedule and still have a free slot appear.
- **Tutor-defined schedules:** each tutor sets weekly time windows (for example Monday 09:00 to 12:00). Learners book one-hour slots inside those windows.
- **Session booking:** pick a day and time from the tutor's live availability. Double-bookings are blocked by the server.
- **Request workflow:** tutors accept or decline requests, and learners are notified.
- **Real-time messaging:** a SignalR chat that saves history to the database and delivers messages instantly to every open tab.
- **Ratings and feedback:** learners rate a completed session from 1 to 5, and the tutor's average updates.
- **Personal dashboard:** upcoming sessions, tutoring requests, completed lessons, a session waiting for a rating, and notifications.
- **JWT authentication:** registration, login, protected API and protected real-time hub.
- **Responsive design with dark mode:** gradient interface that adapts to phones and follows the system theme, with a manual toggle.

---

## 3. Tech stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, Vite 5, plain CSS (design tokens and gradients) |
| Backend | ASP.NET Core Web API (.NET 8 or later) |
| Real-time | ASP.NET Core SignalR (`@microsoft/signalr` on the client) |
| Database | MongoDB (Atlas or local) with the official `MongoDB.Driver` |
| Authentication | JWT bearer tokens, passwords hashed with BCrypt |
| Tooling | npm, dotnet CLI, Git |

> **Note on the original proposal.** The project brief listed Blazor WebAssembly, Entity Framework Core, SQL Server and ASP.NET Identity. The implementation uses **React** for the interface, **MongoDB** for storage and a lightweight **JWT** setup for authentication, while keeping the same ASP.NET Core Web API and SignalR foundations.

---

## 4. Architecture

```mermaid
flowchart LR
    subgraph Browser
        UI["React + Vite SPA"]
    end
    subgraph Server["ASP.NET Core API"]
        C["REST controllers"]
        H["SignalR ChatHub"]
        A["JWT authentication"]
    end
    DB[("MongoDB Atlas")]

    UI -- "HTTP /api + Bearer token" --> C
    UI -- "WebSocket /hubs/chat" --> H
    C --> DB
    H --> DB
    A -.-> C
    A -.-> H
```

In development, Vite runs on **port 5173** and proxies `/api` and `/hubs` to the API on **port 5000**, so the browser sees a single origin and CORS is not a problem.

---

## 5. Project structure

```
peerlearn-connect/
├── backend/
│   └── PeerLearn.Api/
│       ├── Program.cs                 # app setup: auth, CORS, SignalR, DI
│       ├── appsettings.json           # Mongo, JWT and CORS settings
│       ├── Models/Entities.cs         # User, TutoringSession, Message, Review, Notification
│       ├── Dtos/Dtos.cs               # request and response shapes
│       ├── Data/MongoContext.cs       # collections and indexes
│       ├── Services/TokenService.cs   # creates JWTs
│       ├── Common/Extensions.cs       # claim helpers, slot generator
│       ├── Controllers/
│       │   ├── AuthController.cs      # register, login, me
│       │   ├── ProfileController.cs   # become a tutor, edit schedule
│       │   ├── TutorsController.cs    # search, courses, slots
│       │   ├── SessionsController.cs  # book, respond, complete
│       │   ├── ReviewsController.cs   # rate a session
│       │   ├── MessagesController.cs  # chat history
│       │   └── DashboardController.cs # dashboard summary
│       └── Hubs/ChatHub.cs            # real-time chat
└── frontend/
    ├── index.html
    ├── vite.config.js                 # dev server and proxy
    └── src/
        ├── main.jsx · App.jsx         # entry point and app shell
        ├── api.js · utils.js          # fetch wrapper with JWT, helpers
        ├── context/AuthContext.jsx    # login state
        ├── hooks/                     # useChat, useDashboard, useToast, useTheme
        ├── components/                # Sidebar, TutorCard, BookingDialog, Avatar, Toast
        ├── pages/                     # Discover, Teach, Messages, Dashboard, Login
        └── styles/                    # base.css, components.css, pages.css, teach.css
```

---

## 6. Getting started

### Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| .NET SDK | 8.0 or later | `dotnet --version` |
| Node.js | 18 or later | `node --version` |
| MongoDB | Atlas (free) or local Community Server | |
| Git | any recent | `git --version` |

### Step 1: set up MongoDB

**Option A: MongoDB Atlas (recommended)**

1. Create a free **M0** cluster at [mongodb.com/atlas](https://www.mongodb.com/atlas).
2. **Database Access:** add a user with a letters-and-numbers password and the role *Read and write to any database*.
3. **Network Access:** add your IP address, or `0.0.0.0/0` while testing.
4. **Connect → Drivers → C#/.NET:** copy the connection string. If your network blocks the `mongodb+srv://` DNS lookup, switch on **Legacy URI String** and copy the `mongodb://` version instead.

**Option B: local MongoDB**

Install MongoDB Community Server (for example `winget install MongoDB.Server`) and use `mongodb://localhost:27017`.

### Step 2: configure and run the backend

Edit `backend/PeerLearn.Api/appsettings.json`:

```json
"Mongo": {
  "ConnectionString": "mongodb://USER:PASSWORD@your-cluster-hosts...",
  "Database": "peerlearn"
}
```

To keep the password out of Git, supply it as an environment variable instead:

```powershell
$env:Mongo__ConnectionString = "mongodb://USER:PASSWORD@..."
```

Then start the API from the project folder:

```powershell
cd backend/PeerLearn.Api
dotnet run
```

The API is ready when you see `Now listening on: http://localhost:5000`. Collections and indexes are created automatically on first run, and the database starts **empty** with no pre-created tutors.

### Step 3: run the frontend

In a second terminal:

```powershell
cd frontend
npm install
npm run dev
```

Open **http://localhost:5173**.

### Step 4: try the full flow

1. **Account A:** sign up and choose **Be a tutor**. On the **Teach** tab, switch on "I'm available to tutor", add courses and at least one weekly time window, then save.
2. **Account B** (a private window): sign up as **Find a tutor**. Account A now appears with their schedule.
3. Book a slot, then accept it from Account A's **dashboard**.
4. Use **Message** on the tutor card to test live chat between both windows.

---

## 7. Configuration reference

| Key | Where | Purpose | Default |
|-----|-------|---------|---------|
| `Urls` | `appsettings.json` | Address the API listens on | `http://localhost:5000` |
| `Mongo:ConnectionString` | `appsettings.json` or env `Mongo__ConnectionString` | MongoDB connection | none, required |
| `Mongo:Database` | `appsettings.json` | Database name | `peerlearn` |
| `Jwt:Key` | `appsettings.json` or env `Jwt__Key` | Secret that signs tokens (32 or more characters) | development key, **change it** |
| `Jwt:Issuer` / `Jwt:Audience` | `appsettings.json` | Token validation values | `PeerLearn` / `PeerLearn.Web` |
| `Jwt:ExpiryMinutes` | `appsettings.json` | Token lifetime | `120` |
| `Cors:Origins` | `appsettings.json` | Allowed browser origins | `http://localhost:5173` |

Environment variables use a double underscore for nesting, so `Jwt:Key` becomes `Jwt__Key`.

---

## 8. How it works

### 8.1 One account, two roles

There is no separate "tutor account". Any user can open the **Teach** tab, switch tutoring on, list courses and set a schedule. From then on they appear in search for everyone else, while still being able to book other tutors. A tutor never sees their own card in search, and switching tutoring off hides them without cancelling existing bookings.

### 8.2 Schedules and slots

A tutor stores weekly **availability rules**:

```json
{ "day": 1, "start": "09:00", "end": "12:00" }
```

- `day` runs from `0` (Sunday) to `6` (Saturday).
- Times are in **UTC** (the same as Ghana time).
- Each window must be at least one hour long.

The server turns these rules into **one-hour slots for the next 14 days**. A slot is bookable only if it is in the future and no pending or confirmed session already uses it. Bookings are validated against the tutor's *current* schedule on the server, not just in the interface.

### 8.3 Session lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending: learner books a slot
    Pending --> Confirmed: tutor accepts
    Pending --> Declined: tutor declines
    Confirmed --> Completed: tutor marks complete
    Completed --> [*]: learner rates (1 to 5)
```

Each step creates a notification for the other person. When a learner rates a completed session, the tutor's average is recalculated as `(average × count + rating) / (count + 1)`.

### 8.4 Real-time chat

1. The browser opens a SignalR connection to `/hubs/chat`, sending the JWT as `access_token` (WebSockets cannot send headers).
2. The client calls the hub method `SendMessage(toUserId, text)`.
3. The server validates the message (1 to 2000 characters, not to yourself), **saves it to MongoDB**, then pushes `ReceiveMessage` to both the sender and the recipient, which covers every open tab.
4. Chat history loads over REST when a conversation is opened.

### 8.5 Authentication flow

```mermaid
sequenceDiagram
    participant B as Browser
    participant A as API
    participant D as MongoDB
    B->>A: POST /api/auth/login (email, password)
    A->>D: find user, verify BCrypt hash
    A-->>B: JWT + user
    B->>B: store token
    B->>A: any request with "Authorization: Bearer token"
    A->>A: validate signature, issuer, expiry
    A-->>B: data (or 401, then the app returns to the login page)
```

---

## 9. API reference

All endpoints except register and login require the header `Authorization: Bearer <token>`. Errors return `{ "message": "..." }`.

### Auth

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | `{ name, email, password }` | Create an account and return `{ token, user }` (password 8 or more characters) |
| POST | `/api/auth/login` | `{ email, password }` | Sign in and return `{ token, user }` |
| GET | `/api/auth/me` | none | Current user |

### Profile (tutor setup)

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| GET | `/api/profile` | none | Current user's tutor profile |
| PUT | `/api/profile` | `{ isTutor, year, bio, courses[], availability[{day,start,end}] }` | Save the profile. If `isTutor` is true, at least one course and one time window are required |

### Tutors

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tutors?q=&course=&sort=` | Bookable tutors. `sort` is `rating` (default), `soon` or `sessions` |
| GET | `/api/tutors/courses` | Courses taught by bookable tutors |
| GET | `/api/tutors/{id}/slots` | Slots for the next 14 days, each with `available` true or false |

### Sessions

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| POST | `/api/sessions` | `{ tutorId, course, startsAt }` | Request a slot (Pending) |
| GET | `/api/sessions/mine` | none | Sessions where you are learner or tutor |
| POST | `/api/sessions/{id}/respond` | `{ accept }` | Tutor accepts or declines |
| POST | `/api/sessions/{id}/complete` | none | Tutor marks a confirmed session complete |

### Reviews

| Method | Endpoint | Body | Description |
|--------|----------|------|-------------|
| POST | `/api/reviews` | `{ sessionId, rating, comment? }` | Rate a completed session once (1 to 5) |

### Messages and dashboard

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/messages/threads` | Conversations with the latest message |
| GET | `/api/messages/with/{userId}` | Message history with one user |
| GET | `/api/dashboard` | Upcoming, requests, completed count, session to rate, notifications |
| WS | `/hubs/chat` | Hub: invoke `SendMessage(toUserId, text)`, listen for `ReceiveMessage` |

---

## 10. Data model

```mermaid
erDiagram
    USER ||--o{ SESSION : "learns in"
    USER ||--o{ SESSION : "teaches in"
    USER ||--o{ MESSAGE : sends
    USER ||--o{ NOTIFICATION : receives
    SESSION ||--o| REVIEW : "rated by"

    USER {
        string Id
        string Name
        string Email
        string PasswordHash
        bool IsTutor
        string Year
        string Bio
        list Courses
        list Availability
        double RatingAvg
        int RatingCount
        int SessionsTaught
    }
    SESSION {
        string Id
        string LearnerId
        string TutorId
        string Course
        datetime StartsAt
        string Status
        bool Rated
    }
    MESSAGE {
        string Id
        string FromId
        string ToId
        string Text
        datetime SentAt
    }
    REVIEW {
        string Id
        string SessionId
        int Rating
        string Comment
    }
    NOTIFICATION {
        string Id
        string UserId
        string Text
        datetime CreatedAt
    }
```

**Collections:** `users`, `sessions`, `messages`, `reviews`, `notifications`.

**Indexes:** unique on `users.Email`; `sessions (TutorId, StartsAt)`; `messages (FromId, ToId, SentAt)`.

---

## 11. Frontend guide

| Area | Files | Responsibility |
|------|-------|----------------|
| App shell | `App.jsx`, `Sidebar.jsx` | Navigation, page switching, booking dialog, toasts |
| Auth | `AuthContext.jsx`, `Login.jsx` | Sign in and sign up, stores the JWT, logs out on expiry |
| Discover | `Discover.jsx`, `TutorCard.jsx`, `BookingDialog.jsx` | Search, filters, tutor cards with schedules, slot picker |
| Teach | `Teach.jsx` | Become a tutor: courses, bio, weekly schedule editor |
| Messages | `Messages.jsx`, `useChat.js` | SignalR connection and conversation threads |
| Dashboard | `Dashboard.jsx`, `useDashboard.js` | Sessions, requests, ratings, notifications |
| Shared | `api.js`, `utils.js`, `Avatar.jsx`, `Toast.jsx` | Fetch wrapper that attaches the token, date and schedule helpers |

**Design system.** Colors, gradients and spacing are CSS custom properties in `base.css`. Light and dark themes redefine the same tokens, so components never hard-code colors. The primary gradient runs violet → magenta → orange, with mint for availability and success.

---

## 12. Security

- **Passwords** are hashed with BCrypt and never returned by the API.
- **Tokens** are validated for signature, issuer, audience and expiry. A `401` clears the session and returns the user to the login page.
- **Authorization:** every endpoint except register and login needs a valid token. Tutors can only respond to their own requests, and learners can only rate their own completed sessions.
- **Server-side validation:** slot validity, double-booking, ratings, message length and schedule windows are all checked on the server.
- **Secrets:** never commit a real MongoDB password or production `Jwt:Key`. Use environment variables or `dotnet user-secrets`. If a secret was ever pushed, rotate it, because deleting it in a later commit does not remove it from Git history.

---

## 13. Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `Couldn't find a project to run` | Wrong folder | Run `dotnet run` inside `backend/PeerLearn.Api` |
| Framework `8.0.0` not found, only `10.x` installed | Project targets a different .NET than the one installed | Set `<TargetFramework>` in the `.csproj` to match your SDK (for example `net10.0`), or install the .NET 8 runtime |
| `DnsResponseException` / DNS timeout | Network blocks the `mongodb+srv://` lookup | Use the **Legacy URI String** (`mongodb://`) from Atlas, or change DNS servers |
| `TimeoutException`, server selection failed | IP not allowed, or port 27017 blocked | Add your IP under Atlas **Network Access**, or try another network |
| `Authentication failed` | Wrong database user password | Use the **database user** password, not your Atlas login password |
| `CS0234: namespace 'Data' / 'Dtos' does not exist` | `MongoContext.cs` or `Dtos.cs` missing | Restore both files, and only delete files you intend to remove |
| Sign-up shows "Something went wrong" | The API is not running or crashed | Check the backend terminal for the error |
| Old demo tutors still showing | Data left in the database from an earlier version | Drop the `peerlearn` database in Atlas and restart |
| Tutor does not appear in search | Tutoring is off, no courses, no schedule, or no free slot in 14 days | Check the **Teach** tab. Tutors do not see themselves in search |

---

## 14. Team workflow

**Branching.** Work on `main` for small changes, or on short-lived `feature/<name>` branches with a pull request.

**Commit messages** follow a simple convention:

```
<type>: <short description in the present tense>
```

| Type | Use for |
|------|---------|
| `feat` | A new feature |
| `fix` | A bug fix |
| `docs` | Documentation or comments |
| `refactor` | Restructuring without changing behavior |
| `style` | CSS or formatting |
| `test` | Tests |
| `chore` | Setup and configuration |

**Before pushing:**

```powershell
git pull --rebase origin main
git push origin main
```

If a push is rejected, someone pushed first, so run the same two commands again. Check that your Git email matches your GitHub account so commits count on your profile.

---

## 15. Known limitations and roadmap

**Current limitations**

- There is no interface button for a tutor to mark a session **complete**. The endpoint exists, so ratings cannot be reached from the interface until it is added.
- Schedules and slots use **UTC** and have no time-zone setting.
- Slots are fixed at **one hour**.
- No email notifications, password reset or admin area.
- No pagination on tutor search or chat history.
- Sample data is not included, so the database starts empty.

**Roadmap**

- [ ] "Mark complete" button on the dashboard
- [ ] Per-tutor time zones and configurable session length
- [ ] Tutor profile pages with written reviews
- [ ] Email or push notifications
- [ ] Password reset and email verification
- [ ] Automated tests for slot generation, booking rules and ratings
- [ ] Docker setup for one-command local start

---

<div align="center">

Built by the PeerLearn Connect team · DCIT 318 · Programming II

</div>
