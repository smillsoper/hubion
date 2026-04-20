# Hubion Development Log

**Project:** Hubion Platform
**LLC:** Call Center Solutions, LLC
**Repository:** https://github.com/smillsoper/hubion

---

## Summary

| Session | Date | Start | End | Duration | Total Cumulative |
|---------|------|-------|-----|----------|-----------------|
| 1 | 2026-04-12 | 6:32 AM PDT | 7:04 AM PDT | 32 min | 32 min |
| 2 | 2026-04-12 | 7:04 AM PDT | 7:40 AM PDT | 36 min | 68 min |
| 3 | 2026-04-12 | 7:40 AM PDT | 11:12 AM PDT | ~212 min (incl. token limit reset 8:00–11:00 AM) | ~280 min |
| 4 | 2026-04-19 | 8:44 AM PDT | 8:58 AM PDT | 14 min | ~294 min |
| 5 | 2026-04-20 | 9:50 AM PDT | 11:42 AM PDT | 112 min | ~406 min |
| 6 | 2026-04-20 | 3:29 PM PDT | 3:56 PM PDT | 27 min | ~433 min |

---

## Session 1

**Date:** 2026-04-12
**Start:** 6:32 AM PDT
**End:** 7:04 AM PDT
**Duration:** 32 minutes

### Accomplished

- Reviewed ARCHITECTURE.md in full; established authoritative reference for all future sessions
- Assessed initial project scaffold state against Session 1 goals from ARCHITECTURE.md
- Created `Hubion.Domain` project (Clean Architecture domain layer — replaces incorrectly named `Hubion.Core`)
- Created `Hubion.Application` project (Clean Architecture business logic / use cases layer)
- Established correct dependency chain: `Domain ← Application ← Infrastructure ← Api`
- Wired all project references in `.csproj` files and `Hubion.slnx`
- Removed `Class1.cs` placeholders from all projects
- Initialized .NET User Secrets for `Hubion.Api` — connection string removed from `appsettings.Development.json`
- Cleaned `Program.cs` of WeatherForecast boilerplate
- Expanded `docker-compose.yml` to full local dev stack — Redis, Nginx, pgAdmin, MailHog (FreeSWITCH commented out until telephony session)
- Created `nginx/local.conf` — wildcard subdomain routing with SignalR WebSocket support
- Added `freeswitch/conf` and `freeswitch/sounds` directory placeholders
- Created `.gitignore` (standard .NET, secrets, OS, IDE entries)
- Initialized git repository and pushed to GitHub (`smillsoper/hubion`)
- Created `CLAUDE.md` session memory file and `DevLog.md` (this file)
- Built `Tenant` domain entity with factory method (`Tenant.Create`)
- Built `TenantFeatureFlags` value object (all 8 flags, default all off)
- Built `ITenantRepository` and `ITenantProvisioningService` Application interfaces
- Built `TenantConfiguration` (EF fluent — snake_case, JSONB feature_flags, all unique indexes)
- Built `TenantRepository` and `TenantProvisioningService` (creates tenant record + PostgreSQL schema atomically)
- Built `ServiceCollectionExtensions.AddInfrastructure()` — clean DI registration pattern
- Built `TenantResolutionMiddleware` — resolves tenant from `X-Tenant-Subdomain` header per request
- Built tenant endpoints: `POST /api/v1/tenants` (provision), `GET /api/v1/tenants/{id}`
- Created and applied EF migration `CreateTenantsTable`
- Verified `public.tenants` table in PostgreSQL — correct columns, types, indexes

### Session 1 Goals Status (from ARCHITECTURE.md §29)

| Deliverable | Status |
|---|---|
| Solution structure created | ✓ Complete |
| Secrets management wired (.NET User Secrets for local dev) | ✓ Complete |
| PostgreSQL connection with EF Core + Npgsql | ✓ Complete |
| Docker compose stack running locally | ✓ Complete |
| Tenant schema and provisioning | ✓ Complete |
| First API endpoint returning call record data | Deferred → Session 2 |

---

## Session 2

**Date:** 2026-04-12
**Start:** 7:04 AM PDT
**End:** 7:40 AM PDT
**Duration:** 36 minutes

### Accomplished

- Oriented with `Original Application/CRMPro/crmPro_SharedClasses` — confirmed caller identity model (single record, no multi-contact join), confirmed address flags from production (`IsOutlyingUS`, `IsAKHI`)
- Added `Original Application/` to `.gitignore` — reference code is local only, never committed
- Built `AddressData` value object — full production field set including all address classification flags
- Built `CallAddresses` value object — billing + shipping JSONB envelope
- Built `CommitmentEvent` value object — lock registry entry, supervisor override fields
- Built `CallRecord` entity — full schema per ARCHITECTURE.md §19
  - All relational columns with correct types and lengths
  - `handle_time_seconds` as PostgreSQL generated stored column
  - Typed JSONB: `addresses`, `commitment_events`
  - Opaque JSONB strings for engine-owned fields: `flow_execution_state`, `api_response_cache`, `telephony_events`
  - PCI sensitive data lifecycle fields (`sensitive_data`, `stored_at`, `wiped_at`, `wipe_reason`)
  - `AddInteraction()`, `SetCallerIdentity()`, `Complete()`, `DeriveOverallStatus()`, etc.
- Built `CallInteraction` entity — full schema per ARCHITECTURE.md §22 with `InteractionType` and `InteractionStatus` constants
- Built `TenantContext` scoped service — holds resolved `Tenant` for current request, feeds `TenantDbContextFactory`
- Built `ICallRecordRepository` Application interface
- Built `TenantDbContext` — no default schema, relies on `search_path` per connection
- Built `TenantDbContextFactory` — builds context with `Search Path=tenant_{schema},public` per request
- Built `ScopedTenantDbContextFactory` — resolves current tenant from `TenantContext` and creates context
- Built `TenantDbContextDesignTimeFactory` — EF tooling support targeting `tenant_tms`
- Built `CallRecordConfiguration` — all columns, 7 indexes per ARCHITECTURE.md §19, JSONB converters
- Built `CallInteractionConfiguration` — full EF config with FK cascade
- Built `CallRecordRepository`
- Updated `TenantProvisioningService` — now runs `MigrateAsync` on new tenant schema at provision time
- Updated `ServiceCollectionExtensions` — all new services registered
- Updated `TenantResolutionMiddleware` — now also populates `TenantContext` service
- Built `GET /api/v1/call-records/{id}` — tenant-scoped, returns full record with interactions
- Created and applied EF migration `CreateCallRecordsTables` to `tenant_tms` schema
- Verified `tenant_tms.call_records` and `tenant_tms.call_interactions` in PostgreSQL

---

## Session 3

**Date:** 2026-04-12
**Start:** 7:40 AM PDT
**End:** 11:12 AM PDT
**Duration:** ~212 min active (token limit hit at ~8:00 AM PDT; context reset and resumed at ~11:00 AM PDT)

### Accomplished

- Added `Hubion.Infrastructure` NuGet packages: `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- Built `Agent` domain entity — tenant-scoped, stored in tenant schema
  - Factory method `Agent.Create(tenantId, firstName, lastName, email, passwordHash, role)`
  - `RecordLogin()` updates `LastLoginAt`
  - `AgentRole` static class with constants: `agent`, `supervisor`, `admin`
- Built Application interfaces: `IPasswordHasher`, `ITokenService`, `IAgentRepository`
- Built `PasswordHasher` — BCrypt enhanced hashing, work factor 12
- Built `JwtTokenService` — HS256, 480-min expiry, claims: sub/email/given_name/family_name/jti/tenant_id/tenant_schema/tenant_subdomain/role
- Built `AgentConfiguration` — `agents` table, snake_case columns, unique email index, password_hash max 100 chars
- Built `AgentRepository` — lazy `ScopedTenantDbContextFactory` init pattern (same as `CallRecordRepository`)
- Updated `TenantDbContext` — added `Agents` DbSet
- Created and applied EF migration `AddAgentsTable` to `tenant_tms` schema
- Updated `ServiceCollectionExtensions` — wired `IPasswordHasher` (singleton), `ITokenService` (scoped), `IAgentRepository` (scoped)
- Built JWT Bearer pipeline in `Program.cs` — ValidateIssuer/Audience/Lifetime/SigningKey, 1-min clock skew
- Updated `appsettings.json` with Jwt section (Issuer/Audience/ExpiryMinutes); SigningKey in User Secrets only
- Built `POST /api/v1/auth/login` — BCrypt verify, RecordLogin(), GenerateToken(); identical 401 for unknown email and wrong password (no enumeration)
- Built `GET /api/v1/agents/{id}` — RequireAuthorization(), PasswordHash excluded from response
- Built `POST /api/v1/agents` — AllowAnonymous() bootstrap endpoint for first agent creation
- Fixed TenantDbContext DI eager resolution bug — removed TenantDbContext from DI entirely; repositories hold `ScopedTenantDbContextFactory` and lazy-init `Db` via `_db ??= _factory.Create()`
- Seeded TMS tenant directly via SQL INSERT (bootstrap catch-22: protected endpoint, no agents yet)
- **Verified end-to-end auth flow:**
  - `POST /api/v1/agents` (AllowAnonymous) → 200 with agent ID
  - `POST /api/v1/auth/login` correct credentials → 200 with signed JWT
  - `GET /api/v1/agents/{id}` with Bearer token → 200
  - `GET /api/v1/agents/{id}` without token → 401
  - `POST /api/v1/auth/login` wrong password → 401

### Session 3 Goals Status (from ARCHITECTURE.md §29)

| Deliverable | Status |
|---|---|
| JWT Bearer authentication | ✓ Complete |
| BCrypt password hashing | ✓ Complete |
| Agent entity + repository | ✓ Complete |
| Login endpoint | ✓ Complete |
| Protected endpoint enforcement | ✓ Complete |

---

## Session 4

**Date:** 2026-04-19
**Start:** 8:44 AM PDT
**End:** 8:58 AM PDT
**Duration:** 14 minutes (analysis/planning only — session cut short)

### Accomplished

- Read ARCHITECTURE.md in full at session start
- Deep analysis of CRMPro source files to ground flow engine design in real production patterns:
  - `ControlClasses.vb` (10,117 lines) — all control/node types, base class, runtime data model
  - `clsCodeGenerator.vb` (7,177 lines) — script compilation, stored vs. displayed script, code generation
  - `AccordionControl.vb` / `AccordionItem.vb` — script structure (accordion panels = flow steps)
  - `AddressControl.vb` — composite address capture control

### Key Findings from CRMPro Analysis

**Tag syntax:** `<*...*>` delimiters enclosing **executable VB.NET code** (not field references). Hubion will use `{{namespace.field}}` declarative syntax — a deliberate upgrade for safety and no-code ownership.

**Script structure:** `AccordionControl` is the top-level container; each `AccordionItem` panel = one flow step (node). Navigation is linear by default; branching is procedural VB.NET in button click handlers setting `SelectedIndex`.

**Runtime data available to scripts:**
- `CallDetail` (clsCall) → maps to `{{call_record.*}}` + `{{caller.*}}`
- `User` (clsUser) → maps to `{{agent.*}}`
- `ScriptForm.[ControlName]` → maps to `{{input.[node_id]}}`
- `WebServiceControl.Response` → maps to `{{api.[node_id].*}}`

**Node types confirmed by real usage:**
- `script` ← RichTextBoxControl/LabelControl with ScriptBox template
- `input` ← TextBox, ComboBox, CheckBox, DateTimePicker, AddressControl
- `branch` ← button click If/Then/Else navigating to accordion index
- `api_call` ← WebServiceControl (all fields — URL, headers, body — are `<*...*>` template-based)
- `set_variable` ← PostCallCode response mapping after API call
- `end` ← final accordion panel with wrap-up button

**API call control (WebServiceControl):** Every field (URL, method, headers, body, auth) is template-based. Post-call VB.NET code block processes the response. Hubion maps this to `response_map` + `set_variable` — no code, fully declarative.

**Branching:** No declarative condition syntax in CRMPro — all embedded procedural code. Hubion introduces a proper `branch` node with explicit condition expressions.

### CRMPro → Hubion Design Translation

| CRMPro | Hubion |
|---|---|
| `<*VBCode*>` (executable) | `{{namespace.field}}` (declarative) |
| Button click If/Then → `SelectedIndex = N` | `branch` node with declared condition + transitions |
| Accordion panel index navigation | Named node transitions (explicit graph) |
| `ScriptForm.ControlName.Text` | `{{input.node_id}}` |
| `PostCallCode` VB snippet | `response_map` + `set_variable` nodes |
| Client-side WinForms execution | Server-side engine + SignalR push |

### Pending Decision for Next Session Start

**Branch condition complexity:** Simple expressions only (`==`, `>`, `<`, `contains`) evaluated by our own parser — covers 95% of real cases and is faster to build. Or richer boolean expressions (`&&`, `||`) from the start. Recommendation: **simple first, extend later.** Awaiting confirmation before building.

### Planned Build for Session 5

1. `Flow` domain entity + `flows` table
2. `FlowSession` domain entity + `flow_sessions` table
3. `IVariableResolver` + `VariableResolver` (all namespaces)
4. `FlowExecutionContext` (VariableStore, ExecutionHistory, LockedFields, CommitmentEvents)
5. `INodeHandler` + core handlers: `script`, `input`, `branch`, `set_variable`, `end`
6. `FlowEngine` service — StartFlow, AdvanceNode, ProcessInput; Redis active state + PostgreSQL on completion
7. `FlowHub` (SignalR in Hubion.Api) — pushes node state to agent UI
8. Endpoints: `POST /flows`, `GET /flows/{id}`, `POST /flow-sessions`, `POST /flow-sessions/{id}/advance`

→ All of the above were completed in Session 5.

---

## Session 5

**Date:** 2026-04-20
**Start:** 9:50 AM PDT
**End:** 11:42 AM PDT
**Duration:** 112 minutes (token limit hit near end while preparing to test)

### Accomplished

- Analyzed 4 production CRMPro scripts (Dexcom G7, Scrubzz, Shopify CS, ASPCA Donation) + GlobalClasses.vb
- Confirmed branch condition design decision: simple expressions first (`==`, `!=`, `>`, `<`, `>=`, `<=`, `contains`)
- Built full JSON flow engine — all layers:

**Domain**
- `Flow` entity — id, tenant_id, client_id, campaign_id, name, flow_type, version, is_active, definition (JSONB), Publish/Deactivate/UpdateDefinition methods
- `FlowSession` entity — id, flow_id, call_record_id, interaction_id, agent_id, current_node_id, status, variable_store (JSONB), execution_history (JSONB), AdvanceTo/Complete/Abandon methods
- `FlowType` static class: `crm` | `telephony`
- `FlowSessionStatus` static class: `active` | `complete` | `abandoned`

**Application**
- `IFlowRepository` — GetById, GetActiveByTenant, Add, SaveChanges
- `IFlowSessionRepository` — GetById, GetActiveByCallRecord, Add, SaveChanges
- `IVariableResolver` — Resolve, ExtractReferences, EvaluateCondition + `VariableContext` class
- `IFlowEngine` — StartAsync, AdvanceAsync, GetCurrentStateAsync + request/response types
- `IFlowNotifier` — PushNodeStateAsync, PushErrorAsync (keeps Infrastructure free of API dependencies)

**Infrastructure**
- `VariableResolver` — `{{namespace.field}}` tag regex, all namespaces (call_record, caller, agent, tenant, input, api, flow), condition evaluator (==, !=, >, <, >=, <=, contains)
- `FlowExecutionContext` — runtime state with serialization to/from Redis cache entry
- `NodeExecutionRecord` — immutable audit log entry
- `INodeHandler` + `NodeHandlerBase` — dispatch interface and shared JSON/state helpers
- Node handlers:
  - `ScriptNodeHandler` — resolves content, auto-advances on default transition
  - `InputNodeHandler` — waits for agent input, stores in Inputs[node_id], locked field guard
  - `BranchNodeHandler` — evaluates condition, follows true/false/default, transparent to agent
  - `SetVariableNodeHandler` — applies assignments array to FlowVars, transparent to agent
  - `ApiCallNodeHandler` — HTTP call with resolved URL/headers/body, response_map to ApiResults, commitment events on success
  - `EndNodeHandler` — terminal node, sets IsTerminal
- `FlowEngine` — session lifecycle (Start/Advance/GetCurrentState), Redis active state (12hr TTL), PostgreSQL on completion, auto-advance loop for transparent nodes (branch, set_variable), SignalR push via IFlowNotifier
- `FlowRepository`, `FlowSessionRepository` — lazy ScopedTenantDbContextFactory pattern
- `FlowConfiguration`, `FlowSessionConfiguration` — EF snake_case, JSONB columns, indexes
- Added `StackExchange.Redis` and `Microsoft.Extensions.Http` packages
- EF migration `AddFlowsAndFlowSessions` generated ✓

**API**
- `FlowHub : Hub<IFlowHubClient>` — JoinSession, LeaveSession, JoinSupervisorView; JWT WebSocket support (access_token query param)
- `FlowNotifier : IFlowNotifier` — wraps `IHubContext<FlowHub, IFlowHubClient>`
- `POST /api/v1/flows` — create draft flow
- `GET /api/v1/flows` — list active flows for tenant
- `GET /api/v1/flows/{id}` — get flow by id
- `POST /api/v1/flows/{id}/publish` — publish draft
- `POST /api/v1/flow-sessions` — start session (returns first node state)
- `GET /api/v1/flow-sessions/{id}` — get current node state (reconnect)
- `POST /api/v1/flow-sessions/{id}/advance` — submit agent input, get next node
- SignalR hub mapped at `/hubs/flow`
- `AddSignalR()` and `AddScoped<IFlowNotifier, FlowNotifier>()` in Program.cs
- All services registered in `ServiceCollectionExtensions`

**Build:** 0 warnings, 0 errors ✓
**Migration:** Generated ✓ — pending Docker startup to apply to database

### Pending for Next Session Start
- End-to-end test of flow engine (token limit hit before testing could begin):
  1. Start API: `dotnet watch run --project Hubion.Api`
  2. Login to get JWT: `POST /api/v1/auth/login` with `admin@tms.hubion.local`
  3. Create a simple test flow: `POST /api/v1/flows` with a 3-node definition (script → input → end)
  4. Publish it: `POST /api/v1/flows/{id}/publish`
  5. Start a session: `POST /api/v1/flow-sessions`
  6. Advance through nodes: `POST /api/v1/flow-sessions/{id}/advance`
  7. Verify Redis state during, PostgreSQL persistence after completion
- Session 6 (after test passes): Offer/Cart engine or Worker project

---

## Session 6

**Date:** 2026-04-20
**Start:** 3:29 PM PDT
**End:** 3:56 PM PDT
**Duration:** 27 minutes

### Accomplished

End-to-end flow engine test — verified full execution path from flow creation through session completion.

**Bugs discovered and fixed:**

1. **`MapInboundClaims = false` (Program.cs)** — ASP.NET Core's JWT bearer handler remaps the `sub` claim to `ClaimTypes.NameIdentifier` by default. All authorized endpoints called `http.User.FindFirst("sub")` expecting the raw claim name, which always returned null → 401. Fixed by setting `options.MapInboundClaims = false` in JWT bearer options so all JWT claim names are preserved as-is.

2. **`AdvanceInternalAsync` revert bug (FlowEngine.cs)** — After processing a non-auto-advance node (e.g. script) that returned a NextNodeId, the engine set `ctx.CurrentNodeId = result.NextNodeId` then immediately reverted it with `ctx.CurrentNodeId = nodeId`. This caused script nodes to loop forever — each Advance call re-processed the same node. Fixed by introducing an `isStart` parameter: `StartAsync` passes `isStart: true` (stays on entry node after processing), `AdvanceAsync` passes `isStart: false` (advances past the current node and processes the next one until reaching a node that waits for input or is terminal).

3. **Agent password mismatch** — The previously seeded admin agent's BCrypt hash didn't match `Admin123!`. The hash was from a prior session with an unknown password. Deleted and recreated the agent via the bootstrap endpoint.

**End-to-end test verified:**

| Step | Result |
|---|---|
| `POST /api/v1/auth/login` | ✓ 200 with JWT |
| `POST /api/v1/flows` (create draft) | ✓ 201 with flow id |
| `POST /api/v1/flows/{id}/publish` | ✓ 200, is_active=true |
| `POST /api/v1/flow-sessions` | ✓ 200, node_001 (script), `{{agent.id}}` resolved |
| `POST /api/v1/flow-sessions/{id}/advance` (acknowledge script) | ✓ 200, advances to node_002 (input) |
| `POST /api/v1/flow-sessions/{id}/advance` (submit "order") | ✓ 200, auto-advances branch→set_variable→end, `{{flow.call_type}}` = "order", isTerminal=true |
| Redis after completion | ✓ Key deleted |
| PostgreSQL flow_sessions | ✓ status=complete, variable_store has FlowVars + Inputs |
| "billing" path (false branch) | ✓ call_type="billing", same completion flow |

**Test flow definition:** script → input (select) → branch (condition: `{{input.node_002}} == order`) → set_variable (node_004 or node_005) → end

---
