# ContactConnection Development Log

**Project:** ContactConnection Platform
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
| 7 | 2026-04-20 | 3:56 PM PDT | 4:49 PM PDT | 53 min | ~486 min |
| 8 | 2026-04-20 | 4:53 PM PDT | 5:00 PM PDT | 7 min | ~493 min |
| 9 | 2026-04-21 | 6:15 AM PDT | 6:31 AM PDT | 16 min | ~509 min |
| 10 | 2026-04-21 | 6:31 AM PDT | 6:45 AM PDT | 14 min | ~523 min |
| 11 | 2026-04-22 | 6:32 AM PDT | 6:47 AM PDT | 15 min | ~538 min |
| 12 | 2026-04-22 | 6:48 AM PDT | 7:14 AM PDT | 26 min | ~564 min |
| 13 | 2026-04-23 | 7:26 AM PDT | 8:22 AM PDT | 56 min | ~620 min |
| 14 | 2026-04-23 | 10:19 AM PDT | 10:36 AM PDT | 17 min | ~637 min |
| 15 | 2026-04-24 | 6:51 AM PDT | 7:12 AM PDT | 21 min | ~658 min |
| 16 | 2026-04-24 | 7:12 AM PDT | 7:58 AM PDT | 46 min | ~704 min |
| 17 | 2026-04-26 | 6:45 AM PDT | 6:58 AM PDT | 13 min | ~717 min |
| 18 | 2026-04-26 | 7:01 AM PDT | 7:59 AM PDT | 58 min | ~775 min |

---

## Session 1

**Date:** 2026-04-12
**Start:** 6:32 AM PDT
**End:** 7:04 AM PDT
**Duration:** 32 minutes

### Accomplished

- Reviewed ARCHITECTURE.md in full; established authoritative reference for all future sessions
- Assessed initial project scaffold state against Session 1 goals from ARCHITECTURE.md
- Created `ContactConnection.Domain` project (Clean Architecture domain layer — replaces incorrectly named `ContactConnection.Core`)
- Created `ContactConnection.Application` project (Clean Architecture business logic / use cases layer)
- Established correct dependency chain: `Domain ← Application ← Infrastructure ← Api`
- Wired all project references in `.csproj` files and `ContactConnection.slnx`
- Removed `Class1.cs` placeholders from all projects
- Initialized .NET User Secrets for `ContactConnection.Api` — connection string removed from `appsettings.Development.json`
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

- Added `ContactConnection.Infrastructure` NuGet packages: `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
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

**Tag syntax:** `<*...*>` delimiters enclosing **executable VB.NET code** (not field references). ContactConnection will use `{{namespace.field}}` declarative syntax — a deliberate upgrade for safety and no-code ownership.

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

**API call control (WebServiceControl):** Every field (URL, method, headers, body, auth) is template-based. Post-call VB.NET code block processes the response. ContactConnection maps this to `response_map` + `set_variable` — no code, fully declarative.

**Branching:** No declarative condition syntax in CRMPro — all embedded procedural code. ContactConnection introduces a proper `branch` node with explicit condition expressions.

### CRMPro → ContactConnection Design Translation

| CRMPro | ContactConnection |
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
7. `FlowHub` (SignalR in ContactConnection.Api) — pushes node state to agent UI
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
  1. Start API: `dotnet watch run --project ContactConnection.Api`
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

## Session 7

**Date:** 2026-04-20
**Start:** 3:56 PM PDT
**End:** 4:49 PM PDT
**Duration:** 53 minutes

### Accomplished

Completed CRMPro `DatabaseClasses.vb` commerce class analysis, designed and built the initial Offer/Inventory/Cart engine, identified enterprise-grade gaps, and established the roadmap to fill them.

**CRMPro analysis completed:**

Finished reading all commerce subclasses from `DatabaseClasses.vb`:
- `QPBTier` — `MinQty` + full `Payments` schedule (each QPB tier carries its own independent payment plan, not just a price)
- `clsOfferPayment` — `PaymentNumber`, `PaymentDescription`, `PaymentAmount`, `PaymentIntervalDays`
- `clsOfferShipMethod` — `MethodCode`, delivery window, `BusinessDaysOnly`, `Surcharge`
- `clsOfferPersonalization` — typed prompt (Boolean/Date/Decimal/Long/String), dependency chain, selection list, `ChargeAmount`
- `clsOfferFlag` — opaque key-value metadata
- `clsOfferKit` / `clsOfferVariableKit` / `clsOfferVariableKitItem` — fixed and agent-selectable kit components
- `enumBackorderStatus` — `Available` / `CanBackorder` / `NoBackorder` / `Discontinued`
- `AutoshipInterval` — `IntervalDays`, optional `AutoShipId`

**Key pricing insight confirmed:** QPB (Quantity Price Break) is a payment schedule override keyed by minimum quantity — not a flat price reduction. MixMatchTiers use identical structure but MinQty is checked against the summed quantity of all cart items sharing the same `MixMatchCode`. MixMatch takes priority over QPB.

**Built — Domain:**
- `PaymentInstallment`, `QuantityPriceBreak` value objects (`Commerce/ProductPricing.cs`)
- `ProductShipMethod` value object
- `PersonalizationPromptType` enum + `PersonalizationPrompt`, `PersonalizationDependency`, `PersonalizationSelectionItem` value objects
- `ProductFlag`, `AutoShipInterval` value objects
- `TierRange`, `CartPaymentBreakdown`, `CartPersonalizationAnswer`, `CartKitSelection`, `CartItem`, `CartDocument` value objects — full cart document structure
- `Product` entity — all scalar fields relational, complex structures (pricing tiers, personalization, ship methods, flags) as JSONB; `ProductInventoryStatus` enum; `CanAddToCart()` guard
- `ProductKit` entity — fixed and variable kit components with `CreateFixed()` / `CreateVariable()` factory methods
- `CallRecord.Cart` property + `SetCart()` method

**Built — Application:**
- `IProductRepository` — GetById, GetBySku, Search, GetByMixMatchCode, Add, SaveChanges
- `IPricingService` — `ResolvePayments()` (MixMatch → QPB → base fallback) + `CalculateTotals()`

**Built — Infrastructure:**
- `ProductConfiguration` — all scalar columns, 9 JSONB columns with `HasConversion` + `'[]'::jsonb` defaults, 4 indexes, self-referential FK for variants, cascade to kits
- `ProductKitConfiguration`
- Updated `CallRecordConfiguration` — added `cart` JSONB column
- `ProductRepository` — lazy `ScopedTenantDbContextFactory` pattern, `ILike` search on description
- `PricingService` — MixMatch → QPB → base fallback, weight/subtotal tier resolution, installment splitting with rounding correction in payment 1
- Updated `TenantDbContext` — added `Products`, `ProductKits` DbSets and configurations
- Updated `ServiceCollectionExtensions` — registered `IProductRepository` and `IPricingService`

**Built — API:**
- `POST /api/v1/products` — create product with optional pricing, inventory, payment plan
- `GET /api/v1/products` — search (ILike on description, paginated)
- `GET /api/v1/products/{id}`
- `GET /api/v1/products/sku/{sku}`
- `GET /api/v1/call-records/{id}/cart`
- `PUT /api/v1/call-records/{id}/cart` — accepts CartDocument, calls `CalculateTotals`, saves
- Registered `MapProductsEndpoints()` in `Program.cs`

**Build:** 0 warnings, 0 errors ✓
**Migration:** `AddProductsAndCart` — applied to `tenant_tms` ✓
**Tables verified:** `products`, `product_kits` created; `cart` JSONB column added to `call_records`

### Enterprise Gap Analysis

After building, identified 6 structural gaps between the current implementation and true enterprise-grade:

| Gap | Impact | Planned Session |
|---|---|---|
| Product/Offer separation (same product, multiple campaigns/price points) | High | Session 8 |
| Tax service integration point (`ITaxProvider` abstraction) | Medium | Session 9 |
| Inventory reservation (`qty_reserved` + session lifecycle) | Medium | Session 10 |
| Order entity (post-call fulfillment artifact — Order + OrderLine) | Medium | Session 11 |
| Subscription lifecycle for AutoShip | Medium | Session 12 |
| Category/attribute system (for large catalog clients) | Low (DR), High (catalog) | Session 13 |

Root cause: the original CRMPro system was a broker between the call center and the client's external OMS/fulfillment platform. These gaps are what elevate ContactConnection beyond that role into a full-stack contact center commerce platform.

---

## Session 8

**Date:** 2026-04-20
**Start:** 4:53 PM PDT
**End:** ~5:20 PM PDT
**Duration:** ~27 minutes

### Accomplished

Product/Offer separation — the most structurally significant commerce engine gap from Session 7's analysis. The same physical product can now be sold under multiple independent sales configurations (offers) without duplicating inventory tracking.

**Domain**
- Refactored `Product` entity — removed all pricing/sales fields; now holds only physical item identity, inventory, geographic surcharges, and catalog metadata. Added `Offers` navigation collection.
- Created `Offer` entity — all pricing/sales configuration fields moved from `Product` plus three new fields:
  - `Name` — display label distinguishing offers (e.g. "TV Special", "Web Offer", "Upsell")
  - `IsActive` — explicit activation gate; new offers start inactive (draft)
  - `ValidFrom` / `ValidTo` — optional campaign validity window (nullable `DateTimeOffset`)
  - `IsAvailable()` — checks `IsActive` + validity window in one call
- Updated `CartItem` value object — added `OfferId` field; `ProductId` + `Sku` are now explicitly documented as order-time snapshots

**Application**
- Created `IOfferRepository` — GetById, GetByProductId, GetActive, Add, SaveChanges
- Updated `IPricingService.ResolvePayments` — signature changed from `(Product, ...)` to `(Offer, ...)` to reflect that pricing configuration lives on the Offer

**Infrastructure**
- Updated `ProductConfiguration` — stripped all 24 pricing columns; `products` table now contains only physical/inventory/catalog fields
- Created `OfferConfiguration` — all pricing scalar columns, 7 JSONB columns (`payments`, `quantity_price_breaks`, `mix_match_price_breaks`, `auto_ship_intervals`, `ship_methods`, `personalization`, `flags`), 3 indexes (`ix_offers_product_id`, `ix_offers_mix_match_code`, `ix_offers_tenant_active`), cascade FK from products
- Created `OfferRepository` — lazy factory pattern; includes `Product` navigation on GetById/GetActive
- Updated `ProductRepository` — `GetByMixMatchCodeAsync` now queries via offers (`p.Offers.Any(o => o.MixMatchCode == code && o.IsActive)`)
- Updated `PricingService` — `ResolvePayments` takes `Offer` parameter; all logic unchanged (MixMatch → QPB → base fallback)
- Added `Offers` DbSet to `TenantDbContext` and `OfferConfiguration` to model builder
- Registered `IOfferRepository` → `OfferRepository` in `ServiceCollectionExtensions`

**API**
- Updated `ProductsEndpoints` — `CreateProductRequest` now contains only physical fields (Sku, Description, Weight, Inventory, GeographicSurcharges); product response includes `Offers` collection via `OffersEndpoints.ToResponse`
- Created `OffersEndpoints` — `POST /api/v1/offers`, `GET /api/v1/offers`, `GET /api/v1/offers/{id}`, `POST /api/v1/offers/{id}/activate`, `POST /api/v1/offers/{id}/deactivate`, `GET /api/v1/offers/product/{productId}`
- Registered `MapOffersEndpoints()` in `Program.cs`

**Build:** 0 warnings, 0 errors ✓
**Migration:** `SeparateProductFromOffer` — applied to `tenant_tms` ✓
- Dropped 24 pricing columns from `products`
- Created `offers` table with all moved fields + `name`, `is_active`, `valid_from`, `valid_to`

---

## Session 9

**Date:** 2026-04-21
**Start:** 6:15 AM PDT
**End:** 6:31 AM PDT
**Duration:** 16 minutes

### Accomplished

**Part 1 — Product/Offer separation verification**

All 7 API checks passed. Also discovered and fixed a missing global enum serialization config.

| Step | Expected | Result |
|---|---|---|
| `POST /products` with `"inventoryStatus": "Available"` (string) | 201 | ✓ — but failed initially |
| Fix: added `JsonStringEnumConverter` globally via `ConfigureHttpJsonOptions` | Enums as strings everywhere | ✓ |
| `POST /offers` (TV Special, 3-pay $29.95) | 201, `isActive: false` | ✓ |
| `POST /offers` (Web Offer, 1-pay $24.95) | 201, `isActive: false` | ✓ |
| `POST /offers/{id}/activate` (TV Special) | 200, `isActive: true` | ✓ |
| `GET /products/{id}` | 2 offers embedded, different prices/plans | ✓ |
| `GET /offers` | 1 result — TV Special only (active filter works) | ✓ |
| `GET /offers/product/{productId}` | 2 results — TV Special + Web Offer | ✓ |

**Bonus fix:** `JsonStringEnumConverter` registered globally in `Program.cs` via `ConfigureHttpJsonOptions` — all enums across all endpoints now serialize/deserialize as strings (e.g. `"Available"` instead of `0`).

**Part 2 — Tax service integration**

Built the `ITaxProvider` abstraction with a factory pattern, integrated it into `PricingService`, and verified correct calculations.

**Application**
- `ITaxProvider` — `ProviderKey` + `CalculateTaxAsync(CartDocument, ct)` → `TaxResult(Rate, TaxAmount, Jurisdictions?)`
- `ITaxProviderFactory` — `Resolve(providerKey?)` → correct provider; falls back to default
- `TaxResult` + `JurisdictionTax` records — support future multi-jurisdiction breakdown display
- `IPricingService.CalculateTotals` → renamed `CalculateTotalsAsync` (async — external providers need HTTP)
- `CartDocument.TaxProvider` field added — `null`/`""` = flat rate; future: `"avalara"`, `"taxjar"`

**Infrastructure**
- `FlatRateTaxProvider` — `ProviderKey = ""` (default); applies `CartDocument.TaxRate` directly to taxable subtotal; synchronous, no external calls
- `TaxProviderFactory` — builds dispatch dictionary from all registered `ITaxProvider` instances; throws on startup if no default provider registered
- `PricingService` — now takes `ITaxProviderFactory` in constructor; `CalculateTotalsAsync` delegates tax to resolved provider
- DI: `FlatRateTaxProvider` registered as singleton `ITaxProvider`; `TaxProviderFactory` registered as singleton `ITaxProviderFactory`

**API**
- `CallRecordsEndpoints.SetCart` — updated to `await pricing.CalculateTotalsAsync(...)`

**Bug found and fixed during verification:**
Payment installment breakdown logic was always calling `RoundSplit` for payment 1 even when `splitShippingInPayments`/`splitSalesTaxInPayments` were false. This caused payment 1 to receive 1/N of shipping and tax instead of the full amount. Fixed: when split = false, payment 1 gets the full value; when split = true, `RoundSplit` distributes evenly with remainder in payment 1.

**Verified cart calculation (1 item, 3-payment plan, 8.5% tax, split=false):**

| Field | Expected | Actual |
|---|---|---|
| cartSubtotal | 29.95 | 29.95 ✓ |
| shipping | 7.95 | 7.95 ✓ |
| salesTax | 2.55 | 2.55 ✓ |
| cartTotal | 40.45 | 40.45 ✓ |
| pmt 1 | sub=10.00, ship=7.95, tax=2.55, total=20.50 | ✓ |
| pmt 2 | sub=10.00, ship=0, tax=0, total=10.00 | ✓ |
| pmt 3 | sub=9.95, ship=0, tax=0, total=9.95 | ✓ |

**Build:** 0 warnings, 0 errors ✓

### Pending for Session 10
- **Gap 3: Inventory reservation** — `qty_reserved` column on `products`; reserve on add-to-cart, release on session abandon/expire, confirm (convert to decrement) on order commit

---

## Session 10

**Date:** 2026-04-21
**Start:** 6:31 AM PDT
**End:** 6:45 AM PDT
**Duration:** 14 minutes

### Accomplished

Gap 3: Inventory reservation — soft reservation system tracking units held in active carts, all-or-nothing reserve on cart set, automatic release on cart replace, confirmed decrement on order commit (scaffolded).

**Domain**
- `Product.QtyReserved` property — units currently held in active carts, not yet committed to orders
- `Product.Reserve(qty)` — increments `QtyReserved`; returns false (no change) if `CanAddToCart` fails; only tracks if `DecrementOnOrder = true`
- `Product.Release(qty)` — decrements `QtyReserved` (floor 0); called on cart replace/clear
- `Product.Confirm(qty)` — decrements both `QtyAvailable` and `QtyReserved` (floor 0); called on order commit
- `Product.CanAddToCart(qty)` — updated `NoBackorder` case to use `QtyAvailable - QtyReserved - qty >= MinimumQty` (net of reservations)

**Application**
- `IInventoryService` — `ReserveCartAsync`, `ReleaseCartAsync`, `ConfirmCartAsync`
  - `ReserveCartAsync` — all-or-nothing; validates all items before applying any change; returns list of failing SKUs on partial failure (empty = success)
  - `ReleaseCartAsync` — safe to call on null/empty cart
  - `ConfirmCartAsync` — converts soft reservations to real inventory decrements

**Infrastructure**
- `InventoryService` — lazy `ScopedTenantDbContextFactory` pattern; loads all referenced products in one query per operation; all-or-nothing reservation via validate-then-apply
- `ProductConfiguration` — `qty_reserved` mapped as `integer` column
- `ServiceCollectionExtensions` — registered `IInventoryService → InventoryService` (scoped)

**API**
- `PUT /api/v1/call-records/{id}/cart` — now injects `IInventoryService`:
  - Releases old cart reservations before applying new cart
  - Reserves new cart (all-or-nothing); if any SKU unavailable, restores old cart reservations and returns `409 Conflict` with `{ message, unavailableSkus: [...] }`
  - Only calls `CalculateTotalsAsync` and saves after reservation confirmed

**Database**
- Migration `AddInventoryReservation` — `ALTER TABLE products ADD COLUMN qty_reserved integer NOT NULL DEFAULT 0`
- Applied to `tenant_tms` ✓

**Verified end-to-end:**

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | `PUT /cart` with qty=2, product has 3 available | 200, `qty_reserved=2` in DB | ✓ |
| 2 | Second cart requests 2 more (1 free) | 409, `unavailableSkus: ["INVTEST-001"]` | ✓ |
| 3 | Replace cart: qty 2→1 (release 2, reserve 1) | 200, `qty_reserved=1` in DB | ✓ |

**Build:** 0 warnings, 0 errors ✓

### Pending for Session 11
- **Gap 4: Order entity** — `Order` + `OrderLine` post-call fulfillment artifact; `ConfirmCartAsync` wired to order commit; `OrderStatus` lifecycle

---

## Session 11

**Date:** 2026-04-22
**Start:** 6:32 AM PDT
**End:** 6:47 AM PDT
**Duration:** 15 minutes

### Accomplished

Gap 4: Order entity — post-call fulfillment artifact with full lifecycle, inventory confirmation, and line-level fulfillment tracking.

**Domain**
- `Order` entity — `Id`, `TenantId`, `CallRecordId`; `Status` lifecycle (`confirmed` → `partially_shipped` / `shipped` / `delivered` / `cancelled`); financial snapshot (`Subtotal`, `Shipping`, `SalesTax`, `Discount`, `Total`); `PaymentBreakdowns` JSONB (mirrors cart); order-level fulfillment timestamps; `Cancel()` guard; `RefreshStatus()` — derives order status from line statuses and stamps `ShippedAt`/`DeliveredAt` on first transition
- `OrderLine` entity — full point-in-time snapshot of a `CartItem` at commit; all pricing, shipping, tax, personalization, kit, and payment fields frozen; `FulfillmentStatus` lifecycle (`pending` → `processing` / `shipped` / `delivered` / `cancelled` / `returned`); `Ship(trackingNumber)`, `MarkDelivered()`, `Cancel()` methods; `FromCartItem()` static factory
- `OrderStatus` + `OrderLineStatus` static constant classes

**Application**
- `IOrderRepository` — `GetByIdAsync`, `GetByCallRecordIdAsync`, `AddAsync`, `SaveChangesAsync`
- `IOrderService` — `CreateFromCartAsync(callRecordId)` → `(Order, created: bool)`; idempotent; throws on no/empty cart

**Infrastructure**
- `OrderRepository` — lazy `ScopedTenantDbContextFactory` pattern; `Include(o => o.Lines)` on both fetches
- `OrderService` — loads call record; returns existing order if already committed (idempotent); validates non-empty cart; calls `IInventoryService.ConfirmCartAsync` to convert soft reservations to real decrements; pre-generates `orderId` so `OrderLine.FromCartItem` and `Order.CreateFromCart` use the same ID
- `OrderConfiguration` — `orders` table, `payment_breakdowns` JSONB, 3 indexes (`ix_orders_call_record_id`, `ix_orders_tenant_id`, `ix_orders_tenant_status`)
- `OrderLineConfiguration` — `order_lines` table, 3 JSONB columns (`payments`, `personalization_answers`, `kit_selections`), 2 indexes, cascade delete from orders
- `TenantDbContext` — `Orders` + `OrderLines` DbSets; both configurations applied
- `ServiceCollectionExtensions` — `IOrderRepository → OrderRepository` + `IOrderService → OrderService` (scoped)

**API**
- `POST /api/v1/call-records/{callRecordId}/order` — creates order from cart; 201 on first commit, 200 on repeat (idempotent); 400 if no cart
- `GET /api/v1/call-records/{callRecordId}/order` — get order for a call
- `GET /api/v1/orders/{id}` — get order by id
- `POST /api/v1/orders/{id}/cancel` — cancel order (guards: already shipped)
- `POST /api/v1/orders/{id}/lines/{lineId}/ship` — mark line shipped, apply tracking; refreshes order status
- `POST /api/v1/orders/{id}/lines/{lineId}/deliver` — mark line delivered; refreshes order status
- `POST /api/v1/orders/{id}/lines/{lineId}/cancel` — cancel line (guard: already shipped); refreshes order status

**Database**
- Migration `AddOrders` — creates `orders` and `order_lines` tables with all columns, JSONB defaults, indexes
- Applied to `tenant_tms` ✓

**Verified end-to-end:**

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | `POST /call-records/{id}/order` (first commit) | 201, status=confirmed, total=72.94, 1 line | ✓ |
| 2 | `POST /call-records/{id}/order` (repeat) | 200, same order ID | ✓ |
| 3 | `GET /call-records/{id}/order` | 200, order with lines | ✓ |
| 4 | `GET /orders/{id}` | 200, full order + lines | ✓ |
| 5 | `POST /orders/{id}/lines/{lineId}/ship` | 200, order=shipped, line=shipped, tracking applied | ✓ |
| 6 | `POST /orders/{id}/lines/{lineId}/deliver` | 200, order=delivered, line=delivered | ✓ |
| DB: inventory confirmed | `qty_available` 10→8, `qty_reserved` 0 | ✓ |
| DB: orders table | id, status=delivered, total=72.94 | ✓ |
| DB: order_lines | sku=ORDER-001, qty=2, tracking=1Z999AA10123456784 | ✓ |

**Build:** 0 warnings, 0 errors ✓

### Pending for Session 12
- **Gap 5: Subscription lifecycle** — `Subscription` entity; recurring schedule; `ContactConnection.Worker` background service project scaffolded

---

## Session 12

**Date:** 2026-04-22
**Start:** 6:48 AM PDT
**End:** 7:14 AM PDT
**Duration:** 26 minutes

### Accomplished

Gap 5: Subscription lifecycle + ContactConnection.Worker scaffolding — AutoShip enrollment on order commit, full subscription lifecycle management, and a Worker service that processes due subscriptions on a recurring schedule.

**Domain**
- `Order.CallRecordId` made nullable — autoship renewal orders have no originating call record; `CreateFromSubscription` factory added alongside `CreateFromCart`
- `OrderLine.CreateFromSubscription` factory — minimal snapshot for renewal lines
- `Subscription` entity — `Id`, `TenantId`, `CallRecordId?`, `OriginalOrderId`, `OriginalOrderLineId`; product/offer snapshot fields (frozen at enrollment); `IntervalDays`, `NextShipDate`, `LastShipDate`, `ShipmentCount`; `Status` lifecycle; `IsDue()` guard; `RecordShipment()` advances schedule; `Pause()`, `Resume()`, `Cancel()` methods; `CreateFromOrderLine()` factory
- `SubscriptionStatus` static class: `active` | `paused` | `cancelled`

**Application**
- `ISubscriptionRepository` — `GetByIdAsync`, `GetByCallRecordIdAsync`, `GetDueAsync(cutoff)`, `AddAsync`, `AddRangeAsync`, `SaveChangesAsync`
- `ISubscriptionOrderCreator` — `CreateRenewalOrderAsync(subscription)` — called by Worker for due subscriptions

**Infrastructure**
- `SubscriptionConfiguration` — `subscriptions` table; 3 indexes including `ix_subscriptions_due` on `(status, next_ship_date)` for efficient Worker queries
- `SubscriptionRepository` — lazy factory pattern
- `SubscriptionOrderCreator` — loads product, validates stock (`CanAddToCart`), calls `Product.Confirm`, creates `Order.CreateFromSubscription` + `OrderLine.CreateFromSubscription`, saves via `IOrderRepository`
- `OrderService` updated — after order creation, auto-enrolls all `AutoShip = true && IntervalDays > 0` lines into `Subscription` records via `ISubscriptionRepository.AddRangeAsync`
- `TenantDbContext` — `Subscriptions` DbSet + `SubscriptionConfiguration` applied
- `ServiceCollectionExtensions` — `ISubscriptionRepository`, `ISubscriptionOrderCreator` registered

**API**
- `GET /api/v1/call-records/{id}/subscriptions` — list subscriptions for a call
- `GET /api/v1/subscriptions/{id}` — get subscription
- `POST /api/v1/subscriptions/{id}/pause` — pause (guards: cancelled)
- `POST /api/v1/subscriptions/{id}/resume` — resume (guards: cancelled)
- `POST /api/v1/subscriptions/{id}/cancel` — cancel (idempotent)

**ContactConnection.Worker project (scaffolded)**
- `ContactConnection.Worker.csproj` — `Microsoft.NET.Sdk.Worker`; references `ContactConnection.Application` + `ContactConnection.Infrastructure`; added to `ContactConnection.slnx`
- `Program.cs` — `AddInfrastructure` + `AddHostedService<SubscriptionProcessingService>`
- `SubscriptionProcessingService : BackgroundService` — runs every hour; loads all active tenants from `ContactConnectionDbContext`; for each tenant creates a scoped DI scope and pre-populates `TenantContext.Current` so all scoped services (repositories, inventory) route to the correct schema without HTTP middleware; calls `ISubscriptionOrderCreator.CreateRenewalOrderAsync` + `Subscription.RecordShipment()` for each due subscription; per-subscription error isolation (one failure doesn't stop others)

**Database**
- Migration `AddSubscriptions` — creates `subscriptions` table + alters `orders.call_record_id` to nullable
- Applied to `tenant_tms` ✓

**Verified end-to-end:**

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | `POST /call-records/{id}/order` with AutoShip cart item | 201 order created | ✓ |
| 2 | `GET /call-records/{id}/subscriptions` | 200, 1 subscription auto-enrolled, status=active, interval=30d | ✓ |
| 3 | `GET /subscriptions/{id}` | 200 | ✓ |
| 4 | `POST /subscriptions/{id}/pause` | 200, status=paused | ✓ |
| 5 | `POST /subscriptions/{id}/resume` | 200, status=active | ✓ |
| 6 | `POST /subscriptions/{id}/cancel` | 200, status=cancelled | ✓ |
| DB: subscriptions | sku=AUTOSHIP-001, interval_days=30, next_ship_date=2026-05-22 | ✓ |
| 7 | `POST /subscriptions/{id}/process-now` | 200, renewal order created (callRecordId=null, status=confirmed), shipmentCount=1, nextShipDate+30d, qty_available decremented | ✓ |
| DB: renewal order | id=7ef1e343, call_record_id=null, status=confirmed | ✓ |
| DB: inventory | qty_available 100→99 after renewal confirm | ✓ |

**Bugs found and fixed during Session 12:**

1. **`Order` ID mismatch** — `Order.CreateFromCart` was calling `Guid.NewGuid()` internally, so the ID passed to `OrderLine.FromCartItem` and the order's actual ID were different. Fixed by adding `Guid id` as the first parameter to `CreateFromCart` and `CreateFromSubscription` so the caller pre-generates and controls the ID.

2. **`RefreshStatus()` not stamping timestamps** — `ShippedAt`/`DeliveredAt` on the `Order` row were null after status transitioned to `shipped`/`delivered`. Fixed by adding `ShippedAt ??= DateTimeOffset.UtcNow` and `DeliveredAt ??= DateTimeOffset.UtcNow` inside the relevant `RefreshStatus()` transitions.

3. **Worker EF Core version conflict** — `Microsoft.Extensions.Hosting 10.0.6` pulled EF Core abstractions `10.0.4`, conflicting with Infrastructure's `10.0.5`. Fixed by pinning `<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />` explicitly in `ContactConnection.Worker.csproj`.

**Build:** 0 errors ✓

### Pending for Session 13
- **Gap 6: Category/attribute system** — `ProductCategory` (hierarchical); `ProductAttribute` + `ProductAttributeValue`; faceted search on products endpoint

---

## Session 13

**Date:** 2026-04-23
**Start:** 7:26 AM PDT
**End:** 8:22 AM PDT
**Duration:** 56 minutes

### Accomplished

Gap 6: Category/attribute system — hierarchical product categories, typed attribute definitions with discrete values, and faceted search on the products endpoint. This completes the 6-session Commerce Engine gap roadmap.

**Domain**
- `ProductCategory` entity — hierarchical tree via self-referential `ParentId` (null = root); `Name`, `Slug`, `DisplayOrder`, `IsActive`; `Children` navigation (immediate children only); `Create()`, `Rename()`, `Activate()`, `Deactivate()` methods
- `ProductAttribute` entity — attribute definitions per tenant (e.g. "Color", "Scent"); `Name`, `Slug`, `DisplayOrder`, `IsActive`; `Values` navigation; `AddValue()` factory method
- `ProductAttributeValue` entity — discrete allowed values for an attribute (e.g. "Lavender" for "Scent"); `AttributeId` FK; public `Create()` factory
- `Product` updated — two new many-to-many navigation collections (`Categories`, `AttributeValues`); domain methods: `AssignToCategory(category)`, `RemoveFromCategory(categoryId)`, `SetAttributeValue(value)` (one-value-per-attribute enforcement), `RemoveAttributeValue(valueId)`

**Application**
- `IProductCategoryRepository` — `GetRootsAsync`, `GetChildrenAsync`, `GetByIdAsync`, `AddAsync`, `SaveChangesAsync`
- `IProductAttributeRepository` — `GetByIdAsync`, `GetValueByIdAsync`, `GetAllAsync`, `AddAsync`, `AddValueAsync`, `SaveChangesAsync`
- `IProductRepository.SearchAsync` — signature extended with `categoryId?` and `attributeValueIds?` (AND-faceted) parameters

**Infrastructure**
- `ProductCategoryConfiguration` — `product_categories` table; self-referential FK (Restrict); tenant+slug unique index
- `ProductAttributeConfiguration` — `product_attributes` table; `HasMany(Values).WithOne()` cascade delete
- `ProductAttributeValueConfiguration` — `product_attribute_values` table; FK to `product_attributes`
- `ProductConfiguration` — two `UsingEntity` many-to-many join tables: `product_category_map` (product_id, category_id) + `product_attribute_assignments` (product_id, attribute_value_id)
- `ProductCategoryRepository`, `ProductAttributeRepository` — lazy `ScopedTenantDbContextFactory` pattern
- `ProductRepository.SearchAsync` — updated to `Include(Categories)` + `Include(AttributeValues)`; `Where(p.Categories.Any(c => c.Id == categoryId))` for category facet; per-attribute-value AND filter loop

**API**
- `POST /api/v1/categories` — create root or child category
- `GET /api/v1/categories?parentId=x` — list roots (no parentId) or children (with parentId); each node includes its immediate children
- `GET /api/v1/categories/{id}` — get category with children
- `POST /api/v1/attributes` — create attribute definition
- `GET /api/v1/attributes` — list all active attributes with their values
- `GET /api/v1/attributes/{id}` — get attribute with values
- `POST /api/v1/attributes/{id}/values` — add a value to an attribute
- `POST /api/v1/products/{id}/categories/{categoryId}` — assign product to category
- `DELETE /api/v1/products/{id}/categories/{categoryId}` — remove product from category
- `POST /api/v1/products/{id}/attribute-values/{valueId}` — assign attribute value to product
- `DELETE /api/v1/products/{id}/attribute-values/{valueId}` — remove attribute value from product
- `GET /api/v1/products` — updated: accepts `categoryId` + `attributeValueIds` (repeated param); product response now embeds `categories[]` + `attributeValues[]`

**Database**
- Migration `AddCategoriesAndAttributes` — creates `product_categories`, `product_attributes`, `product_attribute_values`, `product_category_map`, `product_attribute_assignments` tables with all columns, FKs, and indexes
- Applied to `tenant_tms` ✓

**Bug found and fixed:**
EF Core 10 + Npgsql 10 throws `DbUpdateConcurrencyException` ("expected 1 row affected, got 0") when adding new `ProductAttributeValue` entities to a tracked parent collection (`_values`) via `List.Add()`. Root cause: EF Core 10 change detection doesn't properly snapshot collection additions through field-backed `IReadOnlyList<T>` navigations in this context. Fix: bypass collection tracking entirely — create the value via `ProductAttributeValue.Create()` and add it directly to the DbSet via `IProductAttributeRepository.AddValueAsync()`.

**Verified end-to-end:**

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | `POST /categories` (root: Health & Beauty) | 201, parentId=null | ✓ |
| 2 | `POST /categories` (child: Skincare, parentId=root) | 201 | ✓ |
| 3 | `GET /categories` | 200, 1 root with Skincare as child | ✓ |
| 4 | `POST /attributes` (Scent) | 201 | ✓ |
| 5 | `POST /attributes/{id}/values` (Lavender, Citrus) | 201 each | ✓ |
| 6 | `GET /attributes/{id}` | 200, both values embedded | ✓ |
| 7 | `POST /products/{id}/categories/{categoryId}` | 200, product has category | ✓ |
| 8 | `POST /products/{id}/attribute-values/{valueId}` | 200, product has attribute value | ✓ |
| 9 | `GET /products?categoryId=x` | 200, only product in Skincare | ✓ |
| 10 | `GET /products?attributeValueIds=x` | 200, only product with Lavender | ✓ |
| 11 | `GET /products?categoryId=x&attributeValueIds=x` | 200, AND filter — same product | ✓ |
| DB: 5 new tables | categories(2), attributes(1), values(2), cat_map(1), attr_assignments(1) | ✓ |

**Build:** 0 warnings, 0 errors ✓

### Commerce Engine Gap Roadmap — COMPLETE

All 6 gaps from Session 7's analysis are now closed:

| Session | Gap | Status |
|---|---|---|
| 8 | Product/Offer separation | ✓ |
| 9 | Tax service integration | ✓ |
| 10 | Inventory reservation | ✓ |
| 11 | Order entity | ✓ |
| 12 | Subscription lifecycle + ContactConnection.Worker | ✓ |
| 13 | Category/attribute system | ✓ |

---

## Session 14 — Test Projects

**Date:** 2026-04-23
**Start:** 10:19 AM PDT
**End:** 10:36 AM PDT
**Duration:** 17 min
**Cumulative Total:** ~637 min

### Goal

Create `ContactConnection.Domain.Tests` and `ContactConnection.Application.Tests` — the first two test projects per ARCHITECTURE.md spec. Cover domain entity lifecycle logic and PricingService business logic with xUnit tests.

### What Was Built

**`tests/ContactConnection.Domain.Tests/`**
- `ContactConnection.Domain.Tests.csproj` — net10.0, xUnit 2.9.3, references ContactConnection.Domain
- `Domain/ProductInventoryTests.cs` — 13 tests: `CanAddToCart` for all 4 InventoryStatus values; `Reserve`, `Release`, `Confirm` behavior with/without `DecrementOnOrder`
- `Domain/OrderLineLifecycleTests.cs` — 6 tests: `FromCartItem` snapshot, `Ship`, `MarkDelivered`, `Cancel` state transitions; `CreateFromSubscription` snapshot
- `Domain/OrderLifecycleTests.cs` — 9 tests: `Cancel`; `RefreshStatus` for all status transitions (all-pending, partial-ship, all-shipped, all-delivered, delivered+cancelled, shipped+cancelled, all-cancelled, guard when order already cancelled); `CreateFromSubscription` null CallRecordId
- `Domain/SubscriptionLifecycleTests.cs` — 12 tests: `IsDue` (active+future, active+past via reflection on backing field, paused, cancelled); `RecordShipment` count/date/advance/re-not-due; `Pause`, `Resume`, `Cancel`, initial state

**`tests/ContactConnection.Application.Tests/`**
- `ContactConnection.Application.Tests.csproj` — net10.0, xUnit + Moq, references Domain + Application + Infrastructure
- `Commerce/PricingServiceResolvePaymentsTests.cs` — 8 tests: base schedule fallback; QPB threshold met/missed; QPB best-break-wins; MixMatch group threshold met/missed; MixMatch over QPB priority; MixMatch different-code exclusion
- `Commerce/PricingServiceCalculateTotalsTests.cs` — 12 tests: empty cart; subtotal/shipping; tax on taxable items only; shipping-exempt exclusion; weight-tier override; subtotal-tier override; weight-over-subtotal precedence; multi-payment breakdown; shipping not-split; shipping split; single-payment breakdown; RoundSplit remainder in payment 1

**Solution:** `ContactConnection.slnx` updated with both test project paths

**Test results:** 65/65 passed, 0 failures

### Technical Notes

- `xUnit` 2.x does not add global usings — `using Xunit;` required explicitly in each file despite `<ImplicitUsings>enable</ImplicitUsings>`
- EF Core version mismatch warning (10.0.4 vs 10.0.5) in Application.Tests — harmless; tests don't call EF Core directly; the 10.0.5 assembly from Infrastructure is used at runtime
- Subscription `IsDue()` with a past date requires reflection to set the backing field `<NextShipDate>k__BackingField` since `NextShipDate` has a `private set` — pattern documented inline in `SubscriptionLifecycleTests`
- `PricingService` is in Infrastructure but tested here because it's pure computation (no I/O) and is the core business logic of the commerce engine; Application.Tests therefore references Infrastructure
- `Order`/`OrderLine` tests work because `CreateFromCart` stores the passed-in `List<OrderLine>` by reference — calling `line.Ship()` on the original reference mutates the object in the order's `_lines` list, making `RefreshStatus` see the correct statuses

---

## Session 15 — Custom Field System

**Date:** 2026-04-24
**Start:** 6:51 AM PDT
**End:** 7:12 AM PDT
**Duration:** 21 minutes
**Cumulative Total:** ~658 min

### Goal

Build the custom field type system per ARCHITECTURE.md §20 — typed campaign/client/tenant-scoped fields stored on call records with a denormalized JSONB snapshot for fast display.

### What Was Built

**Domain**
- `CustomFieldDataType` static class — 8 well-known type name constants (`string`, `integer`, `decimal`, `currency`, `boolean`, `date`, `datetime`, `json`) + `All` HashSet for validation
- `DataType` entity — platform-level reference (public schema); maps type names to CLR/PostgreSQL types + aggregation metadata (`IsAggregatable`, `AggregationFunctions`); seeded with all 8 types
- `CustomFieldDefinition` entity — scoped to tenant/client/campaign; `FieldName` (stable machine key), `DisplayLabel` (editable UI label), `DataTypeName` FK to data_types; `ScopeRank` computed property drives resolution (0=campaign, 1=client, 2=tenant); `Create()`, `UpdateLabel()`, `SetDisplayOrder()`, `SetRequired()`, `Activate()`, `Deactivate()`
- `CustomFieldValue` entity — one row per (call_record, definition); 7 typed columns (one populated per row); `GetTypedValue()` returns the active column as `object?` for snapshot serialization; `ClearTypedColumns()` ensures only one column is ever set
- `CallRecord.UpdateCustomFieldsSnapshot(string? json)` — new method; updates `custom_fields` JSONB column + stamps `UpdatedAt`

**Application**
- `IDataTypeRepository` — `GetAllAsync`, `GetByNameAsync`
- `ICustomFieldDefinitionRepository` — `GetByIdAsync`, `GetForContextAsync` (scope-filtered), `GetAllForTenantAsync`, `AddAsync`, `SaveChangesAsync`
- `ICustomFieldValueRepository` — `GetByCallRecordAsync` (with Definition nav), `GetByCallRecordAndDefinitionAsync`, `AddAsync`, `DeleteAsync`, `SaveChangesAsync`
- `ICustomFieldService` + `ResolvedCustomField` record — `GetFieldsForCallAsync` (scope resolution + value pairing), `SetValueAsync` (upsert + snapshot refresh), `DeleteValueAsync` (delete + snapshot refresh)

**Infrastructure**
- `DataTypeConfiguration` — `public.data_types` table; stable well-known seed IDs (10000000-…-0001 through -0008); `AggregationFunctions` JSONB with value converter
- `CustomFieldDefinitionConfiguration` — `custom_field_definitions` table; 3 indexes including unique `(tenant_id, field_name, client_id, campaign_id)`
- `CustomFieldValueConfiguration` — `custom_field_values` table; FK cascade from definitions; unique `(call_record_id, definition_id)` index
- `DataTypeRepository` — reads from `ContactConnectionDbContext` (public schema)
- `CustomFieldDefinitionRepository`, `CustomFieldValueRepository` — lazy `ScopedTenantDbContextFactory` pattern
- `CustomFieldService` — scope resolution (groups by `FieldName`, picks lowest `ScopeRank`); typed value parsing via `ApplyTypedValue`; snapshot rebuild via `RefreshSnapshotAsync` (loads all current values → composes JSON dict → updates call_record)
- `ContactConnectionDbContext` — added `DataTypes` DbSet + `DataTypeConfiguration`
- `TenantDbContext` — added `CustomFieldDefinitions`, `CustomFieldValues` DbSets + configurations
- `ServiceCollectionExtensions` — registered `ICustomFieldDefinitionRepository`, `ICustomFieldValueRepository`, `IDataTypeRepository`, `ICustomFieldService`

**API**
- `GET /api/v1/data-types` — list all 8 data types (used by flow designer type picker)
- `POST /api/v1/custom-field-definitions` — create with scope (clientId/campaignId optional); 400 on unknown data type name
- `GET /api/v1/custom-field-definitions` — all for tenant; optional `?clientId=&campaignId=` for scope-filtered view
- `GET /api/v1/custom-field-definitions/{id}` — get by id
- `PATCH /api/v1/custom-field-definitions/{id}` — update label, displayOrder, isRequired, validationRules, isActive
- `GET /api/v1/call-records/{id}/custom-fields` — scope-resolved definitions + current values for a call
- `PUT /api/v1/call-records/{id}/custom-fields/{definitionId}` — upsert typed value; 400 on format error
- `DELETE /api/v1/call-records/{id}/custom-fields/{definitionId}` — remove value; 204

**Database**
- Migration `AddDataTypes` (ContactConnectionDbContext) — creates `public.data_types` with all 8 seed rows ✓
- Migration `AddCustomFields` (TenantDbContext) — creates `custom_field_definitions` + `custom_field_values` with all indexes ✓
- Both applied to target databases ✓

### Verified End-to-End

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1 | `GET /data-types` | 8 types, correct agg metadata | ✓ |
| 2 | `POST /custom-field-definitions` (string) | 201, fieldName normalized to lowercase | ✓ |
| 3 | `POST /custom-field-definitions` (currency, required) | 201, isRequired=true | ✓ |
| 4 | `GET /custom-field-definitions` | 2 results | ✓ |
| 5 | `GET /call-records/{id}/custom-fields` (no values) | 2 definitions, both value=null | ✓ |
| 6 | `PUT .../custom-fields/{def1}` ("SUMMER25") | 200 | ✓ |
| 7 | `PUT .../custom-fields/{def2}` ("29.95") | 200 | ✓ |
| 8 | `GET /call-records/{id}/custom-fields` (values set) | promo_code="SUMMER25", donation_amount=29.95 | ✓ |
| 9 | DB snapshot | `{"promo_code": "SUMMER25", "donation_amount": 29.95}` in call_records.custom_fields | ✓ |
| 10 | `PATCH /custom-field-definitions/{id}` | label updated, isActive=false | ✓ |
| 11 | `DELETE .../custom-fields/{def2}` | 204 | ✓ |
| 12 | DB snapshot after delete | `{"promo_code": "SUMMER25"}` — donation_amount removed | ✓ |
| 13 | Unknown data type name | 400 | ✓ |
| 14 | Bad format value (currency "not-a-number") | 400 | ✓ |

**Build:** 0 warnings, 0 errors ✓

### Key Design Decisions

- `DataType` lives in `ContactConnectionDbContext` (public schema) — platform reference, seeded once; no cross-schema FK from tenant tables (use string `DataTypeName` as the link, validated at domain layer)
- `ScopeRank` on `CustomFieldDefinition` is a computed property (ignored by EF) — drives in-memory scope resolution in `CustomFieldService.ResolveMostSpecific()`
- `ClearTypedColumns()` called before every `Set*()` — ensures only one typed column ever populated per row
- Snapshot refreshed on every `SetValueAsync` and `DeleteValueAsync` — `call_records.custom_fields` always reflects current state without joins on read

---

## Session 16 — Agent UI Scaffold (ContactConnection.Web)

**Date:** 2026-04-24
**Start:** 7:12 AM PDT
**End:** 7:58 AM PDT
**Duration:** 46 minutes
**Cumulative Total:** ~704 min

### Goal

Strategic direction confirmed: build the full ContactConnection platform — not releasing ContactConnection Flow as a standalone product first. Reporting and dashboards are deferred until all other systems (telephony, commerce, flow, custom fields, chat) are complete.

Scaffold `ContactConnection.Web` — the React-based agent UI — with a 3-panel layout and a live flow panel wired to the flow engine backend. Plan and document the tenant-scoped enterprise chat system (placeholder in UI for now).

### Strategic Decisions Made

- **Full platform build order confirmed** — ContactConnection.Web Agent UI → Flow Designer → Chrome Extension → FreeSWITCH/Telephony → ContactConnection.Integrations → Chat → ContactConnection.HubService → Reporting & Dashboards
- **Chat System architecture added to CLAUDE.md** — tenant-scoped, Slack/Pumble-level features: public/private channels, DMs, threads, @mentions, emoji reactions, presence; 5-table data model; `ChatHub : Hub<IChatHubClient>`; Redis for presence

### What Was Built

**`ContactConnection.Web/` — React + Vite + TypeScript SPA**

Config files:
- `package.json` — Vite 4, React 18, Tailwind CSS v3, React Router v6, Zustand v4, `@microsoft/signalr` v8 (pinned to Node 16-compatible versions; Node 24.15.0 installed in background — upgrade packages in Session 17)
- `vite.config.ts` — proxies `/api` → `localhost:5135` and `/hubs` → `ws://localhost:5135`
- `tailwind.config.js` + `postcss.config.js` — Tailwind v3 PostCSS pipeline
- `tsconfig.json`, `tsconfig.app.json`, `tsconfig.node.json`
- `index.html`

Core source:
- `src/main.tsx` — React 18 `createRoot`, `<StrictMode>`
- `src/index.css` — Tailwind directives (`@tailwind base/components/utilities`)
- `src/App.tsx` — `BrowserRouter` with `/login` and `/agent` routes; `RequireAuth` wrapper redirects to login if no token
- `src/vite-env.d.ts` — Vite client type reference

Types / stores / API:
- `src/types/flow.ts` — `FlowNodeState`, `FlowOption`, `StartSessionRequest`, `AdvanceSessionRequest` (mirrors C# `FlowNodeState`)
- `src/stores/authStore.ts` — Zustand with `persist` middleware; holds `token`, `agentId`, `tenantSubdomain`; `setAuth()` + `clearAuth()`
- `src/api/client.ts` — `apiFetch` wrapper auto-injects `Authorization: Bearer` and `X-Tenant-Subdomain` headers from auth store
- `src/api/auth.ts` — `login()` sends raw `fetch` with tenant header before auth state is populated
- `src/api/flows.ts` — `list()`, `startSession()`, `getSession()`, `advance()`

Pages:
- `src/pages/LoginPage.tsx` — subdomain + email + password form; dark theme; calls `authApi.login()`, sets auth store, redirects to `/agent`
- `src/pages/AgentPage.tsx` — thin wrapper that renders `<AgentShell />`

Layout and panels:
- `src/components/AgentShell.tsx` — 3-panel grid: 240px left softphone | flex center flow | 300px right chat; top bar with "ContactConnection" brand + sign-out button
- `src/components/SoftphonePanel.tsx` — placeholder with phone icon + "Coming soon"
- `src/components/ChatPanel.tsx` — placeholder with chat icon + "Channels, DMs & threads / Coming soon"

Flow components (live, wired to backend):
- `src/components/FlowPanel.tsx` — fetches flow list on mount; flow picker dropdown + Start button; `HubConnectionBuilder` with auto-reconnect; `JoinSession`/`LeaveSession` on session change; `advance()` calls `POST /flow-sessions/{id}/advance`; state machine: `idle → loading → running → error`
- `src/components/NodeDisplay.tsx` — renders all node types: `script` (content + Continue), `input` (text/select/checkbox/date/phone inputs + Next), `end` (green check + terminal message); `branch`/`set_variable`/`api_call` all show Continue button

**Build verified:** `npm run build` — 85 modules, 0 errors, 4.13s ✓

### Infrastructure Note

Node.js v16.17.0 was the active version when the session started — incompatible with Vite 6 / Tailwind v4's native oxide bindings. Packages were pinned to Node 16-compatible versions. Node.js 24.15.0 (LTS) was installed via `winget install OpenJS.NodeJS.LTS` in background during the session. Session 17 will open with a fresh terminal on Node 24 and upgrade packages to latest versions.

### Pending for Session 17
- Open fresh terminal, verify `node --version` shows 24.x
- Upgrade `package.json` to latest Vite 6, Tailwind v4, React Router v7, React 19, Zustand v5
- `npm install` + `npm run build` on the upgraded packages
- Begin Flow Designer (visual no-code canvas, React Flow)

---

## Session 17

**Date:** 2026-04-26
**Start:** 6:45 AM PDT
**End:** 6:58 AM PDT
**Duration:** 13 minutes
**Cumulative Total:** ~717 min

### Goal

Upgrade `ContactConnection.Web` packages to current versions (Node 24 now active) and build the Flow Designer — the visual no-code canvas built on React Flow.

### Accomplished

**Package upgrades — ContactConnection.Web**
- Node 24.15.0 confirmed active (installed via winget in Session 16 background)
- Vite 4 → 6.4.2; Tailwind CSS v3 (PostCSS) → v4 (`@tailwindcss/vite` plugin, `@import "tailwindcss"` in CSS, no `tailwind.config.js` or `postcss.config.js` or `autoprefixer`)
- React 18.3 → 19.1; React Router v6 → v7.6; Zustand v4 → v5; TypeScript 5.6 → 5.8
- `@xyflow/react` v12 installed — React Flow canvas library
- Package count: 172 → 125 (Tailwind v4 Vite plugin replaced several PostCSS packages)
- Build: 0 TypeScript errors ✓

**Backend — FlowsEndpoints.cs**
- `PUT /api/v1/flows/{id}` — new endpoint; calls `Flow.UpdateDefinition(definition)`, bumps version, returns detail response
- `GET /api/v1/flows/{id}` — updated to return `definition` field via new `ToDetailResponse()` (previously excluded)
- `UpdateFlowRequest` record added

**Flow Designer — src/types/designer.ts**
- `ContactConnectionNodeType` union type (script, input, branch, set_variable, api_call, end)
- `NodeData` — flat record with all optional fields per node type (compatible with React Flow's generic `Node<T>`)
- `ContactConnectionNodeDef`, `ContactConnectionFlowDefinition` — match the JSON flow schema from ARCHITECTURE.md §9
- `NODE_META` — color, label, description, handle count per type (single/dual/none)
- `defaultNodeData(type)` — factory returning sensible defaults for each type

**Flow Designer — src/api/flows.ts**
- `FlowSummary`, `FlowDetail` interfaces exported
- Added `create`, `getDetail`, `updateDefinition`, `publish` methods for designer use

**Flow Designer — custom node components (src/components/designer/nodes/)**
- `NodeShell.tsx` — shared wrapper: colored header bar, ENTRY badge, target handle (top), source handle(s) (bottom); single/dual/none per type
- Branch/ApiCall dual handles: left handle green (true/success), right handle red (false/error) — visually labeled in node body
- `ScriptNode` (blue #3b82f6) — content preview
- `InputNode` (emerald #10b981) — fieldType + required indicator
- `BranchNode` (amber #f59e0b) — condition expression preview + true/false labels
- `SetVariableNode` (violet #8b5cf6) — assignment count
- `ApiCallNode` (indigo #6366f1) — method + URL preview + success/error labels
- `EndNode` (red #ef4444) — status preview

**Flow Designer — NodePalette.tsx**
- 176px left panel; colored draggable cards per node type; `dataTransfer` sets `application/reactflow-node-type`

**Flow Designer — NodePropertiesPanel.tsx**
- 288px right panel; type-specific form fields:
  - script: content textarea (5 rows), `{{namespace.field}}` hint
  - input: fieldType dropdown (text/select/checkbox/date/phone/email/address), options textarea (select only), required checkbox
  - branch: condition input, operator reference
  - set_variable: dynamic key-value assignment list with add/remove
  - api_call: method dropdown, URL, headers JSON textarea, body JSON textarea
  - end: status input
- "Set as Entry Node" button (hidden when already entry); "Delete Node" button
- Node ID shown for reference

**Flow Designer — FlowDesignerPage.tsx**
- `ReactFlowProvider` outer wrapper; `DesignerCanvas` inner component uses `useReactFlow`
- Drag-and-drop from palette via `onDrop` + `screenToFlowPosition`; first dropped node auto-set as entry
- Edge connections via `onConnect` + `addEdge` (smoothstep type)
- Node click → opens properties panel; pane click → deselects
- Delete key removes selected nodes + connected edges
- `toContactConnectionDef()` — React Flow state → ContactConnection JSON; stores `_pos` in each node for layout persistence
- `fromContactConnectionDef()` — ContactConnection JSON → React Flow state; restores positions from `_pos`
- Save: POST (new flow → redirects to `/designer/{id}`) or PUT (existing flow); status message auto-clears after 3s
- Publish: `POST /flows/{id}/publish`; status message auto-clears after 4s
- Load: fetches `GET /flows/{id}` on mount, parses `definition` JSON, restores full canvas state
- React Flow: `Background` grid, `Controls`, `MiniMap` (color-coded by node type)

**Routing + Navigation**
- `App.tsx` — `/designer` (new flow) and `/designer/:id` (edit existing), both `RequireAuth`-wrapped
- `AgentShell.tsx` — "Flow Designer" link in top bar navigates to `/designer`

**Build:** 0 TypeScript errors, 0 warnings ✓ (253 modules; SignalR annotation warnings from third-party ESM bundle only)

### Pending for Session 18
- Chrome Extension (web automation bridge) — Manifest V3, background service worker, content scripts, frame registry, annotation system

---

## Session 18 — Script Node Rich Text Editor + Branding

**Date:** 2026-04-26
**Start:** 7:01 AM PDT
**End:** 7:59 AM PDT
**Duration:** 58 minutes
**Cumulative Total:** ~775 min

### Goal

Live test the stack, diagnose and fix the input node loop bug, then build the Script node rich text editing experience (TipTap) and place ContactConnection branding assets across the app.

### Accomplished

**Bug fix — input node infinite loop**
- Root cause: TypeScript `AdvanceSessionRequest` used field `input?: string` but the C# record deserialized the property as `inputValue` (camelCase). The backend always received `inputValue: null` → `InputNodeHandler` treated every advance as "no input submitted" → returned the same node. Fixed by renaming the TypeScript field to `inputValue` and updating `FlowPanel.tsx` to send `{ inputValue: input }`.
- Secondary fix: `NodeDisplay.tsx` script content changed from plain text render to `dangerouslySetInnerHTML={{ __html: node.content }}` with `className="script-content"` so HTML-formatted script content renders correctly in the agent UI.

**TipTap rich text editor — RichTextEditor.tsx**
- Installed TipTap packages: `@tiptap/react`, `@tiptap/starter-kit`, `@tiptap/extension-text-style`, `@tiptap/extension-color`, `@tiptap/extension-highlight`, `@tiptap/extension-underline`, `@tiptap/extension-font-family`
- Extensions: StarterKit, TextStyle (named import — default import does not exist), Color, Highlight (multicolor), Underline, FontFamily, custom FontSize (inline TipTap Extension using `addGlobalAttributes` on `textStyle` mark), Image (base64, allowBase64: true)
- Toolbar: font family dropdown (Default/Arial/Georgia/Verdana/Times New Roman/Courier New), font size dropdown (10–28 px), Bold, Italic, Underline, text color picker, highlight color picker, bullet list, numbered list, insert image button, clear formatting, expand button
- Color pickers: swatch grid popover with backdrop dismiss; active color shown as underline bar on button
- Expand button: rendered only when `onExpand` prop is provided; far-right of toolbar
- `index.css`: `.script-editor` rules for TipTap surface; `.script-content` rules for agent UI rendering; image rules for both (`max-width: 100%`, border-radius, selection outline)

**Image support**
- Installed `@tiptap/extension-image`
- Paste: `editorProps.handlePaste` intercepts clipboard events, detects `image/*` MIME, reads as base64 Data URL via FileReader, inserts via ProseMirror transaction
- Insert button: hidden `<input type="file" accept="image/*">` triggered by ref; reads file as base64 Data URL via FileReader, calls `editor.chain().setImage({ src })`
- Images stored as inline base64 in script HTML — no server upload needed

**Popout modal editor**
- `ScriptEditorModal.tsx` — fixed full-viewport overlay (z-50), blurred dark backdrop, 90vw × 85vh white container, scrollable body with full RichTextEditor (no expand button), Done button + backdrop click closes
- `ScriptContentEditor.tsx` — wrapper holding `modalOpen` state; when modal open: compact editor unmounted, dashed placeholder shown so panel keeps shape; when closed: compact editor remounts from latest `content` prop
- `NodePropertiesPanel.tsx` — script case replaced `RichTextEditor` with `<ScriptContentEditor key={node.id} />` (key resets TipTap on node switch)

**ContactConnection branding**
- `Images/hubion-favicon.svg` and `Images/hubion-logo.svg` copied to `ContactConnection.Web/public/`
- `index.html` — `<link rel="icon" href="/hubion-favicon.svg" type="image/svg+xml">` added
- `LoginPage.tsx` — favicon icon (56px, centered) replaces plain "ContactConnection" h1; dark card background makes the blue gradient favicon pop
- `AgentShell.tsx` — favicon icon (24px) + "ContactConnection" white text replaces plain indigo text span in top bar
- `FlowDesignerPage.tsx` — full logo SVG (h-8, 32px tall) added to left of top bar before the Back button; vertical divider separates logo from nav controls; light bar background is the design target for the full wordmark logo

**Build:** 0 TypeScript errors ✓

### Pending for Session 19
- Chrome Extension (web automation bridge) — Manifest V3, background service worker, content scripts, frame registry, annotation system
- Continue working through remaining Flow Designer node types (Input, Branch, Set Variable, API Call, End)
