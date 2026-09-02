# Home Dashboard Plan

Status: **Phase 1 (Member) · Phase 2 (Admin) · Phase 3 (Coordinator) all built & verified** · Last updated: 2026-09-02

Post-build passes (2026-09-02): a bento-grid rewrite so panel edges align across
each section (no ragged mid-layout gaps), then a design/content-priority pass —
`.sky-split` doughnut width-capped so it stops clipping its legend; Admin "Recent
activity" filtered to writes + errors (was ~80% read-log noise); Coordinator
dashboard reordered to lead with the pending queue; explore-row labels to Title
Case; "Jan 26" axis label → "Jan 2026". Details in the devlog.

Every dashboard component's data source, field and calculation was validated
against the live API for all three roles on 2026-09-02 — see
[`dashboards.md`](./dashboards.md) for the per-panel reference and screenshots.

Session-by-session work log: [`member-dashboard-devlog.md`](./member-dashboard-devlog.md)

The current `Home/Index` is only a grid of navigation cards with no data. This plan
replaces it with a real, engaging dashboard. We build **one role at a time**.

| Role | Lands on `Home/Index` today? | Phase |
|------|------------------------------|-------|
| Member | Yes | Phase 1 — built & verified |
| Admin | Yes | Phase 2 — built & verified |
| Coordinator | Yes (redirect fixed 2026-09-02) | Phase 3 — built & verified |

Every role now lands on `Home/Index` after login. `UsersController.Login` no
longer special-cases Coordinator to `Loans/Index`; `HomeController.Index` picks
the view by role (`MemberDashboard` / `AdminDashboard` / `CoordinatorDashboard`),
with the old `Index.cshtml` nav-card grid kept only as a fallback for an
unrecognised role.

**Nav label:** the sidebar entry stays **"Dashboard"** (not "Home"). It is the
established term across the app (`ViewData["Title"]`, the browser tab, the old
h1) and the page is a genuine dashboard for all three roles (KPI tiles + charts +
activity panels), which is what the word means; "Home" would read as a vaguer
portal.

---

## Phase 1 — Member dashboard

### Goals

- Give a member a personal, profile-style landing page: their observing activity,
  their sky, their loan situation, and what needs their attention.
- One centerpiece area with **two charts** (Chart.js via CDN): a monthly activity
  bar chart plus a "sky composition" donut.
- Degrade gracefully: each API call is independent; partial data still renders;
  a brand-new member sees friendly empty states, not errors.
- Keep quick navigation reachable (slim quick-actions row, not the old full grid).

### Data sources (all via `AuxiliarClienteHttp`, existing endpoints)

| Endpoint | Returns | Used for |
|----------|---------|----------|
| `GET api/v1/users/nights/{userId}` | `List<ObservationNightVM>` — `Date`, `CelestialObject{Id,Name,Type}`, `IsRequested` | activity chart, sky donut, distinct-objects, planning count, "next up" |
| `GET api/v1/loanrequests/user/{userId}` | `List<PendingLoanRequestVM>` — `Status`, `AiIndicator`, `ObservationNight`, `EquipmentSet` | pending count, AI-quality strip, "equipment secured?" flag, attention items |
| `GET api/v1/loantickets/user/{userId}` | `List<LoanTicketVM>` — `StartDate`, `EndDate`, `Status`, `IsOverdue` | active loans, overdue, returns-due-soon, attention items |
| `GET api/v1/celestialobjects/most-asked` *(optional)* | `List<CelestialObjectVM>` | small "club is watching" teaser linking to Celestial Ranking |

No new server data is needed. All aggregation is client-side C# in the controller.

### KPI tile row

| Tile | Derivation | Emphasis |
|------|-----------|----------|
| Nights logged | `nights.Count` | — |
| In planning | `nights.Count(n => !n.IsRequested)` | — |
| Objects observed | distinct non-null `CelestialObject.Id` in `nights` | — |
| Active loans | `tickets.Count(Status == "Loaned")` | — |
| Overdue | `tickets.Count(t => t.IsOverdue)` | `--color-danger` when > 0 |
| Pending requests | `requests.Count(Status == "Pending")` | `--color-warning` when > 0 |

### Centerpiece — charts (Chart.js 4.x from `cdn.jsdelivr.net`)

1. **Observation activity — bar chart (hero).**
   Group `nights` by month of parsed `Date`; produce the **last 6 months** as
   buckets, zero-filled, labelled `Sep`, `Oct`, … Count of nights per month.
2. **Sky composition — donut.**
   Group `nights` by `TypeGroup(CelestialObject.Type)` (reuse the
   `TypeGroup` / `GroupColor` mapping already in `Views/Reports/Ranking.cshtml` —
   lift it into a shared helper). Slices: Star / Cluster / Nebula / Galaxy /
   Planet / Small Body / Quasar / Other.

Chart.js integration rules:
- Load the script **only on this view** via `@section Scripts`, pinned version
  (`chart.js@4.4.1`), following the flatpickr-from-CDN precedent in `_Layout.cshtml`.
- Serialize chart data to a `<script type="application/json" id="dashboard-data">`
  block with `JsonSerializer.Serialize` + `Html.Raw`; `wwwroot/js/dashboard.js`
  reads it and builds the charts.
- Theme-aware: read palette at runtime with
  `getComputedStyle(document.documentElement).getPropertyValue('--color-…')`
  so charts follow the dark/light token set. Set `Chart.defaults.color` and
  grid `borderColor` from `--color-text-secondary` / `--color-border-soft`.
- Empty data → hide the canvas, show the card's empty state instead.

### Supporting panels

- **Next up** — `nights.Where(parsed Date >= today).OrderBy(Date).Take(5)`.
  Each row: date, object name, and an "equipment secured" pill (true when a
  request exists for that night with `Status` in `Pending`/`Approved`).
- **Needs attention** — a list built from:
  - each overdue ticket → "«Object» loan was due «date»" · danger · → `Loans/MyLoans`
  - `PendingRequests > 0` → "N request(s) waiting on a coordinator" · info · → `Loans/MyLoans`
  - planned future night with no request → "«Object» on «date» has no equipment yet"
    · warning · → `Loans/Create?nightId={id}`
  - empty list → cheeky all-clear ("All clear. The sky's yours tonight.")
- **AI planning strip** *(compact)* — `requests` grouped by `AiIndicator`
  (IDEAL / ADECUADO / NO_RECOMENDABLE) as three small counts, reusing the
  `AiColor` / `AiIcon` / `AiLabel` conventions from `Views/Loans/MyLoans.cshtml`.
- **Stargazer level** *(flavour)* — derived from `NightsLogged`:
  0 Cadet · 1–4 Observer · 5–14 Navigator · 15–29 Voyager · 30+ Cosmonaut.

### Copy / UX conventions (from project memory)

- All UI copy in **English**.
- Client-side messages: gentle, ambiguous, light astronomy voice, no em dashes.
- Icons: Font Awesome `fa-solid` only, never emoji.
- Views follow the example-project structural patterns adapted to our custom CSS
  (`card` / `card__body`, `tbl`, `page-head`, `detail-list`) — never Bootstrap.
- Pair every status colour with text/icon.

---

## Implementation checklist (Phase 1)

- [x] `Models/MemberDashboardVM.cs` — root VM + nested `MonthBucket`,
      `SkySlice`, `UpcomingNight`, `AttentionItem`, `AiCount`.
- [x] Shared helper `Models/CelestialTypeInfo.cs` — `Label` / `Group` /
      `GroupColorVar` / `ColorVarForType`, extracted from `Ranking.cshtml`
      (which now calls it, so the two stay in sync).
- [x] `HomeController` — injects `AuxiliarClienteHttp`; `Index()` branches on
      `UserRole`. For `Member`: reads `UserId` + `Token`, fetches the three
      lists (each in its own try/catch, sets `PartialLoad`), builds
      `MemberDashboardVM`, returns `View("MemberDashboard", vm)`. Other roles
      keep the existing `Index.cshtml` card grid.
- [x] `Views/Home/MemberDashboard.cshtml` — **full view** (not a partial, so
      `Index.cshtml` stays untouched for Admin/Coordinator): page-head +
      stargazer chip, KPI row, charts card (activity bar + sky doughnut +
      server-rendered legend), Needs attention, Next up, Planning-quality
      strip, quick-actions row, full-page empty state when there's no data.
- [x] `wwwroot/js/dashboard.js` — parses `#dashboard-data`, resolves the
      `--color-*` palette via `getComputedStyle`, builds the bar + doughnut
      charts, guards against empty datasets and a missing `Chart` global.
- [x] `wwwroot/css/site.css` — dashboard section appended: `.dash-stack`,
      `.kpi-row` / `.kpi` (+ `--danger` / `--warning`), `.dash-cols`,
      `.dash-card-title`, `.chart-wrap`, `.sky-legend`, `.attn`, `.upcoming`,
      `.ai-strip`, `.dash-actions`.
- [x] Chart.js 4.4.1 `<script>` (jsdelivr) + `dashboard.js` in the view's
      `@section Scripts`.
- [x] Verified in-browser as `bdiaz / Member@123` against the Render API:
      KPI tiles, both charts, stargazer chip, attention list, Next up chips,
      Planning-quality strip all render in the dark theme; no console errors.
- [x] Build: 0 errors (14 pre-existing nullability warnings unchanged).

### Revision pass (2026-09-01, after design review)

- **Primary CTAs promoted:** "Plan a night" (primary) + "Request equipment"
  (ghost) now sit in the page head, top-right — no longer buried at the bottom.
- **Stargazer meta** moved under the greeting and now names the next tier
  ("8 nights to Voyager"), via new `NextLevelName` on the VM.
- **KPI strip:** zero-value tiles dimmed (`.kpi--zero`); the Overdue tile only
  renders when `OverdueLoans > 0`; Pending requests is amber when > 0.
- **Hero chart reworked** from a sparse 6-month bar chart into a 12-month combo:
  bars = nights that month, filled line = **distinct objects discovered
  all-time** (a number that only climbs, so low-activity members still see
  progress). New `MonthBucket.Cumulative`; combo config in `dashboard.js`.
- **Layout rebalanced:** left rail = hero chart + sky-composition (doughnut and
  legend now side-by-side via `.sky-split`); right rail = Needs attention +
  Next up. No more wide-screen dead space.
- **Planning quality** only renders when the AI has actually rated something
  (`AiBreakdown.Any(a => a.Count > 0)`) — the 0/0/0 panel is gone by default;
  when shown it drops to a full-width row below the two rails.
- Re-verified in-browser as `bdiaz`: balanced two-column layout, combo chart,
  dimmed zeros, promoted CTAs, planning panel correctly hidden. No console errors.

### Empty-state pass (2026-09-01)

Every panel now shows a message + an action instead of a zero, an empty list, or
a hidden card:

- **KPI tiles:** a zero renders a short worded state (`kpi__value--empty`), e.g.
  "None borrowed", "None waiting", "Nothing queued" — never a lonely `0`. The
  Overdue tile still only appears when `> 0`.
- **Hero chart** (no nights): `empty--sm` block — "No timeline yet" + *Plan a night*.
- **Sky composition:** card always renders now; when there's nothing observed →
  "Your sky map is blank" + *Plan a night*.
- **Needs attention** (all clear): keeps the positive message, now with a
  *Plan your next night* ghost action.
- **Next up** (nothing upcoming): upgraded from a text stub to an `empty--sm`
  block — "Nothing on the horizon" + *Plan a night*.
- **Planning quality:** card always renders now. Three states — the AI strip when
  there are ratings; "No assessments yet" + *View my requests* when the member
  has requests but no ratings (bdiaz's case, verified); "Nothing to assess yet" +
  *Request equipment* when they've never requested.
- New VM signals `HasNights` / `HasRequests` drive the wording.
- New CSS: `.empty--sm`, `.empty__action`, `.kpi__value--empty`; `.kpi--zero`
  opacity softened 0.5 → 0.8 (no longer needed to carry the whole "quiet" signal).

### Follow-ups / polish (not blocking)

- New-member full empty state and `PartialLoad` banner are coded but not yet
  exercised against live data (bdiaz has a full history); the per-panel empty
  states other than Planning quality likewise weren't triggerable with bdiaz.
- "Nights logged" counts future-dated planned nights too (bdiaz: 7, of which 5
  are upcoming). Consider splitting "observed" vs "planned" if it reads oddly.

## Risks / considerations

- `AuxiliarClienteHttp` is synchronous (sync-over-async). The dashboard makes
  3–4 sequential calls; against the Render-hosted API a cold start can make the
  first load slow. Acceptable for the obligatorio; note a loading state if it
  feels bad in practice.
- `most-asked` is optional — skip silently on failure.
- Date strings come from the API unparsed; use `DateTime.TryParse` everywhere and
  drop rows that don't parse rather than throwing.

---

## Phase 2 — Admin dashboard

Status: **specced (2026-09-01), not started.** Decisions taken with the user:
emphasis = **balanced** (operational health + club engagement); charts = **combo
+ equipment utilization + role mix** (all three); **per-coordinator throughput**
panel = yes (full-width); **recent-activity** audit panel = yes.

Admins land on `Home/Index` today and fall through to the nav-card `Index.cshtml`.
Phase 2 adds an `else if (role == "Admin")` branch in `HomeController.Index()` →
`View("AdminDashboard", BuildAdminDashboard())`. Coordinator keeps the card grid
until Phase 3. `Index.cshtml` stays untouched (still the Coordinator landing).

### Data sources (all via `AuxiliarClienteHttp`, existing endpoints, each in its
own try/catch → `PartialLoad`)

| Endpoint | Returns | Used for |
|----------|---------|----------|
| `GET api/v1/users` | `List<UserVM>` — `role` | role-mix donut, club-size KPIs |
| `GET api/v1/equipment` | `List<EquipmentVM>` — `Type`, `Quantity`, `availableQuantity` | utilization bar, "units out" KPIs, "fully out" attention |
| `GET api/v1/loantickets` | `List<LoanTicketVM>` — `Status`, `IsOverdue`, `StartDate`, `EndDate`, `CoordinatorName`, `LoanRequest` | on-loan-now KPI, overdue KPI + attention, hero-chart cumulative line, coordinator throughput |
| `GET api/v1/loanrequests/pending` | `List<PendingLoanRequestVM>` — `Status`, `AiIndicator`, `ObservationNight`, `RequestingUser` | queue-depth KPI, request-queue panel, AI-risk breakdown, stale-queue attention |
| `GET api/v1/observationnights` | `List<ObservationNightVM>` — `Date`, `CelestialObject`, `RequestingUserId` | hero-chart monthly bars, club activity trend |
| `GET api/v1/logs` | `List<LogEventDto>` — `Operation`, `TableName`, `Email`, `IsError`, `Date` | recent-activity panel, "errors this week" KPI, log-error attention items |

Six sequential calls. Phase 1's three were "acceptable for the obligatorio" but a
cold Render start makes this noticeably slower — see Risks. All six endpoints are
already exercised elsewhere in the client (verified): `users`→`UsersController`,
`equipment`→everywhere, `loantickets`(all)→`Reports/LoanAudit` & `Loans/Index`,
`loanrequests/pending`→`Loans/Index`, `observationnights`→`ObservationNights/Index`
(non-Member branch), `logs`→`Reports/Logs`.

No new server data. All aggregation is client-side C# in the controller.

### KPI tile row (reuses `.kpi-row` / `.kpi`, worded zero states like Phase 1)

| Tile | Derivation | Emphasis |
|------|-----------|----------|
| Members | `users.Count(u => u.role == "Member")` | — |
| Coordinators | `users.Count(u => u.role == "Coordinator")` | — |
| Equipment available | `Σ availableQuantity` / `Σ Quantity` → "34 of 50" | — |
| On loan now | `tickets.Count(Status == "Loaned")` | — |
| Overdue | `tickets.Count(t => t.IsOverdue)` | `.kpi--danger`; **only rendered when > 0** (Phase 1 rule) |
| Pending requests | `pending.Count` | `.kpi--warning` when > 0 |
| Errors this week | `logs.Count(l => l.IsError && parsed Date >= today.AddDays(-7))` | `.kpi--danger` when > 0; worded "All quiet" at 0 |

### Centerpiece — 3 charts (Chart.js 4.4.1 from jsdelivr, same integration rules
as Phase 1: pinned version in `@section Scripts`, JSON in a
`<script type="application/json" id="admin-dashboard-data">`, palette resolved at
runtime via `getComputedStyle`, empty dataset → server-rendered empty state)

1. **Club activity over time — 12-month combo (hero).** Identical config to the
   Member hero. Bars = observation nights logged club-wide that month
   (zero-filled, `MMM` / `MMM yy` labels). Filled line = **cumulative observation
   nights all-time** up to each month-end (only climbs — the club logbook
   growing). Reuses `MonthBucket { Label, Count, Cumulative }`.
2. **Equipment utilization — horizontal stacked bar.** One row per `Type`
   (Telescope / Mount / Camera / Eyepiece). Stack = "On loan" (`Quantity −
   availableQuantity`, `--color-accent`) + "Available" (`availableQuantity`,
   `--color-success`). `indexAxis: 'y'`, both scales `stacked: true`.
3. **Role mix — doughnut** + server-rendered legend, laid out with the existing
   `.sky-split` (doughnut left, legend right). Slices: Member / Coordinator /
   Admin. Colors: `--color-accent` / `--color-info` / `--color-warning`
   (matches the sidebar role-chip colours).

Charts 1 + 2 sit in the left rail; chart 3 pairs with it in the right rail head,
mirroring Phase 1's "hero + sky composition" left rail.

### Supporting panels

- **Needs attention** — reuses `.attn` / `AttentionItem { Icon, Severity, Text, Url }`:
  - each overdue ticket → "«member»'s «object» loan was due «date»" · danger · → `Reports/LoanAudit`
  - `pending` older than 3 days → "«N» request(s) have been waiting 3+ days" · warning · → `Loans/Index`
  - each equipment `Type` with `Σ availableQuantity == 0` → "Every «type» is out on loan" · warning · → `Equipment/Index`
  - each `logs` error in the last 7 days → "«operation» on «table» hit an error «date»" · danger · → `Reports/Logs` (cap 2)
  - empty → cheeky all-clear ("All systems nominal. The observatory hums.")
  - `.Take(6)` overall, same as Phase 1.
- **Request queue** — `pending.OrderBy(parsed ObservationNight.Date).Take(5)`.
  Each row (reuses `.upcoming` list): requester username, night date, target
  object, and an AI chip (`IDEAL`/`ADECUADO`/`NO_RECOMENDABLE` → the
  `AiColor`/`AiIcon`/`AiLabel` maps already in the Member view). Row links to
  `Loans/Index`. Empty → "The queue is clear" positive state.
- **AI risk of the current queue** *(compact `.ai-strip`)* — `pending` grouped by
  `AiIndicator` as three counts. Only rendered when
  `AiBreakdown.Any(a => a.Count > 0)` (Phase 1 rule); else folded away (the
  queue panel already carries per-row AI chips).
- **Coordinator throughput** *(full-width, below the rails)* — all `tickets`
  grouped by `CoordinatorName`: tickets issued **this month** (`StartDate` in the
  current month) and **all-time**. Rendered as a `.tbl` table sorted by
  this-month desc, with a thin proportional bar in the count cell
  (`.coord-meter`, one small new CSS rule). Empty → "No approvals logged yet."
- **Recent activity** *(full-width or right rail foot)* — `logs.OrderByDescending
  (parsed Date).Take(6)`. Each row (reuses `.upcoming` list): an icon by
  `Operation` (CREATE `fa-plus`, UPDATE `fa-pen`, DELETE `fa-trash`, else
  `fa-circle-info`) tinted `--color-danger` when `IsError`, the message / "«op»
  «table»", the actor email, and a relative time. Links to `Reports/Logs`. Empty
  → "Nothing in the log yet."

### Page head

- Title "Observatory overview", sub "A club-wide read on people, gear and loans."
- Meta chip under the greeting: "«M» members · «C» coordinators" (new
  `ClubSizeNote` on the VM), matching Phase 1's stargazer-chip slot.
- Top-right actions: **Add member** (`.btn--primary` → `Users/Create`) +
  **Add equipment** (`.btn--ghost` → `Equipment/Create`).

### Explore row (`.dash-actions`, ghost buttons)

Members · Equipment · Event Log · Members by Telescope · Loan Audit · Celestial Ranking.

### Empty / partial states (Phase 1 conventions carried over)

- `!HasAnyData` (seed-fresh install) → single full-page `empty` card, "The
  observatory is quiet" + Add member / Add equipment. Unlikely but handled.
- `PartialLoad` → the amber `.alert.alert--error` banner from Phase 1, same copy.
- Every panel renders a worded state + action instead of a bare zero / empty
  list, per the Phase 1 empty-state pass.

### Copy / UX conventions

Same as Phase 1: English only; gentle, non-technical, lightly astronomy-cheeky
for the soft/empty/all-clear messages (operational panel titles stay plain); no
em dashes in client-facing strings; Font Awesome `fa-solid` only; `card` /
`card__body` / `tbl` / `page-head` structural patterns with our CSS, never
Bootstrap; every status colour paired with text + icon.

---

## Implementation checklist (Phase 2)

**Built & verified 2026-09-01.** All items done; deviations from the spec noted inline.

- [x] `Models/AdminDashboardVM.cs` — root VM + nested `UtilizationBar
      { Type, OnLoan, Available }`, `RoleSlice { Role, Count, ColorVar }`,
      `QueueItem { RequestId, Requester, NightDate, ObjectName, AiIndicator, Url }`,
      `CoordinatorStat { Name, ThisMonth, AllTime }`,
      `AuditEntry { Icon, Text, Actor, When, IsError, Url }`. Reuses
      `MonthBucket`, `AttentionItem`, `AiCount` from `MemberDashboardVM.cs`.
- [x] `HomeController` — add `else if (role == "Admin")` branch +
      `BuildAdminDashboard()`: six fetches each in their own try/catch setting a
      `*Ok` flag → `PartialLoad = !(all ok)`; `HasAnyData` = any list non-empty;
      client-side aggregation for every KPI / chart / panel above; `DateTime.
      TryParse` everywhere, drop unparseable rows. Mirror the structure of
      `BuildMemberDashboard()`.
- [x] `Views/Home/AdminDashboard.cshtml` — full view (not a partial). Helper
      `@{ }` maps for AI + operation icon/colour at the top (inline `@if` /
      tuple-array `@foreach` only — **no local functions as the sole body of an
      `else if`**, RZ1010, per the Phase 1 devlog). Layout: page-head + club-size
      chip + CTAs, KPI strip, `.dash-cols` (left rail = hero combo + equipment
      utilization; right rail = role-mix `.sky-split` + Needs attention + Request
      queue), then full-width Coordinator throughput, then Recent activity, then
      `.dash-actions`. `#admin-dashboard-data` JSON block. `@section Scripts` =
      Chart.js 4.4.1 (jsdelivr) + `admin-dashboard.js`.
- [x] `wwwroot/js/admin-dashboard.js` — self-contained IIFE reading
      `#admin-dashboard-data`; carries its own copy of the `tok()` / `withAlpha()`
      palette helpers from `dashboard.js` (≈30 lines; extracting a shared
      `dashboard-charts-common.js` is a possible follow-up, not this pass).
      Builds the combo (`#chart-activity`), the stacked horizontal bar
      (`#chart-utilization`), and the role doughnut (`#chart-roles`); each guards
      on canvas presence + non-empty data + a missing `Chart` global.
- [x] `wwwroot/css/site.css` — small "Admin dashboard" subsection appended:
      `.chart-wrap--util { height: 220px; }` and `.coord-meter` / `.coord-meter__bar`
      (thin proportional bar in the throughput table). Everything else reuses
      Phase 1 classes.
- [x] Build: 0 errors, 14 pre-existing warnings (unchanged). Hit the same RZ1010
      as Phase 1 — an inner `@{ }` as the first thing inside an `@if { }` inside
      markup; fixed by hoisting the `coordMax` calc into the view's top `@{ }`.
- [~] Verify against the Render API as `jperez / Admin@123`
      (`--ApiBaseUrl=https://stellarmind-api.onrender.com/`): the browser
      extension was unavailable this session, so verification was **HTTP-level,
      not visual** — logged in via `curl`, fetched `/Home/Index`, HTTP 200,
      18.6 KB, no server-side exception / RZ error. Confirmed from the rendered
      HTML: all six fetches succeeded (no `PartialLoad` banner); KPIs populated
      (6 members, 7 coordinators, 123 of 330 units free, 10 pending, "None
      active" loans, "All quiet" errors); `#admin-dashboard-data` JSON well-formed
      with real 12-month activity (Jun–Sep: 2/2/3/2, cumulative to 9), 4-type
      utilization, 3-slice role mix; request queue + recent activity lists
      populated (5 + 6 rows); needs-attention "all systems nominal" all-clear;
      coordinator-throughput "No approvals logged yet" empty state (see below).
      **Not visually confirmed:** the three Chart.js canvases actually paint, and
      the browser console is clean. `admin-dashboard.js` is a near-verbatim copy
      of the proven `dashboard.js`, so risk is low, but a visual pass is still
      owed.

### Deviations from the spec (applied during the build)

- **KPI/chart language clash fixed.** "On loan now" (counts `loantickets` with
  `Status == "Loaned"`, = 0 in seed) sat next to an "Equipment utilization" chart
  that derives usage from stock (`Quantity − availableQuantity`, ~50/type) —
  contradictory at a glance. Renamed the KPI to **"Loans in progress"** (zero →
  "None active"), the chart's on-loan stack segment to **"In use"**, its subtitle
  to "Units in use versus available, by type", and the fully-out attention line
  to "No «type» is free right now. Every unit is in use."
- **AI "Queue risk" panel** renders only when `pending` carries `AiIndicator`
  values; seed pending requests for `jperez` have none, so it folds away (same as
  the Member "Planning quality" panel with `bdiaz`). Not visually exercised.
- **Coordinator throughput** shows its "No approvals logged yet" empty state:
  `api/v1/loantickets` returned nothing usable for this admin (also why "Loans in
  progress" is 0). The empty state is correct behavior; the populated `.tbl` +
  `.coord-meter` path is coded but **untriggered** with current seed data.

### Risks / considerations (Phase 2)

- **Six sequential sync calls** on a cold Render start ≈ 6× the first-byte
  latency. Options: (a) accept it for the obligatorio, matching Phase 1's stance;
  (b) wrap the six in `Task.Run(...)` + `Task.WhenAll` for a parallel fetch —
  real win, ~1 call of latency, ~15 lines. **Recommendation:** build sequential
  first for parity with Phase 1, add the parallel fetch only if the cold load
  feels bad in practice.
- `api/v1/logs` requires Admin/Coordinator — fine here (Admin branch only), but
  keep its fetch in its own try/catch so a 403 on a misconfigured token just
  drops the Recent-activity panel and the errors KPI, not the page.
- `CoordinatorName` on tickets can be blank/duplicated across coordinators with
  the same name — group by name is good enough for the obligatorio; note it.
- Same date-parsing rule as Phase 1: `DateTime.TryParse` everywhere, drop rows
  that don't parse rather than throwing.

## Phase 3 — Coordinator dashboard

Status: **built & verified 2026-09-02.** "Your desk": the shared pending queue
plus the loans this coordinator has issued. Mirrors the Member dashboard's
structure (page-head + greeting, KPI strip, `.dash-bento` grid, `@section
Scripts` with Chart.js 4.4.1).

### Redirect

`UsersController.Login` (POST) now `return RedirectToAction("Index", "Home")` for
every role — the Coordinator `? Loans/Index` branch is gone. `HomeController.
Index` gains `if (role == "Coordinator") return View("CoordinatorDashboard",
BuildCoordinatorDashboard())`.

### Data sources (each in its own try/catch → `PartialLoad`)

| Endpoint | Returns | Used for |
|----------|---------|----------|
| `GET api/v1/loanrequests/pending` | `List<PendingLoanRequestVM>` | queue-depth KPI, AI-flag KPI, queue-risk doughnut, request-queue panel, attention |
| `GET api/v1/loantickets` | `List<LoanTicketVM>` | filtered client-side to `CoordinatorId == me` → on-loan / due-this-week / overdue / issued-all-time KPIs, approvals combo chart, returns-due panel, attention |

**Endpoint note:** `loantickets/coordinator/{id}` is **Admin-only** — it returns
**403** for the coordinator themselves (confirmed against the Render API). So the
dashboard reads the full `loantickets` list (Coordinator-accessible; `204 No
Content` when empty, which deserialises to an empty list without tripping
`PartialLoad`) and keeps `t.CoordinatorId == coordinatorId`. `ApprovedLoans`
still calls the coordinator-scoped route and likely hits the same 403 — a
separate pre-existing bug, not touched here.

### KPI strip (worded zero states, Phase 1 rules)

| Tile | Derivation | Emphasis |
|------|-----------|----------|
| Pending requests | `pending.Count` | `.kpi--warning` when > 0; zero → "Queue clear" |
| Flagged by AI | `pending.Count(r => r.AiIndicator == "NO_RECOMENDABLE")` | `.kpi--danger` when > 0; zero → "None flagged" |
| On loan now | `mine.Count(Status == "Loaned")` | zero → "None active" |
| Due back this week | `mine.Count(Loaned && EndDate in [today, today+7])` | `.kpi--warning` when > 0; zero → "Nothing due" |
| Tickets issued | `mine.Count` | zero → "None yet" |
| Overdue | `mine.Count(IsOverdue)` | `.kpi--danger`; **only rendered when > 0** |

### Charts (Chart.js 4.4.1, `#coordinator-dashboard-data`, `coordinator-dashboard.js`)

1. **Approvals over time — 12-month combo (hero).** Bars = tickets this
   coordinator issued that month (`StartDate`); line = cumulative all-time. Same
   config as the Member/Admin heroes. Empty (`chartHasData` false) → `empty--sm`
   "No approvals yet" + *Open the queue*.
2. **Queue risk — doughnut** + `.sky-split` server legend. `pending` grouped by
   `AiIndicator` (IDEAL/ADECUADO/NO_RECOMENDABLE → success/warning/danger,
   `UNRATED` slice added only when > 0). Shown **only when at least one request
   is actually rated** — an all-`UNRATED` queue would just be a flat grey ring,
   so that case falls to an `empty--sm` "No AI ratings yet" + *Open the queue*
   (distinct from the queue-empty "Nothing to weigh up").

### Panels

- **Needs attention** (`bento--narrow`, `.attn`) — overdue tickets of mine
  (danger) · AI-flagged pending count (warning) · pending nights ≤ 3 days out
  still pending (warning) · my loans due back ≤ 2 days (info). `.Take(6)`. Empty
  → "Your desk is clear".
- **Returns due soon** (`bento--narrow`, `.upcoming` list) — my `Loaned` tickets
  by `EndDate` asc, take 5: due date, object · member, an "Overdue" danger chip.
  Empty → "Nothing to chase".
- **Request queue** (`bento--full`, `.upcoming` list) — `pending` by target-night
  date, take 6: date, object · requester, AI chip when rated. Links to
  `Loans/Index`. Empty → "The queue is clear".

### Bento layout (the 6-col `.dash-bento` grid from the 2026-09-02 pass)

Row 1: Approvals `bento--wide` + Needs attention `bento--narrow` ·
Row 2: Queue risk `bento--wide` + Returns due soon `bento--narrow` ·
Row 3: Request queue `bento--full`. Then `.dash-actions` (Loan Requests ·
Equipment · Observation Nights · Celestial Ranking).

### VM

`Models/CoordinatorDashboardVM.cs` — root VM + `ReturnDue { Member, ObjectName,
DueDate, IsOverdue, Url }`. Reuses `MonthBucket` / `AttentionItem` / `AiCount`
from `MemberDashboardVM` and `QueueItem` from `AdminDashboardVM`. No new CSS —
everything reuses Phase 1/2 classes plus the bento grid.

### Verification (in-browser, Render API, `vcastro` / Coordinator)

Login redirects to `/Home/Index` → `CoordinatorDashboard`. KPIs populated
(10 pending / worded zeros elsewhere — vcastro has issued no tickets and none
exist in seed, `GET /loantickets` → 204). No `PartialLoad` banner after the
endpoint swap. Bento rows share a baseline; "No approvals yet" and "No AI ratings
yet" empty states centre in their stretched cards; Request queue full-width with
6 rows; console clean. Build 0 errors / 14 pre-existing warnings.

**Not exercised** (no seed data): the approvals combo actually painting, the
queue-risk doughnut with rated requests, the returns-due list, and any
attention item.
