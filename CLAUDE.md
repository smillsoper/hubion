# Claude Code — Session Memory

**Read ARCHITECTURE.md.** It is the authoritative reference for all development decisions. Every session should start by reading it.

---

## Current Project Status

**As of:** 2026-04-12 (Session 1)

### Solution Structure

```
Hubion.sln
├── Hubion.Domain          ← Core domain models, value objects, enums. No dependencies.
├── Hubion.Application     ← Business logic, use cases, interfaces. Depends on Domain only.
├── Hubion.Infrastructure  ← EF Core + Npgsql, external service clients. Depends on Application + Domain.
└── Hubion.Api             ← ASP.NET Core Web API. Depends on Application + Infrastructure.
```

Clean Architecture dependency chain is correct: `Domain ← Application ← Infrastructure ← Api`

**Target framework:** net10.0 across all projects.

### What Is Done

- Solution scaffold with correct Clean Architecture layers
- `Hubion.Domain`, `Hubion.Application`, `Hubion.Infrastructure`, `Hubion.Api` all build (0 warnings, 0 errors)
- EF Core 8 + Npgsql wired — `HubionDbContext` exists in Infrastructure
- `.NET User Secrets` initialized for `Hubion.Api` — connection string is NOT in any config file
- `docker-compose.yml` has PostgreSQL 16 service (`hubion_postgres`, db: `hubion_master`)
- `Program.cs` is clean — OpenAPI + DbContext registration only, no boilerplate

### What Is NOT Done Yet (Session 2+)

- **Tenant model** — `tenants` table in `public` schema, schema-per-tenant provisioning flow
- **First real API endpoint** — call record data (ARCHITECTURE.md §29 Session 1 deliverable, deferred)
- **docker-compose.yml** — missing Redis, FreeSWITCH, Nginx, pgAdmin, MailHog services
- **Hubion.Worker**, **Hubion.HubService**, **Hubion.Integrations** projects — not yet created
- **Hubion.Web** — React frontend, not yet scaffolded
- **Test projects** — none created yet
- **`Hubion.Core` folder** — still on disk (VS Code had it locked during rename). Can be deleted manually.
- **EF Core migrations** — none yet; no tables exist in PostgreSQL

### Key Architecture Decisions (from ARCHITECTURE.md)

- **Single Record of Truth** — every data modeling decision flows from this principle
- **Separate PostgreSQL schema per tenant** — `tenant_{subdomain}` schema per tenant, `public` schema for platform-level tables
- **Secrets** — Azure Key Vault in production, .NET User Secrets in local dev. Never in config files or source.
- **All timestamps as `timestamptz`** — UTC in DB, ISO 8601 on wire, convert to IANA timezone at display
- **JSONB for flexible/unqueried fields, typed columns for anything in WHERE/GROUP BY/ORDER BY**
- **No-code flow designer** — React Flow canvas, JSON flow execution engine, nodes are the unit of logic
- **Variable resolution engine** — single shared service, `{{namespace.field}}` syntax used everywhere

### Local Dev Connection

- PostgreSQL: `Host=localhost;Port=5432;Database=hubion_master;Username=hubion;Password=hubion_dev`
- Stored in .NET User Secrets (not in any committed file)
- Start DB: `docker compose up -d` from solution root
