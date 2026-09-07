# Member Dashboard — Dev Session Log

Companion to [`member-dashboard-plan.md`](./member-dashboard-plan.md). One entry per
working session on the Home dashboard.

---

## 2026-09-02 (session 4) — Data-source validation + screenshots + reference doc

Validated every dashboard component's source / field / calculation against the
live Render API for all three roles, then captured screenshots and wrote a
reference doc.

### Validation

For each role, curl'd every endpoint the Build method calls and cross-checked
the response shape, field names (camelCase, case-insensitive deser) and the
computed values against what renders:

- **Member (`bdiaz`/15):** 7 nights → 7 logged / 2 in planning / 7 distinct
  objects; sky groups Star 3 / Galaxy 2 / Cluster 1 / Planet 1 (types
  1,1,19 / 11,11 / 5 / 13); 5 requests (2A/2P/1R, all `aiIndicator` null) → 2
  pending, "No assessments yet"; `loantickets/user` = 204 → None borrowed;
  Stargazer Navigator + "8 to Voyager". All exact.
- **Admin (`jperez`):** 20 users → 6/7/7; 41 equipment → 330 total / 123 free,
  utilisation 48·30 / 58·37 / 49·28 / 52·28; `loantickets` = 204 → empty
  throughput, no Overdue; 10 pending (null AI) → queue lists 5 soonest, risk
  strip hidden; 29 nights (from Jun 2026) → bars 2/2/3/2, cumulative 9; 13 logs
  (11 GET + 2 POST, 1 old error) → Errors this week "All quiet", Recent activity
  = the 2 POST rows only (reads filtered).
- **Coordinator (`vcastro`/8):** 10 pending (null AI) → 10 pending, None flagged,
  "No AI ratings yet"; `loantickets` = 204, none `coordinatorId == 8` → all
  ticket KPIs empty, "Your desk is clear".

Every rendered figure matched the API to the digit. Cross-checked all `Model.*`
refs in the three views against the VM classes — no phantom bindings.

### One fix from the pass

Admin "Needs attention" stale-pending item filtered `night <= today+2` with **no
lower bound**, so a past-dated pending request would match with future-tense copy
("is almost here"). Added `night >= today` to match the Coordinator panel's
guard. Not currently triggered (all seed pending nights are Dec 2026+).

### Screenshots + doc

- `docs/screenshots/` — `admin-0{1,2,3}`, `coordinator-0{1,2}`, `member-0{1,2}`.jpg
- `docs/dashboards.md` — new reference: per-panel source endpoint, VM field,
  calculation, zero state, and the validation results above.

Build 0 errors. Not committed.

### Files touched this session

```
A  docs/dashboards.md
A  docs/screenshots/*.jpg  (7 files)
M  docs/member-dashboard-devlog.md
M  docs/member-dashboard-plan.md
M  Obligatorio-N3D-342742-360021-Client/Controllers/HomeController.cs
```

---

## 2026-09-02 (session 3) — Final design pass (all three dashboards)

Visual review of all three role dashboards in-browser, then targeted fixes for
design and content priority.

### Fixes

1. **`.sky-split` doughnut clipped the legend** (Admin "Who's in the club" at
   `bento--half`; intermittent on Member "Your sky composition"). Cause: the
   doughnut lived in a `minmax(0,1fr)` grid track with nothing capping it, so a
   `maintainAspectRatio:false` Chart.js canvas kept growing into the flex room
   and pushed the `max-content` legend past the `overflow:hidden` card edge.
   Fix: `site.css` — `.sky-split .chart-wrap { max-width: 240px; margin-inline:
   auto; }` (canvas can't exceed it), `.chart-wrap--sky` 230→220px.

2. **"Recent activity" was 82% noise** (Admin). The audit log records reads too —
   9 of 11 seed rows were `GET Equipments "…retrieved successfully."`. Added
   `IsMutation()` and filter the panel to `l.IsError || IsMutation(l.Operation)`
   (CREATE/UPDATE/DELETE/POST/PUT/PATCH). Six identical rows → the two real
   events (a user create + its validation failure). `OpIcon` also maps
   POST→`fa-plus`, PUT→`fa-pen`. The full stream stays one click away via "Open
   the event log". Errors-this-week KPI and attention log-items already filtered
   on `IsError`, untouched.

3. **Coordinator dashboard led with empty charts, not the queue.** A coordinator
   lands to work the pending queue, but it was `bento--full` at row 3 under two
   rows of (seed-empty) charts. Reordered:
   - Row 1: Request queue `bento--wide` + Needs attention `bento--narrow`
   - Row 2: Returns due soon `bento--half` + Queue risk `bento--half`
   - Row 3: Approvals over time `bento--full`
   Actionable list first; the history chart sinks to the foot.

4. **Explore-row labels were sentence case**, out of step with the Title Case
   sidebar nav and the rest of the chrome. Admin + Member rows fixed ("Event
   Log", "Members by Telescope", "Loan Audit", "Observation Plans", "My Loans",
   "Loan History", "Celestial Ranking"). Coordinator was already Title Case.

5. **"Jan 26" axis label** read like a day-of-month. `HomeController` (3 spots)
   → `m.ToString("MMM yyyy")` for January → "Jan 2026".

### Verified in-browser (Render API)

Admin: "Who's in the club" doughnut + full legend, no clip; Recent activity shows
the create + error only; Jan 2026 label; Title Case explore row. Coordinator:
Request queue leads, halves aligned below. Member: sky doughnut capped, legend
visible; Title Case explore row. Build: 0 errors.

Not committed. (Browser extension went unresponsive near the end; the final
`max-width` tweak to Admin's doughnut is strictly safer than the fixed-width
version already verified, so not re-shot.)

### Files touched this session

```
M  docs/member-dashboard-devlog.md
M  Obligatorio-N3D-342742-360021-Client/Controllers/HomeController.cs
M  Obligatorio-N3D-342742-360021-Client/Views/Home/AdminDashboard.cshtml
M  Obligatorio-N3D-342742-360021-Client/Views/Home/CoordinatorDashboard.cshtml
M  Obligatorio-N3D-342742-360021-Client/Views/Home/MemberDashboard.cshtml
M  Obligatorio-N3D-342742-360021-Client/wwwroot/css/site.css
```

---

## 2026-09-02 (session 2) — Phase 3 (Coordinator): build + redirect fix

Built the last missing dashboard and made every role land on `Home/Index`.

### Redirect

`UsersController.Login` (POST) dropped its `user.UserRole == "Coordinator" ?
RedirectToAction("Index", "Loans")` branch — now always `RedirectToAction("Index",
"Home")`. `HomeController.Index` gained `if (role == "Coordinator") return
View("CoordinatorDashboard", BuildCoordinatorDashboard())`; the bare `return
View()` (old `Index.cshtml` card grid) is now only a fallback for an
unrecognised role.

### Nav label

Left as **"Dashboard"** (user asked to evaluate "Home"). It is the established
term everywhere (`ViewData["Title"]`, browser tab, old h1) and the page is a
real dashboard for all three roles. "Home" would be a vaguer downgrade.

### Build

New files:
- `Models/CoordinatorDashboardVM.cs` — root VM + `ReturnDue`. Reuses
  `MonthBucket` / `AttentionItem` / `AiCount` (from `MemberDashboardVM`) and
  `QueueItem` (from `AdminDashboardVM`).
- `Views/Home/CoordinatorDashboard.cshtml` — page-head + greeting + queue-note
  chip + "Review queue" / "Approved loans" CTAs; KPI strip (5 + conditional
  Overdue); `.dash-bento` (Approvals `wide` + Needs attention `narrow` · Queue
  risk `wide` + Returns due soon `narrow` · Request queue `full`); `.dash-actions`.
- `wwwroot/js/coordinator-dashboard.js` — self-contained IIFE, own
  `tok()`/`withAlpha()`, builds `#chart-approvals` (combo) and
  `#chart-queue-risk` (doughnut). Third near-verbatim copy of `dashboard.js`'s
  chart helpers — the extract-to-common follow-up is now overdue.

Changed:
- `Controllers/HomeController.cs` — `BuildCoordinatorDashboard()`: two fetches
  (`loanrequests/pending`, `loantickets`), each try/catch → `PartialLoad`;
  client-side aggregation; `DateTime.TryParse` throughout.
- `Controllers/UsersController.cs` — the redirect above.

No new CSS.

### Endpoint gotcha (cost one debug cycle)

First build used `GET api/v1/loantickets/coordinator/{id}` per the Phase 3 notes.
In-browser it lit the `PartialLoad` banner every load. `curl` against the Render
API as `vcastro`: that route returns **HTTP 403** for the coordinator themselves
(it's Admin-only — the API-contract memory even tagged it "(Admin)"). Switched to
`GET api/v1/loantickets` (Coordinator-accessible; returns **204 No Content** when
empty → `EnviarYDeserializar` gives `default`/`null` → `?? new()` → empty list,
no `PartialLoad`) and filter `t.CoordinatorId == coordinatorId` client-side, the
same shape `LoansController.Index` already uses. Banner gone.

Note: `LoansController.ApprovedLoans` still calls the coordinator-scoped route
and almost certainly hits the same 403 for a coordinator — pre-existing, left
alone.

### Design tweak in-pass

Seed pending requests carry no `aiIndicator`, so the queue-risk doughnut rendered
as one flat grey "Unrated" ring. Changed `showRisk` to require at least one
*rated* request; an all-unrated queue now shows an `empty--sm` "No AI ratings
yet" + *Open the queue* (kept separate from the queue-empty "Nothing to weigh
up"). The `UNRATED` slice still shows in the doughnut for the mixed case.

### Verification (in-browser, Render API, `vcastro` / Coordinator)

Login → `/Home/Index` → `CoordinatorDashboard` (redirect confirmed). No
`PartialLoad` after the endpoint swap. KPIs: 10 pending (amber), worded zeros
elsewhere (no tickets exist in seed — `GET /loantickets` → 204). Bento rows share
a baseline; the two empty states centre in their stretched cards; Request queue
full-width, 6 rows. Console clean. Build: **0 errors, 14 pre-existing warnings.**

**Not exercised** (no seed data): the approvals combo painting, the queue-risk
doughnut with rated data, returns-due rows, any attention item. Chart JS is a
near-verbatim copy of the proven Member/Admin heroes.

### Files touched this session

```
M  docs/member-dashboard-plan.md
M  docs/member-dashboard-devlog.md
A  Obligatorio-N3D-342742-360021-Client/Models/CoordinatorDashboardVM.cs
A  Obligatorio-N3D-342742-360021-Client/Views/Home/CoordinatorDashboard.cshtml
A  Obligatorio-N3D-342742-360021-Client/wwwroot/js/coordinator-dashboard.js
M  Obligatorio-N3D-342742-360021-Client/Controllers/HomeController.cs
M  Obligatorio-N3D-342742-360021-Client/Controllers/UsersController.cs
```

Not committed.

---

## 2026-09-02 (session 1) — Bento grid pass (both dashboards)

The two-`.dash-rail` layout was the problem: `.dash-cols` put a left flex stack
(tall chart cards) next to a right flex stack (short list cards), so card edges
never lined up and the shorter rail left a dead band down the middle — worst on
Admin, where the left rail had 2 tall panels against the right rail's 3 short ones.

Replaced it with a real bento grid. `site.css`: dropped `.dash-cols`,
`.dash-cols--even`, `.dash-rail`; added `.dash-bento` (6-col grid,
`align-items: stretch`, cards are direct children) + span helpers
`.bento--wide` (4) / `.bento--half` (3) / `.bento--narrow` (2) / `.bento--full`
(1/-1). Cards are flex columns so a lone trailing `.empty--sm` gets
`margin-block: auto` and centres in the height its row-mate forces. Collapses to
one column at ≤900px (unchanged breakpoint).

Both views: panels flattened out of the rails into `.dash-bento` and reordered so
each row sums to 6.
- **Admin:** Club activity `wide` + Needs attention `narrow` · Equipment
  utilization `half` + Who's in the club `half` · Request queue `full`.
- **Member:** Your sky over time `wide` + Needs attention `narrow` · Sky
  composition `wide` + Next up `narrow`.

The full-width cards below the bento (Queue risk / Coordinator throughput /
Recent activity / Planning quality / `.dash-actions`) were already stacked and
were left alone.

Verified in-browser against the Render API (`jperez`/Admin and `bdiaz`/Member,
`--launch-profile http`): every bento row now shares a baseline, no mid-layout
void on either dashboard, three Chart.js canvases paint, console clean. Build:
0 errors, 14 pre-existing warnings. Not committed.

---

## 2026-09-01 (session 2) — Phase 2 (Admin): design + build

Picked up from Phase 1. Read the plan + devlog + Phase 1 implementation, catalogued
the Admin-reachable API surface, took four design decisions with the user, wrote
the full Phase 2 spec into `member-dashboard-plan.md`, then built and HTTP-verified
the Admin dashboard in one pass.

### Decisions (user)

| Question | Choice |
|---|---|
| Dashboard emphasis | **Balanced** — operational health + club engagement |
| Charts | **All three** — club activity combo + equipment utilization + role mix |
| Coordinator throughput panel | **Yes**, full-width |
| Recent-activity audit panel | **Yes** |

### Build

New files:
- `Models/AdminDashboardVM.cs` — root VM + `UtilizationBar`, `RoleSlice`,
  `QueueItem`, `CoordinatorStat`, `AuditEntry`. Reuses `MonthBucket`,
  `AttentionItem`, `AiCount` from `MemberDashboardVM.cs`.
- `Views/Home/AdminDashboard.cshtml`
- `wwwroot/js/admin-dashboard.js` — self-contained; own copy of `dashboard.js`'s
  `tok()` / `withAlpha()` helpers (≈30 lines dup, flagged as a possible
  extract-to-common follow-up).

Changed:
- `Controllers/HomeController.cs` — `Index()` gains `if (role == "Admin") return
  View("AdminDashboard", BuildAdminDashboard())`. `BuildAdminDashboard()` fires
  six independent fetches (`users`, `equipment`, `loantickets`,
  `loanrequests/pending`, `observationnights`, `logs`), each in its own try/catch
  → `PartialLoad`; all aggregation client-side, `DateTime.TryParse` everywhere.
- `wwwroot/css/site.css` — appended "Admin dashboard" subsection:
  `.chart-wrap--util`, `.coord-meter`, `.coord-meter__bar`. Everything else is
  Phase 1 classes.

Layout: page-head (club-size chip + Add member / Add equipment CTAs) → KPI strip
(6 tiles, Overdue only when > 0, worded zeros) → `.dash-cols` (left rail = activity
combo + utilization bar; right rail = role-mix `.sky-split` + Needs attention +
Request queue) → Queue risk `.ai-strip` (conditional) → Coordinator throughput
`.tbl` → Recent activity `.upcoming-list` → `.dash-actions`.

**Razor gotcha (same family as Phase 1's RZ1010):** an `@{ }` block as the first
child of an `@if { }` that sits in markup context throws
`RZ1010 Unexpected "{" after "@"`. Fixed by hoisting the `coordMax` computation
into the view's top-of-file `@{ }` block.

### Design-review deviation applied same pass

"On loan now" KPI (loan-ticket count, 0 in seed) clashed with the utilization
chart's stock-derived "on loan" (~50/type). Realigned language: KPI →
**"Loans in progress"**, chart segment → **"In use"**, subtitle + attention copy
to match. Two lenses, no longer contradictory.

### Verification (HTTP-level only — no browser extension this session)

`curl` login as `jperez / Admin@123` against `stellarmind-api.onrender.com`, then
`GET /Home/Index`: **HTTP 200, 18.6 KB, no server exception, no RZ error.** From
the HTML: all six sources loaded (no `PartialLoad` banner); KPIs populated
(6 members / 7 coordinators / 123 of 330 units free / 10 pending / "None active" /
"All quiet"); `#admin-dashboard-data` JSON well-formed — 12-month activity
(Jun–Sep 2/2/3/2, cumulative→9), 4-type utilization, 3-slice role mix; request
queue (5) + recent activity (6) populated; needs-attention all-clear;
coordinator-throughput empty state ("No approvals logged yet" — `loantickets`
came back empty for this admin, also why "Loans in progress" is 0).

Build: **0 errors, 14 pre-existing warnings** (unchanged).

### Not done / follow-ups

- **Visual pass owed:** confirm the three Chart.js canvases paint and the console
  is clean, ideally against an account with loan-ticket history so the
  coordinator-throughput table and the "Queue risk" AI strip actually render.
- The `tok()` / `withAlpha()` duplication between `dashboard.js` and
  `admin-dashboard.js` could move to a shared `dashboard-charts-common.js`.
- Parallel fetch (`Task.WhenAll`) for the six calls — deferred, matching Phase 1;
  revisit only if the cold Render load feels bad.
- Phase 3 (Coordinator) still needs its post-login redirect changed from
  `Loans/Index` to `Home/Index`.

### Files touched this session

```
M  docs/member-dashboard-plan.md
M  docs/member-dashboard-devlog.md
A  Obligatorio-N3D-342742-360021-Client/Models/AdminDashboardVM.cs
A  Obligatorio-N3D-342742-360021-Client/Views/Home/AdminDashboard.cshtml
A  Obligatorio-N3D-342742-360021-Client/wwwroot/js/admin-dashboard.js
M  Obligatorio-N3D-342742-360021-Client/Controllers/HomeController.cs
M  Obligatorio-N3D-342742-360021-Client/wwwroot/css/site.css
```

Not committed.

---

## 2026-09-01 — Phase 1 (Member): design, build, review, revise

Single session. Went from "no dashboard exists" to a built, browser-verified
Member dashboard, then two rounds of revision off a design critique.

### 0. Context gathering

Read the memory bank + the codebase: `HomeController` was a one-line `View()`;
`Home/Index.cshtml` was only a role-gated grid of nav cards. Members and Admins
land on `Home/Index`; Coordinators are redirected to `Loans/Index` at login.
Catalogued the data reachable per role via `AuxiliarClienteHttp` (sync HTTP helper).

### 1. Planning + decisions (recorded in `member-dashboard-plan.md`)

Build one role at a time. Decisions taken with the user:

| Question | Choice |
|---|---|
| Which role first | **Member** (richest personal data, lands on Home) |
| Charts | **Chart.js 4.4.1 via jsdelivr CDN** (flatpickr-CDN precedent), theme-aware by resolving `--color-*` at runtime |
| Hero visual | Both a monthly bar chart and a sky-composition doughnut |

Deferred: Phase 2 Admin dashboard, Phase 3 Coordinator dashboard (Coordinator
needs its post-login redirect changed).

### 2. First build

New files:
- `Models/MemberDashboardVM.cs` — VM + `MonthBucket`, `SkySlice`, `UpcomingNight`, `AttentionItem`, `AiCount`
- `Models/CelestialTypeInfo.cs` — shared `Label` / `Group` / `GroupColorVar` / `ColorVarForType`; `Views/Reports/Ranking.cshtml` refactored to call it (dropped its inline copy)
- `Views/Home/MemberDashboard.cshtml`
- `wwwroot/js/dashboard.js`

Changed:
- `Controllers/HomeController.cs` — primary-ctor injects `AuxiliarClienteHttp`; `Index()` branches on `UserRole` → Members get `View("MemberDashboard", vm)`, other roles keep `Index.cshtml`. `BuildMemberDashboard()` aggregates `users/nights/{id}` + `loanrequests/user/{id}` + `loantickets/user/{id}`, each fetch in its own try/catch → `PartialLoad`.
- `wwwroot/css/site.css` — appended "Member dashboard" section.

Layout v1: page-head, KPI row (6 tiles), charts card (bar + divider + doughnut + legend, all stacked), Needs attention, Next up, Planning-quality strip, quick actions, full-page empty state.

Razor gotcha hit: a local function used as the *sole* body of an `else if { }`
throws `RZ1010 Unexpected "{" after "@"`. Fixed by dropping local functions in
favour of inline `@if` blocks / tuple-array `@foreach`.

Verified in a real browser as `bdiaz / Member@123` against the Render API
(`--ApiBaseUrl=https://stellarmind-api.onrender.com/` override, since Development
appsettings points at a local API on :7077 that isn't running). Build: 0 errors,
14 pre-existing warnings.

### 3. Design review (self-critique, at the user's ask)

Verdict: clean and on-brand but a **B+**, not "engaging / excellent". Problems:
1. Hero bar chart was mostly empty (2 bars at height 1 over 6 months) — deflating.
2. "Planning quality" showed 0 / 0 / 0 — looked broken (bdiaz's requests carry no `aiIndicator`).
3. Large dead space to the right of the tall left column.
4. Zeros rendered loud; primary action buried at the bottom; 2+ scrolls for a "welcome" screen; flat voice.

### 4. Revision pass

- **CTAs promoted:** "Plan a night" (primary) + "Request equipment" (ghost) → page-head, top-right.
- **Stargazer meta** moved under the greeting; names the next tier ("8 nights to Voyager") — new `MemberDashboardVM.NextLevelName`, `StargazerLevel()` returns a 3-tuple.
- **KPI strip:** zero tiles dimmed (`.kpi--zero`); Overdue tile only when `> 0`; Pending amber when `> 0`.
- **Hero chart reworked** → 12-month **combo**: bars = nights that month, filled line = **distinct objects discovered all-time** (only climbs, so low-activity members still see progress). New `MonthBucket.Cumulative` (running count of first-seen objects up to each month-end); `dashboard.js` builds a bar+line combo with a `withAlpha()` helper for the area fill.
- **Layout rebalanced:** left rail = hero chart + sky composition (doughnut & legend side-by-side via `.sky-split`); right rail = Needs attention + Next up. Dead space gone.
- **Planning quality** only rendered when `AiBreakdown.Any(a => a.Count > 0)`, dropped to a full-width row below the rails.

CSS added: `.page-head__meta`, `.page-head__meta-note`, `.page-head__actions`,
`.kpi--zero`, `.dash-rail`, `.dash-cols--even`, `.sky-split`. Re-verified in-browser.

Process note: killing/rebuilding the running app needs the child
`Obligatorio-N3D-342742-360021-Client.exe` (not just `dotnet.exe`) stopped, or the
build fails with a locked `apphost.exe`.

### 5. Empty-state pass

User: "instead of zeros and empty lists, always display a message with an action."

- **KPI tile = 0** → worded state (`kpi__value--empty`): "None borrowed", "None waiting", "Nothing queued", "None yet". Implemented as a tuple-array `@foreach`. Overdue tile still only shows when `> 0`. `.kpi--zero` opacity softened 0.5 → 0.8.
- **Hero chart, no nights** → `empty--sm` block: "No timeline yet" + *Plan a night*.
- **Sky composition** → card always renders now; empty → "Your sky map is blank" + *Plan a night*.
- **Needs attention, all clear** → keeps the positive message, adds a *Plan your next night* ghost action.
- **Next up, nothing upcoming** → upgraded from a `<p>` stub to an `empty--sm` block + *Plan a night*.
- **Planning quality** → card always renders; three states: AI strip / "No assessments yet" + *View my requests* (when `HasRequests`, bdiaz's case — verified in-browser) / "Nothing to assess yet" + *Request equipment*.
- New VM signals: `HasNights`, `HasRequests`.
- CSS added: `.empty--sm`, `.empty__action`, `.kpi__value--empty`; `.empty__body` capped at `46ch`.

Build: 0 errors. No console errors in-browser.

### Files touched this session

```
A  docs/member-dashboard-plan.md
A  docs/member-dashboard-devlog.md
A  Obligatorio-N3D-342742-360021-Client/Models/CelestialTypeInfo.cs
A  Obligatorio-N3D-342742-360021-Client/Models/MemberDashboardVM.cs
A  Obligatorio-N3D-342742-360021-Client/Views/Home/MemberDashboard.cshtml
A  Obligatorio-N3D-342742-360021-Client/wwwroot/js/dashboard.js
M  Obligatorio-N3D-342742-360021-Client/Controllers/HomeController.cs
M  Obligatorio-N3D-342742-360021-Client/Views/Reports/Ranking.cshtml
M  Obligatorio-N3D-342742-360021-Client/wwwroot/css/site.css
```

Not committed.

### Verification summary

- Logged in as `bdiaz / Member@123` against `stellarmind-api.onrender.com` in Chrome.
- Confirmed rendering: promoted CTAs, stargazer + next tier, worded zero KPI
  ("None borrowed"), 12-month combo chart (bars + cumulative area line), balanced
  two-rail layout, sky doughnut + side legend, Next-up equipment chips, Planning
  quality "No assessments yet" empty state + action. No console errors.
- **Not exercised** (bdiaz has a full history): the brand-new-member full-page
  empty state, the `PartialLoad` banner, and the empty states for the hero chart,
  sky composition, Next up, and "all clear" attention.

### Open follow-ups

- Exercise the untriggered empty states against a fresh/empty member account.
- "Nights logged" counts future-dated planned nights (bdiaz: 7 total, 5 upcoming).
  Consider splitting "observed" vs "planned" if it reads oddly.
- Wide-screen balance is good now but the right rail can still end well above the
  left rail depending on data volume — revisit if it looks sparse in practice.
- Phase 2 (Admin) and Phase 3 (Coordinator) still to do — see the plan.
