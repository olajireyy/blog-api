# BlogApi

# Blog API

A RESTful API built with ASP.NET Core 10 and Entity Framework Core. Handles blog post management with JWT authentication and per-user post ownership.

## Tech Stack

- ASP.NET Core 10 Web API
- Entity Framework Core + SQLite
- JWT Authentication
- BCrypt password hashing

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repo
```bash
   git clone https://github.com/olajireyy/blog-api.git
   cd blog-api
```

2. Install dependencies
```bash
   dotnet restore
```

3. Add your JWT secret to `appsettings.Development.json`
```json
   {
     "Jwt": {
       "Key": "your-secret-key-minimum-32-characters-long",
       "Issuer": "BlogApi",
       "Audience": "BlogClient"
     }
   }
```

4. Run migrations
```bash
   dotnet ef database update
```

5. Run the API
```bash
   dotnet run
```

API runs on `http://localhost:7174` by default.

## API Endpoints

### Auth

| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | `/api/auth/register` | Public | Register a new user, returns JWT token |
| POST | `/api/auth/login` | Public | Login, returns JWT token |

### Posts

| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | `/api/posts` | Public | Get all posts, newest first |
| GET | `/api/posts/{id}` | Public | Get a single post by ID |
| GET | `/api/posts/me` | Auth | Get the logged in user's ID |
| POST | `/api/posts` | Auth | Create a new post |
| PUT | `/api/posts/{id}` | Owner | Update a post (owner only) |
| DELETE | `/api/posts/{id}` | Owner | Delete a post (owner only) |

## Authentication

This API uses JWT Bearer tokens. After registering or logging in, include the token in every protected request:
Authorization: Bearer your-token-here

## Ownership Rules

- Any authenticated user can create posts
- Only the user who created a post can edit or delete it
- Attempting to edit or delete another user's post returns `403 Forbidden`

## Project Structure
BlogApi/
├── Controllers/
│   ├── AuthController.cs     # Register and login endpoints
│   └── PostsController.cs    # CRUD endpoints for posts
├── Models/
│   ├── AppDbContext.cs        # EF Core database context
│   ├── AuthModels.cs          # Register, Login, AuthResponse models
│   ├── Post.cs                # Post model
│   └── User.cs                # User model
├── Migrations/                # EF Core migration files
├── appsettings.json           # App config (no secrets)
└── Program.cs                 # App setup, middleware, DI

## Environment Variables

For production, set these as environment variables instead of appsettings:

| Key | Description |
|-----|-------------|
| `Jwt__Key` | Secret key for signing JWT tokens (min 32 chars) |
| `Jwt__Issuer` | JWT issuer (e.g. BlogApi) |
| `Jwt__Audience` | JWT audience (e.g. BlogClient) |

## Related

- [blog-client](https://github.com/olajireyy/blog-client) — React frontend for this API