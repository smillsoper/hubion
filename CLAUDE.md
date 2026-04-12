# Claude Code — Session Memory

**Read ARCHITECTURE.md.** It is the authoritative reference for all development decisions. Every session should start by reading it.

---

## Current Project Status

**As of:** 2026-04-12 (Session 1 complete)

### Solution Structure

```
Hubion.slnx
├── Hubion.Domain          ← Core domain models, value objects, enums. No dependencies.
├── Hubion.Application     ← Business logic, use cases, interfaces. Depends on Domain only.
├── Hubion.Infrastructure  ← EF Core + Npgsql, repositories, services. Depends on Application + Domain.
└── Hubion.Api             ← ASP.NET Core Web API. Depends on Application + Infrastructure.
```

Clean Architecture dependency chain: `Domain ← Application ← Infrastructure ← Api`  
Target framework: **net10.0** across all projects.  
Build status: **0 warnings, 0 errors.**

---

### What Is Done

**Infrastructure / Docker**
- `docker-compose.yml` — full local dev stack running:
  - PostgreSQL 16 on `5432` (healthy)
  - Redis 7 on `6379` (healthy)
  - Nginx on `80` — wildcard subdomain proxy to API on host port `5135`
  - pgAdmin 4 on `5050` — `admin@hubion.dev` / `hubion_dev`
  - MailHog on `8025` — SMTP trap on `1025`
  - FreeSWITCH — commented out, enabled when telephony session begins
- `nginx/local.conf` — subdomain routing + SignalR WebSocket support
- `freeswitch/conf/` and `freeswitch/sounds/` placeholder directories
- Hosts file entries: `hubion.local`, `tms.hubion.local`, `demo.hubion.local` → `127.0.0.1`

**Secrets**
- `.NET User Secrets` initialized for `Hubion.Api` — connection string NOT in any committed file
- Local connection: `Host=localhost;Port=5432;Database=hubion_master;Username=hubion;Password=hubion_dev`

**Domain**
- `Tenant` entity — factory method `Tenant.Create(name, subdomain, planTier, timezone)`, private setters
- `TenantFeatureFlags` value object — 8 flags (telephony_native, telephony_byod, web_automation, oms_built_in, shopify_adapter, advanced_reporting, parallel_queuing, api_builder), all default off

**Application**
- `ITenantRepository` — GetById, GetBySubdomain, GetByCustomDomain, SubdomainExists, Add, SaveChanges
- `ITenantProvisioningService` — ProvisionAsync

**Infrastructure**
- `TenantConfiguration` — EF fluent config: snake_case columns, JSONB `feature_flags`, unique indexes on subdomain/schema_name/custom_domain
- `TenantRepository` — EF Core implementation
- `TenantProvisioningService` — creates tenant record + `CREATE SCHEMA tenant_{subdomain}` atomically
- `ServiceCollectionExtensions.AddInfrastructure()` — single DI registration call in Program.cs
- EF migration `20260412140034_CreateTenantsTable` — applied to local PostgreSQL

**API**
- `TenantResolutionMiddleware` — reads `X-Tenant-Subdomain` header (set by Nginx), falls back to parsing Host header. Sets `context.Items["Tenant"]` for downstream use.
- `TenantsEndpoints` — `POST /api/v1/tenants` (provision), `GET /api/v1/tenants/{id}`
- `Program.cs` — `AddInfrastructure()` + `UseTenantResolution()` + `MapTenantsEndpoints()`

**Database (verified in PostgreSQL)**
- `public.tenants` — all columns correct (`timestamptz`, `jsonb`, proper lengths)
- `public.__EFMigrationsHistory` — migration tracked

---

### What Is NOT Done Yet (Session 2+)

- **First API endpoint returning call record data** — deferred from Session 1, natural start of Session 2
- **Call record domain model** — `CallRecord`, `CallInteraction` entities (ARCHITECTURE.md §19, §22)
- **`Hubion.Worker`** — background service project not yet created
- **`Hubion.HubService`** — SignalR hub project not yet created
- **`Hubion.Integrations`** — adapter framework project not yet created
- **`Hubion.Web`** — React frontend not yet scaffolded
- **Test projects** — none created yet
- **Authentication** — no auth middleware or JWT yet

---

### Key Architecture Decisions (from ARCHITECTURE.md)

- **Single Record of Truth** — one `call_records` row is authoritative for everything on a call
- **Separate PostgreSQL schema per tenant** — `tenant_{subdomain}` schema, `public` for platform tables
- **Secrets** — Azure Key Vault (prod), .NET User Secrets (local). Never in config files or source.
- **All timestamps `timestamptz`** — UTC in DB, ISO 8601 on wire, IANA timezone at display
- **JSONB for flexible/unqueried fields** — typed columns for WHERE/GROUP BY/ORDER BY/aggregates
- **No-code flow designer** — React Flow canvas, JSON flow execution engine
- **Variable resolution engine** — single shared service, `{{namespace.field}}` syntax everywhere

---

### Running the Stack

```bash
docker compose up -d        # start all services
docker compose down         # stop all services
dotnet watch run --project Hubion.Api   # hot-reload API (runs on localhost:5135)
```

pgAdmin: http://localhost:5050  
MailHog: http://localhost:8025
