# Claude Code — Session Memory

**Read ARCHITECTURE.md.** It is the authoritative reference for all development decisions. Every session should start by reading it.

DevLog.md needs to be updated every session with the session start date and time, session end date and time, session duration, total duration, and what was done for the session.  sessions 1 and 2 were logged without timestamps. These are REQUIRED GOING FORWARD. Ask for the current date and time for logging if needed.

---

## Current Project Status

**As of:** 2026-04-20 (Session 6 complete)

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
- Hosts file entries: `hubion.local`, `tms.hubion.local`, `demo.hubion.local` → `127.0.0.1`
- `Original Application/` is in `.gitignore` — local reference only, never committed

**Secrets**
- `.NET User Secrets` initialized for `Hubion.Api` — connection string NOT in any committed file
- Local connection: `Host=localhost;Port=5432;Database=hubion_master;Username=hubion;Password=hubion_dev`

**Domain**
- `Tenant` entity — `Tenant.Create(name, subdomain, planTier, timezone)`, private setters
- `TenantFeatureFlags` value object — 8 flags, all default off
- `CallRecord` entity — full schema per ARCHITECTURE.md §19
  - `handle_time_seconds` — PostgreSQL generated stored column
  - Typed JSONB: `Addresses` (CallAddresses), `CommitmentEvents` (List\<CommitmentEvent\>)
  - Opaque JSONB strings (owned by future engines): `FlowExecutionState`, `ApiResponseCache`, `TelephonyEvents`
  - PCI lifecycle fields: `SensitiveData`, `SensitiveDataStoredAt`, `SensitiveDataWipedAt`, `SensitiveWipeReason`
  - Static constant classes: `CallSource`, `CallRecordType`, `CallRecordStatus`
- `CallInteraction` entity — full schema per ARCHITECTURE.md §22
  - Static constant classes: `InteractionType`, `InteractionStatus`
- `AddressData` value object — full production field set (prefix, street, unit, flags: IsPOBox, IsCanada, IsForeign, IsMilitary, IsOutlyingUS, IsAKHI, IsVerified)
- `CallAddresses` value object — `Billing` + `Shipping` envelope (JSONB on call_records)
- `CommitmentEvent` value object — lock entry with supervisor override fields

**Application**
- `ITenantRepository`
- `ITenantProvisioningService`
- `ICallRecordRepository`
- `IAgentRepository`
- `IPasswordHasher`
- `ITokenService`
- `TenantContext` — scoped service holding the resolved `Tenant` for the current request

**Infrastructure**
- `HubionDbContext` — `public` schema, `tenants` table
- `TenantDbContext` — no default schema, `agents` + `call_records` + `call_interactions`; search_path applied per connection
- `TenantDbContextFactory` — builds `TenantDbContext` with `Search Path=tenant_{schema},public`
- `ScopedTenantDbContextFactory` — resolves tenant from `TenantContext`, creates context for current request
- `TenantDbContextDesignTimeFactory` — EF tooling targets `tenant_tms`
- All EF configurations: `TenantConfiguration`, `AgentConfiguration`, `CallRecordConfiguration`, `CallInteractionConfiguration`
- All repositories: `TenantRepository`, `AgentRepository`, `CallRecordRepository`
- `TenantProvisioningService` — creates schema + runs `MigrateAsync` on provision
- `PasswordHasher` — BCrypt enhanced hashing, work factor 12 (singleton)
- `JwtTokenService` — HS256, 480-min expiry, full tenant+agent claims
- `ServiceCollectionExtensions.AddInfrastructure()`

**Flow Engine (Session 5)**
- `Flow` entity + `FlowSession` entity — full lifecycle methods
- `FlowType` (`crm` | `telephony`), `FlowSessionStatus` (`active` | `complete` | `abandoned`)
- `IVariableResolver` + `VariableResolver` — `{{namespace.field}}` tags, all 7 namespaces, condition evaluator
- `VariableContext` — runtime data bag passed to resolver (call_record, caller, agent, tenant, input, api, flow)
- `FlowExecutionContext` — full runtime state, Redis serialization
- `INodeHandler` + `NodeHandlerBase` — dispatch interface + shared helpers
- Node handlers: `ScriptNodeHandler`, `InputNodeHandler`, `BranchNodeHandler`, `SetVariableNodeHandler`, `ApiCallNodeHandler`, `EndNodeHandler`
- `FlowEngine` — Start/Advance/GetCurrentState, auto-advance loop, Redis (12hr TTL) + PostgreSQL on complete; `isStart` parameter distinguishes first-display from agent-advance to prevent script-node infinite loop
- `IFlowNotifier` (Application) + `FlowNotifier` (API) — Clean Architecture SignalR abstraction
- `FlowRepository`, `FlowSessionRepository` — lazy factory pattern
- `FlowConfiguration`, `FlowSessionConfiguration` — EF configs, JSONB columns, indexes
- `StackExchange.Redis` + `Microsoft.Extensions.Http` packages added to Infrastructure

**API**
- `TenantResolutionMiddleware` — resolves tenant from `X-Tenant-Subdomain` header, populates `TenantContext`
- JWT Bearer authentication — ValidateIssuer/Audience/Lifetime/SigningKey, 1-min clock skew; WebSocket token from query string; **`MapInboundClaims = false`** (required — keeps `sub` as `"sub"`, not remapped to `ClaimTypes.NameIdentifier`)
- `POST /api/v1/auth/login` — BCrypt verify + JWT issue; identical 401 for unknown email and wrong password
- `POST /api/v1/agents` — AllowAnonymous() bootstrap endpoint
- `GET /api/v1/agents/{id}` — RequireAuthorization()
- `POST /api/v1/tenants` — provision tenant (creates record + PostgreSQL schema + runs migrations)
- `GET /api/v1/tenants/{id}`
- `GET /api/v1/call-records/{id}` — tenant-scoped, returns full record with interactions
- `POST /api/v1/flows` — create draft flow
- `GET /api/v1/flows` — list active flows for tenant
- `GET /api/v1/flows/{id}` — get flow by id
- `POST /api/v1/flows/{id}/publish` — publish draft
- `POST /api/v1/flow-sessions` — start session, returns first node state
- `GET /api/v1/flow-sessions/{id}` — get current node state (reconnect)
- `POST /api/v1/flow-sessions/{id}/advance` — submit agent input, get next node
- `FlowHub : Hub<IFlowHubClient>` at `/hubs/flow` — JoinSession, LeaveSession, JoinSupervisorView

**Database (verified in PostgreSQL)**
- `public.tenants` — all columns, indexes
- `tenant_tms.agents` — all columns, unique email index
- `tenant_tms.call_records` — all columns, generated column, 7 indexes
- `tenant_tms.call_interactions` — all columns, FK cascade
- `tenant_tms.flows` — definition JSONB, indexes
- `tenant_tms.flow_sessions` — variable_store + execution_history JSONB, indexes
- Each has its own `__EFMigrationsHistory` table

---

### CRMPro Flow Engine Analysis (Session 4 — informs Session 5 build)

**Tag syntax:** `<*...*>` with executable VB.NET between delimiters. Hubion uses `{{namespace.field}}` (declarative, safe, no-code).

**Namespace mapping (CRMPro → Hubion):**
- `CallDetail.*` → `{{call_record.*}}` + `{{caller.*}}`
- `User.*` → `{{agent.*}}`
- `ScriptForm.[ControlName].Text` → `{{input.[node_id]}}`
- `WebServiceControl.Response` → `{{api.[node_id].*}}`
- Programmatically set values → `{{flow.*}}`

**Script structure:** AccordionControl = top-level container; each AccordionItem panel = one flow step. Branching = button click handlers calling `SelectedIndex = N`. Hubion replaces this with a `branch` node with declared condition + named transitions.

**Node types confirmed by real CRMPro usage:**
- `script` ← RichTextBoxControl/LabelControl with ScriptBox template
- `input` ← TextBox, ComboBox, CheckBox, DateTimePicker, AddressControl
- `branch` ← procedural If/Then in button click handlers
- `api_call` ← WebServiceControl (all fields template-based with `<*...*>`)
- `set_variable` ← PostCallCode response mapping after API call
- `end` ← final accordion panel

**Pending decision:** Branch condition complexity — simple (`==`, `>`, `contains`) or full boolean (`&&`, `||`). Recommendation: simple first.

---

### What Is NOT Done Yet (Session 7+)

- **`Hubion.Worker`** — background service project not yet created
- **`Hubion.HubService`** — SignalR hub project not yet created
- **`Hubion.Integrations`** — adapter framework project not yet created
- **`Hubion.Web`** — React frontend not yet scaffolded
- **Test projects** — none created yet
- **Flow engine** — JSON flow execution engine (ARCHITECTURE.md §9)
- **Variable resolution engine** — `{{namespace.field}}` template service (ARCHITECTURE.md §11)

---

### Key Architecture Decisions (from ARCHITECTURE.md)

- **Single Record of Truth** — one `call_records` row is authoritative for everything on a call
- **Separate PostgreSQL schema per tenant** — `tenant_{subdomain}` schema; `public` for platform tables
- **search_path routing** — `TenantDbContext` uses unqualified table names; `Search Path=tenant_{schema},public` on the connection routes to the right schema at runtime
- **Secrets** — Azure Key Vault (prod), .NET User Secrets (local). Never in config files or source.
- **All timestamps `timestamptz`** — UTC in DB, ISO 8601 on wire, IANA timezone at display
- **JSONB for flexible/unqueried fields** — typed columns for WHERE/GROUP BY/ORDER BY/aggregates
- **Commitment events = lock registry** — appended to call record; record state IS lock state

---

### Running the Stack

```bash
docker compose up -d                              # start all services
docker compose down                               # stop all services
dotnet watch run --project Hubion.Api             # hot-reload API (localhost:5135)
dotnet ef migrations add <Name> --context TenantDbContext --project Hubion.Infrastructure --startup-project Hubion.Api
dotnet ef database update --context TenantDbContext --project Hubion.Infrastructure --startup-project Hubion.Api
```

pgAdmin: http://localhost:5050
MailHog: http://localhost:8025
