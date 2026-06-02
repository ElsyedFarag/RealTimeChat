# 💬 RealTimeChat

A full-stack real-time chat application built with **ASP.NET Core** and **React**, supporting private and group conversations with live presence indicators and message delivery receipts.

---

## ✨ Features

- 🔐 **JWT Authentication** — Secure login & registration with access/refresh token flow
- 💬 **Private & Group Chats** — Start one-on-one conversations or create named group channels
- ⚡ **Real-Time Messaging** — Powered by SignalR WebSockets for instant message delivery
- 🟢 **Online Presence** — Live online/offline status with last-seen timestamps (multi-tab aware)
- ✅ **Message Receipts** — Delivered/read status tracking per message
- 👤 **User Profiles** — Avatar uploads, profile updates, and user search
- 🏗️ **Clean Architecture** — Domain, Application, Infrastructure, and API layers fully separated
- 📄 **Swagger UI** — Interactive API documentation in development mode

---

## 🛠️ Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Real-Time | SignalR |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Identity | ASP.NET Core Identity |
| Auth | JWT Bearer Tokens |
| Mapping | AutoMapper |
| API Docs | Swagger / OpenAPI |

### Frontend
| Layer | Technology |
|---|---|
| Framework | React 18 + Vite |
| Routing | React Router v6 |
| Real-Time | @microsoft/signalr |
| HTTP | Axios |
| Forms | React Hook Form + Yup |
| Styling | Tailwind CSS |
| Icons | Lucide React |
| Toasts | React Hot Toast |

---

## 📁 Project Structure

```
RealTimeChat/
├── backend/
│   ├── RealTimeChat.API/            # Controllers, Hubs, Middleware, Program.cs
│   ├── RealTimeChat.Application/    # Services, Interfaces, DTOs, AutoMapper profiles
│   ├── RealTimeChat.Domain/         # Entities, Enums (pure domain logic)
│   ├── RealTimeChat.Infrastructure/ # EF DbContext, Repositories, JWT Provider, Migrations
│   ├── RealTimeChat.Shared/         # Shared response wrappers, settings
│   └── RealTimeChat.sln
└── frontend/
    └── src/
        ├── api/                     # Axios API modules (auth, chats, messages, users)
        ├── components/              # Reusable UI (chat, messages, layout, common)
        ├── contexts/                # AuthContext, ChatContext (global state)
        ├── hooks/                   # useDebounce, useUserSearch
        ├── pages/                   # Auth, Chat, Groups, Profile pages
        ├── services/                # SignalR service wrapper
        ├── types/                   # Shared type definitions
        └── utils/                   # Helper utilities
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (local or remote)
- [Node.js](https://nodejs.org/) v18+

---

### 1. Backend Setup

**Clone the repo and navigate to the backend:**

```bash
git clone <repo-url>
cd RealTimeChat/backend
```

**Configure your connection string** in `RealTimeChat.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=RealTimeChatDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JWT": {
    "Key": "your-secret-key-here",
    "Issuer": "RealTimeChat",
    "Audience": "RealTimeChatUsers",
    "DurationInMinutes": 60
  }
}
```

> ⚠️ **Important:** Replace `JWT.Key` with a long, random secret string before deploying to production.

**Apply migrations and run:**

```bash
cd RealTimeChat.API
dotnet ef database update
dotnet run
```

The API will be available at `http://localhost:5000` and Swagger UI at `http://localhost:5000/swagger`.

---

### 2. Frontend Setup

```bash
cd RealTimeChat/frontend
```

**Create your `.env` file** (use `.env.example` as a reference):

```env
VITE_API_BASE_URL=http://localhost:5000
```

**Install dependencies and run:**

```bash
npm install
npm run dev
```

The app will be available at `http://localhost:5173`.

---

## 🔌 SignalR Hub

The real-time hub is mounted at `/hubs/chat` and requires a valid JWT token passed as a query parameter (`access_token`) for WebSocket connections.

### Hub Events

| Event | Direction | Description |
|---|---|---|
| `UserOnline` | Server → Client | A user connected |
| `UserOffline` | Server → Client | A user disconnected (all tabs closed) |
| `ReceiveMessage` | Server → Client | New message delivered |
| `MessageRead` | Server → Client | Message receipt updated |

---

## 📡 API Endpoints

| Controller | Prefix | Description |
|---|---|---|
| AuthController | `/api/auth` | Register, Login, Refresh, Revoke |
| UsersController | `/api/users` | Get profile, Update, Upload avatar |
| ChatsController | `/api/chats` | List chats, Create private/group, Add/remove members |
| MessagesController | `/api/messages` | Send, Edit, Delete, Mark as read |

Full interactive docs available at `/swagger` when running in Development mode.

---

## 🗃️ Data Model

```
AppUser          ──┐
                   ├── ChatParticipant ──── Chat
RefreshToken ──────┘                        │
UserConnection                           ChatMessage
                                            │
                                        MessageReceipt
```

**Chat types:** `Private` | `Group`  
**Message types:** `Text` | *(extensible)*  
**Message statuses:** `Sent` → `Delivered` → `Read`

---

## ⚙️ Configuration Reference

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JWT:Key` | HMAC-SHA256 signing key (keep secret!) |
| `JWT:Issuer` | Token issuer name |
| `JWT:Audience` | Token audience name |
| `JWT:DurationInMinutes` | Access token lifetime |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m "feat: add your feature"`
4. Push and open a Pull Request

---

## 📄 License

This project is open source. See [LICENSE](LICENSE) for details.
