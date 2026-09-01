# Home Dashboard Plan

Status: **Phase 1 built, revised & verified** · Last updated: 2026-09-01

The current `Home/Index` is only a grid of navigation cards with no data. This plan
replaces it with a real, engaging dashboard. We build **one role at a time**.

| Role | Lands on `Home/Index` today? | Phase |
|------|------------------------------|-------|
| Member | Yes | **Phase 1 — in progress** |
| Admin | Yes | Phase 2 — deferred |
| Coordinator | No (redirected to `Loans/Index`) | Phase 3 — deferred |

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

## Phase 2 — Admin dashboard (deferred, notes only)

Data: `users` (role mix donut), `equipment` (`Quantity` vs `availableQuantity`
by `Type` — utilization bar), all `loantickets` (on-loan now, overdue table),
`loanrequests/pending` (queue depth), `observationnights` (club activity trend),
`logs` (recent audit events). Same chart + tile system as Phase 1.

## Phase 3 — Coordinator dashboard (deferred, notes only)

Requires changing the post-login redirect in `UsersController.Login` from
`Loans/Index` to `Home/Index` (or adding a Dashboard entry point). Data:
`loanrequests/pending` (oldest-first queue, AI-risk count), 
`loantickets/coordinator/{id}` (their active loans, returns due this week,
approvals-per-month chart).
