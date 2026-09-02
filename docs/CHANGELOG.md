# Changelog

Recent work on `feat/dashboards`.

- `a7bd549 "stage 1 of dashboard design"` — **committed**: the Member dashboard
  (Phase 1) and its plan.
- Everything after that — Phase 2 (Admin), Phase 3 (Coordinator), the layout and
  design passes, and the docs — is **uncommitted** on top of `a7bd549`.

Detail: [`member-dashboard-devlog.md`](./member-dashboard-devlog.md) ·
reference: [`dashboards.md`](./dashboards.md)

---

## 2026-09-02

### Coordinator dashboard (Phase 3) + all-roles redirect
- New `CoordinatorDashboard` — "your desk": pending queue, the loans this
  coordinator issued, AI queue-risk, returns due soon, approvals-over-time.
- `UsersController.Login` now redirects **every** role to `Home/Index`; the
  Coordinator → `Loans/Index` special-case is gone. `HomeController.Index` picks
  the view by role (`Member` / `Admin` / `Coordinator` dashboards; old nav-card
  grid only as an unknown-role fallback).
- Sidebar entry stays **"Dashboard"** (evaluated "Home", rejected — it's the
  established term and the page is a real dashboard for all roles).
- Endpoint fix: `loantickets/coordinator/{id}` is Admin-only (403 for the
  coordinator); read the full `loantickets` list and filter by `coordinatorId`
  client-side instead.
- New: `Models/CoordinatorDashboardVM.cs`, `Views/Home/CoordinatorDashboard.cshtml`,
  `wwwroot/js/coordinator-dashboard.js`.

### Bento-grid layout rewrite (Admin + Member)
- Replaced `.dash-cols` + two independent `.dash-rail` flex stacks with
  `.dash-bento` — one 6-column grid, cards as direct children, `align-items:
  stretch`, span helpers `--wide` (4) / `--half` (3) / `--narrow` (2) / `--full`.
- Panels paired so every row sums to 6; no more ragged mid-layout gaps between a
  tall chart column and a short list column.
- `site.css`: removed `.dash-cols` / `.dash-cols--even` / `.dash-rail`.

### Design + content-priority pass
- `.sky-split` doughnut was clipping its legend (Admin "Who's in the club",
  intermittently Member) — the Chart.js canvas grew into an unbounded `1fr`
  track. Capped `.sky-split .chart-wrap { max-width: 240px }`.
- Admin **Recent activity** was ~80% noise (the audit log records reads). Now
  filtered to writes + errors; `OpIcon` maps POST/PUT.
- **Coordinator dashboard reordered** to lead with the Request queue (the
  coordinator's actual job), history chart moved to the foot.
- Explore-row link labels → Title Case, matching the sidebar nav.
- Activity chart January label → "Jan 2026" (`MMM yyyy`), was the ambiguous
  "Jan 26".

### Data-source validation + docs
- Validated every dashboard component against the live Render API for all three
  roles — source endpoint, field names, and calculations. Every rendered figure
  matches the API exactly. All `Model.*` view bindings check out against the VMs.
- Fix: Admin "Needs attention" stale-pending item now also requires
  `night >= today` (was future-tense copy with no lower bound).
- New: `docs/dashboards.md` (per-panel source/calculation reference),
  `docs/screenshots/*.jpg` (7 shots), `docs/CHANGELOG.md`.

---

## 2026-09-01 — uncommitted

### Admin dashboard (Phase 2)
- New `AdminDashboard` — club-wide read: KPI strip, 12-month club-activity combo
  chart, equipment-utilization bar, role-mix doughnut, needs-attention, request
  queue, AI queue-risk, coordinator throughput, recent activity.
- Aggregates six endpoints (`users`, `equipment`, `loantickets`,
  `loanrequests/pending`, `observationnights`, `logs`), each in its own
  try/catch → `PartialLoad`.
- New: `Models/AdminDashboardVM.cs`, `Views/Home/AdminDashboard.cshtml`,
  `wwwroot/js/admin-dashboard.js`.

---

## 2026-09-01 — committed in `a7bd549`

### Member dashboard (Phase 1)
- New `MemberDashboard` — personal landing: KPI strip, 12-month activity combo
  (nights + cumulative objects discovered), sky-composition doughnut,
  needs-attention, next-up nights, planning-quality AI strip, stargazer level.
- `HomeController` gains DI (`AuxiliarClienteHttp`) and role branching;
  aggregates `users/nights`, `loanrequests/user`, `loantickets/user`.
- New: `Models/MemberDashboardVM.cs`, `Models/CelestialTypeInfo.cs` (shared
  type→group→colour map, also adopted by `Reports/Ranking`),
  `Views/Home/MemberDashboard.cshtml`, `wwwroot/js/dashboard.js`.
- Chart.js 4.4.1 via jsdelivr, theme-aware (palette resolved from `--color-*`
  tokens at runtime).
- Revision + empty-state passes: promoted CTAs to the page head, reworked the
  hero chart from a sparse bar chart into the combo, every panel shows a worded
  state + action instead of a bare zero / empty list.

---

## Uncommitted working tree (vs `a7bd549`)

Modified:
```
Controllers/HomeController.cs          + Admin & Coordinator branches/aggregators,
                                         recent-activity read filter, stale-pending guard
Controllers/UsersController.cs         login → Home for every role
Views/Home/MemberDashboard.cshtml      bento grid, Title-Case explore links
wwwroot/css/site.css                   bento-grid system, sky-split width cap
appsettings.Development.json            ApiBaseUrl → Render
docs/member-dashboard-plan.md          Phase 2 + 3 spec, validation note
```

New:
```
Models/AdminDashboardVM.cs
Models/CoordinatorDashboardVM.cs
Views/Home/AdminDashboard.cshtml
Views/Home/CoordinatorDashboard.cshtml
wwwroot/js/admin-dashboard.js
wwwroot/js/coordinator-dashboard.js
docs/member-dashboard-devlog.md        session-by-session log
docs/dashboards.md                     per-panel data-source reference
docs/screenshots/*.jpg                 7 screenshots
docs/CHANGELOG.md                      this file
```

Build: 0 errors.
