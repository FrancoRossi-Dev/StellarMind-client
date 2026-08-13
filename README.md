# StellarMinds — MVC Client

> ASP.NET Core MVC web application — separate consumer of the StellarMinds WebAPI.
> Mandatory project — Web Programming and Design 2026 | ORT Uruguay University

---

## Overview

This project is the MVC front-end for the StellarMinds observatory management system. It lives in its own solution (`Client.slnx`) and has **no direct dependency** on the API's domain or application layers. Every operation goes through `AuxiliarClienteHttp`, an `IHttpClientFactory`-backed wrapper that calls the StellarMinds WebAPI over HTTP and deserializes JSON responses into ViewModels.

Authentication is session-based on this side: after login, the JWT token returned by the API is stored in the ASP.NET Core session and attached as a `Bearer` header on every subsequent request.

**API repository:** `Obligatorio-N3D-342742-360021` (separate solution in the parent directory).

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The StellarMinds WebAPI running and reachable (see [API setup](#api-dependency))

---

## API Dependency

The client needs the WebAPI running before it can do anything useful. By default:

| Environment | API base URL (`ApiBaseUrl`) |
|---|---|
| Development | `https://localhost:7077/` |
| Production / fallback | `http://localhost:5074/` |

The value is read from `appsettings.json` / `appsettings.Development.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7077/"
}
```

Change this to point at a deployed SOMEE instance or any other host before publishing.

---

## Running Locally

```bash
# 1. Start the WebAPI first (from the API solution)
cd ../Obligatorio-N3D-342742-360021/StellarMinds.WebApi
dotnet run

# 2. Start the MVC client (from this solution)
cd Client
dotnet run --project Obligatorio-N3D-342742-360021-Client
```

The MVC app will be available at `https://localhost:7077` (development) and lands on the login page by default.

---

## Deployment (Render)

The app ships with a multi-stage `Dockerfile` (`mcr.microsoft.com/dotnet/sdk:10.0` build stage, `mcr.microsoft.com/dotnet/aspnet:10.0` runtime stage) and a `render.yaml` Blueprint for [Render](https://render.com).

**Key points:**

- The container listens on `$PORT` (Render sets this at runtime; falls back to `8080` for local `docker run`), not on a fixed port.
- `Program.cs` trusts `X-Forwarded-Proto`/`X-Forwarded-For` from Render's edge proxy — required so `UseHttpsRedirection()`/`UseHsts()` don't loop, since Render terminates TLS and forwards plain HTTP internally.
- `ApiBaseUrl` should be set as an environment variable on the Render service (it falls back to the SOMEE URL baked into `appsettings.json` otherwise).

**Deploy via Blueprint:** connect the repo on Render and point it at `render.yaml`, or create the Web Service manually with runtime "Docker" and the Dockerfile at the repo root. Either way, set `ApiBaseUrl` in the service's environment variables before the first deploy.

**Local Docker test:**

```bash
docker build -t stellarminds-client .
docker run -p 8080:8080 -e ApiBaseUrl=https://obligatorio342742360021.somee.com/ stellarminds-client
```

---

## Project Structure

```
Client/
├── Client.slnx
└── Obligatorio-N3D-342742-360021-Client/
    ├── Controllers/
    │   ├── HomeController.cs           # Dashboard (logged-in landing for non-coordinators)
    │   ├── UsersController.cs          # Login, logout, member CRUD
    │   ├── EquipmentController.cs      # Equipment CRUD (all four types)
    │   ├── LoansController.cs          # Loan requests + tickets management
    │   ├── ObservationNightsController.cs  # Observation night CRUD
    │   └── ReportsController.cs        # Ranking, by-telescope, loan audit
    ├── Filters/
    │   ├── LoggedUserFilter.cs         # Redirects to login if no active session
    │   ├── AlreadyLoggedFilter.cs      # Redirects away from login if already logged in
    │   └── AccessFilter.cs             # Role-based action guard
    ├── Models/                         # ViewModels and DTOs (no domain types)
    ├── Services/Http/
    │   └── AuxiliarClienteHttp.cs      # HttpClient wrapper — all API calls go through here
    ├── Views/
    │   ├── Equipment/   (Index, Create, Edit)
    │   ├── Home/        (Index)
    │   ├── Loans/       (Index, Create, MyLoans)
    │   ├── ObservationNights/ (Index, Create, Edit, Details)
    │   ├── Reports/     (Ranking, ByTelescope, LoanAudit, AuditDetail)
    │   └── Users/       (Index, Create, Edit, Login)
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## Authentication and Session

After a successful login the following values are stored in the ASP.NET Core session:

| Key | Value |
|---|---|
| `UserId` | `int` — the user's database ID |
| `Username` | `string` |
| `UserRole` | `string` — `Admin`, `Coordinator`, or `Member` |
| `Email` | `string` |
| `Token` | JWT bearer token issued by the API |

The token is attached automatically by `AuxiliarClienteHttp` on every outgoing request. Sessions expire after **30 minutes of inactivity**.

### Post-login redirect

| Role | Redirects to |
|---|---|
| Coordinator | `Loans/Index` |
| Admin / Member | `Home/Index` |

---

## Access Control

Three action filters enforce auth at the controller/action level:

- **`[LoggedUserFilter]`** — applied to every protected controller/action; redirects to `Users/Login` if `UserId` is absent from the session.
- **`[AlreadyLoggedFilter]`** — applied to the Login action; redirects an already-logged user away from the login page.
- **`[AccessFilter("Role1, Role2")]`** — checks that the session role matches one of the comma-separated values; redirects to `Home/Index` on failure.

---

## Controllers and Routes

### UsersController

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/Users/Login` | — | Login form |
| POST | `/Users/Login` | — | Authenticate against `POST /api/v1/auth/login` |
| GET | `/Users/Logout` | Any | Clears session, redirects to Login |
| GET | `/Users/Index` | Any | Member list with search + role filter |
| GET | `/Users/Create` | Admin | New member form |
| POST | `/Users/Create` | Admin | `POST /api/v1/Users/create` |
| GET | `/Users/Edit/{id}` | Admin | Edit member form |
| POST | `/Users/Edit/{id}` | Admin | `POST /api/v1/Users/update/{id}` |
| POST | `/Users/Delete/{id}` | Admin | `POST /api/v1/Users/delete/{id}` |

### EquipmentController

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/Equipment` | Any | List with optional `?type=` filter |
| GET | `/Equipment/Create` | Admin | Create form (type-aware fields) |
| POST | `/Equipment/Create` | Admin | `POST /api/v1/equipment` |
| GET | `/Equipment/Edit/{id}` | Admin | Edit form |
| POST | `/Equipment/Edit/{id}` | Admin | `PUT /api/v1/equipment/{id}` |
| POST | `/Equipment/Delete/{id}` | Admin | `DELETE /api/v1/equipment/{id}` |

### LoansController

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/Loans` | Admin, Coordinator | All pending loan requests |
| GET | `/Loans/Create` | Any | Loan request form (pre-selects observation night) |
| POST | `/Loans/Create` | Any | `POST /api/v1/loanrequests` |
| POST | `/Loans/Approve/{id}` | Coordinator | `PUT /api/v1/loanrequests/{id}/approve` |
| POST | `/Loans/Reject/{id}` | Coordinator | `PUT /api/v1/loanrequests/{id}/reject` |
| POST | `/Loans/Return/{id}` | Coordinator | `PUT /api/v1/loantickets/{id}/return` |
| POST | `/Loans/Cancel/{id}` | Coordinator | `PUT /api/v1/loantickets/{id}/cancel` |
| POST | `/Loans/CancelRequest/{id}` | Member | `PUT /api/v1/loanrequests/{id}/cancel` (redirects to MyLoans) |
| GET | `/Loans/MyLoans` | Member | Own pending requests + ticket history with overdue indicator |
| POST | `/Loans/Delete/{id}` | Admin | `DELETE /api/v1/loanrequests/{id}` |

### ObservationNightsController

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/ObservationNights` | Any | Members see own nights; Admins/Coordinators see all |
| GET | `/ObservationNights/Details/{id}` | Any | Detail view |
| GET | `/ObservationNights/Create` | Any | Create form with celestial object picker |
| POST | `/ObservationNights/Create` | Any | `POST /api/v1/observationnights` |
| GET | `/ObservationNights/Edit/{id}` | Any | Edit form |
| POST | `/ObservationNights/Edit/{id}` | Any | `PUT /api/v1/observationnights/{id}` |
| POST | `/ObservationNights/Delete/{id}` | Any | `DELETE /api/v1/observationnights/{id}` |
| POST | `/ObservationNights/CancelRequest/{requestId}` | Member | Cancels the associated loan request |

### ReportsController

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/Reports/Ranking` | Any | Celestial objects ranked by observation count (computed client-side) |
| GET | `/Reports/ByTelescope` | Admin | Members filtered by the telescope they have borrowed |
| GET | `/Reports/LoanAudit` | Admin | Full loan ticket history, filterable by coordinator |
| GET | `/Reports/AuditDetail/{id}` | Admin | Ticket detail + associated log events |

---

## Functional Requirements Coverage

| ID | Description | Status | Notes |
|---|---|---|---|
| RF01 | Login / Logout | ✅ | Session-based; JWT forwarded to API |
| RF02 | Register members | ✅ | Create, Edit, Delete in UsersController |
| RF03 | Equipment CRUD | ✅ | All four types; type-aware create/edit form |
| RF04 | Create loan request | ⚠️ | Form works; no JS availability check per letter requirement |
| RF05 | Approve/Reject/Return loan | ✅ | Coordinator actions in LoansController |
| RF06 | Automatic audit | N/A | Handled server-side by the API |
| RF07 | Observation night with AI | ⚠️ | CRUD done; Gemini evaluate button not yet wired |
| RF08 | Loans with overdue indicator | ⚠️ | MyLoans shows `IsOverdue`; no month/year filter form yet |
| RF09 | Members by borrowed telescope | ✅ | Reports/ByTelescope |
| RF10 | Celestial object ranking | ✅ | Reports/Ranking (grouped client-side from observation nights) |
| RF11 | Filterable loan audit | ✅ | Reports/LoanAudit + AuditDetail |

### Known gaps

- **RF04 — JS availability check:** The letter requires that when equipment IDs are entered, a client-side JavaScript call checks availability via a WebAPI endpoint and shows an inline message without submitting the form. This is not implemented.
- **RF07 — Gemini evaluate step:** The letter requires an "Evaluate" button in the observation creation form that calls the AI endpoint before the user confirms. The API endpoint for Gemini evaluation is not yet exposed; once it is, the MVC form needs to be updated to call it and display the indicator.
- **RF08 — Month/year filter:** `MyLoans` lists all of the member's requests and tickets. The API supports `GET /api/v1/loanrequests/user/{id}/{month}/{year}`; a filter form in the view is still pending.

---

## AuxiliarClienteHttp

All HTTP calls go through `Services/Http/AuxiliarClienteHttp`, registered as a scoped service. It wraps `IHttpClientFactory` and exposes two methods:

- `EnviarSolicitud(url, verb, body?, token?)` — sends the request and throws an `Exception` with the API's error message on non-2xx responses.
- `EnviarYDeserializar<T>(url, verb, body?, token?)` — calls `EnviarSolicitud` and deserializes the JSON body to `T` with case-insensitive camelCase mapping.

---

_ORT Uruguay University — Faculty of Engineering — Web Programming and Design 2026_
