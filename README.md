# Bruno Vehicle Hire

Solid-level assessment: vehicle hire system covering **Vehicles**, **Customers**, and **Bookings**.

## Stack

| Layer | Choice |
| --- | --- |
| Backend | .NET 8, Clean Architecture, CQRS (MediatR), FluentValidation, EF Core, PostgreSQL |
| Frontend | React + TypeScript, Vite, React Router, Axios, React Query, React Hook Form, Zod |
| Auth | Static API key (`X-Api-Key`) |
| Run | Docker Compose (postgres + api + frontend) or local tooling |

## Architecture

```
backend/
  Bruno.Domain/          Rich entities, DateRange VO, domain events
  Bruno.Application/     Commands / queries, DTOs, validators
  Bruno.Infrastructure/  EF Core, repositories, Unit of Work, seed + migrations
  Bruno.Api/             Thin controllers, API key + exception middleware, Swagger
  Bruno.Tests/           Command, query, and domain rule tests
frontend/
  src/app/               Router, layout, error boundary
  src/features/          vehicles | customers | bookings (+ viewModels per feature)
  src/shared/            api client (DTOs), components, hooks, utils
```

Business rules live in the domain. Application handlers orchestrate persistence. Controllers only dispatch MediatR requests.

## Domain assumptions

- **Inclusive day pricing:** `TotalPrice = DailyRate × (EndDate − StartDate + 1)`
- `EndDate` must be greater than `StartDate` (spec)
- **No same-day handoff:** closed-interval overlap (`StartA <= EndB && StartB <= EndA`); only **Active** bookings block a vehicle
- Soft-deleted vehicles cannot be booked; hidden by default; `includeDeleted=true` shows them
- Customers cannot be deleted while they have **any** bookings (per spec)
- Booking status changes are **explicit** (`PATCH /api/bookings/{id}/status`); no auto-complete
- Bookings can be hard-deleted only when `StartDate > today (UTC)`
- Booking date/vehicle edits after create are out of v1 scope
- Single API key acts as “admin”; not real RBAC

## Run with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
| --- | --- |
| Frontend | http://localhost:5173 |
| API / Swagger | http://localhost:5080/swagger |
| Postgres | localhost:5432 (`bruno` / `bruno_dev_password`) |

API key: `bruno-dev-api-key` (send as `X-Api-Key`).

Seed data creates sample vehicles, customers, and one future booking on first API startup.

## Run locally (dev)

### Database

```bash
docker compose up postgres -d
```

### API

```bash
cd backend
dotnet run --project Bruno.Api
```

API listens on http://localhost:5080

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Uses `frontend/.env` (copy from `.env.example`):

```bash
cp frontend/.env.example frontend/.env
```

```
VITE_API_URL=http://localhost:5080
VITE_API_KEY=bruno-dev-api-key
```

Dev API keys only — do not commit real secrets. `.env` is gitignored.

### Tests

```bash
cd backend
dotnet test
```

## API overview

- `GET/POST /api/vehicles`, `GET/PUT/DELETE /api/vehicles/{id}` (DELETE = soft delete)
- `GET/POST /api/customers`, `GET/PUT/DELETE /api/customers/{id}`
- `GET/POST /api/bookings`, `GET /api/bookings/{id}`, `PATCH /api/bookings/{id}/status`, `DELETE /api/bookings/{id}`

List endpoints support pagination and filtering.

## Evaluation notes

Implements Clean Architecture, rich domain models, CQRS, repository + UoW, FluentValidation, EF query filters, global exception handling, React feature modules with React Query cache invalidation, reusable modals/confirmations, and centralized API error toasts.
