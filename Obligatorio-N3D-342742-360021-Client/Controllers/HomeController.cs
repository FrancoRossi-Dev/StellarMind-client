using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Filters;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Services.Http;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class HomeController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Member")
                return View("MemberDashboard", BuildMemberDashboard());

            if (role == "Admin")
                return View("AdminDashboard", BuildAdminDashboard());

            if (role == "Coordinator")
                return View("CoordinatorDashboard", BuildCoordinatorDashboard());

            // Fallback for any unrecognised role: the old navigation-card grid.
            return View();
        }

        // ------------------------------------------------------------------ //
        //  Member dashboard aggregation                                      //
        // ------------------------------------------------------------------ //
        private MemberDashboardVM BuildMemberDashboard()
        {
            var vm = new MemberDashboardVM();

            int userId  = HttpContext.Session.GetInt32("UserId") ?? 0;
            var token   = HttpContext.Session.GetString("Token");
            var today   = DateTime.Today;

            var nights   = new List<ObservationNightVM>();
            var requests = new List<PendingLoanRequestVM>();
            var tickets  = new List<LoanTicketVM>();
            bool nightsOk = false, requestsOk = false, ticketsOk = false;

            try
            {
                nights = _auxiliarHttp.EnviarYDeserializar<List<ObservationNightVM>>(
                    $"api/v1/users/nights/{userId}", "GET", token: token) ?? new();
                nightsOk = true;
            }
            catch { /* panel degrades gracefully */ }

            try
            {
                requests = _auxiliarHttp.EnviarYDeserializar<List<PendingLoanRequestVM>>(
                    $"api/v1/loanrequests/user/{userId}", "GET", token: token) ?? new();
                requestsOk = true;
            }
            catch { }

            try
            {
                tickets = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                    $"api/v1/loantickets/user/{userId}", "GET", token: token) ?? new();
                ticketsOk = true;
            }
            catch { }

            vm.PartialLoad = !(nightsOk && requestsOk && ticketsOk);
            vm.HasAnyData  = nights.Count > 0 || requests.Count > 0 || tickets.Count > 0;
            vm.HasNights   = nights.Count > 0;
            vm.HasRequests = requests.Count > 0;

            static DateTime? Parse(string? s) => DateTime.TryParse(s, out var d) ? d : null;

            // ---- KPI tiles ----
            vm.NightsLogged     = nights.Count;
            vm.NightsInPlanning = nights.Count(n => !n.IsRequested);
            vm.DistinctObjects  = nights.Where(n => n.CelestialObject != null)
                                        .Select(n => n.CelestialObject!.Id)
                                        .Distinct().Count();
            vm.ActiveLoans      = tickets.Count(t => string.Equals(t.Status, "Loaned", StringComparison.OrdinalIgnoreCase));
            vm.OverdueLoans     = tickets.Count(t => t.IsOverdue);
            vm.PendingRequests  = requests.Count(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            // ---- "Your sky, over time" (last 12 months, zero-filled) ----
            // Bars = observation nights that month. Line = distinct objects discovered
            // all-time up to the end of that month (a collection that only grows).
            var firstSeen = nights
                .Where(n => n.CelestialObject != null)
                .Select(n => new { n.CelestialObject!.Id, Date = Parse(n.Date) })
                .Where(x => x.Date.HasValue)
                .GroupBy(x => x.Id)
                .Select(g => g.Min(x => x.Date!.Value))
                .ToList();

            for (int i = 11; i >= 0; i--)
            {
                var m = today.AddMonths(-i);
                var monthEndExclusive = new DateTime(m.Year, m.Month, 1).AddMonths(1);
                vm.ActivityByMonth.Add(new MonthBucket
                {
                    Label = m.Month == 1 ? m.ToString("MMM yyyy") : m.ToString("MMM"),
                    Count = nights.Count(n =>
                    {
                        var d = Parse(n.Date);
                        return d.HasValue && d.Value.Year == m.Year && d.Value.Month == m.Month;
                    }),
                    Cumulative = firstSeen.Count(d => d < monthEndExclusive)
                });
            }

            // ---- Sky composition ----
            vm.SkyComposition = nights
                .Where(n => n.CelestialObject != null)
                .GroupBy(n => CelestialTypeInfo.Group(n.CelestialObject!.Type))
                .Select(g => new SkySlice
                {
                    Group    = g.Key,
                    Count    = g.Count(),
                    ColorVar = CelestialTypeInfo.GroupColorVar(g.Key)
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            // ---- Upcoming nights ----
            var securedNightIds = requests
                .Where(r => r.ObservationNight != null
                            && (r.Status == "Pending" || r.Status == "Approved"))
                .Select(r => r.ObservationNight!.Id)
                .ToHashSet();

            vm.Upcoming = nights
                .Select(n => new { Night = n, Date = Parse(n.Date) })
                .Where(x => x.Date.HasValue && x.Date.Value.Date >= today)
                .OrderBy(x => x.Date!.Value)
                .Take(5)
                .Select(x => new UpcomingNight
                {
                    NightId          = x.Night.Id,
                    Date             = x.Date!.Value.ToString("MMM d, yyyy"),
                    ObjectName       = x.Night.CelestialObject?.Name ?? "—",
                    EquipmentSecured = x.Night.IsRequested || securedNightIds.Contains(x.Night.Id)
                })
                .ToList();

            // ---- Needs attention ----
            var attention = new List<AttentionItem>();

            foreach (var t in tickets.Where(t => t.IsOverdue))
            {
                string name = t.LoanRequest?.ObservationNight?.CelestialObject?.Name ?? "A loan";
                string due  = Parse(t.EndDate)?.ToString("MMM d") ?? "recently";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-triangle-exclamation",
                    Severity = "danger",
                    Text     = $"{name} loan was due {due}. Time to bring it home.",
                    Url      = Url.Action("MyLoans", "Loans") ?? "#"
                });
            }

            if (vm.PendingRequests > 0)
            {
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-hourglass-half",
                    Severity = "info",
                    Text     = vm.PendingRequests == 1
                        ? "1 request is waiting on a coordinator."
                        : $"{vm.PendingRequests} requests are waiting on a coordinator.",
                    Url      = Url.Action("MyLoans", "Loans") ?? "#"
                });
            }

            foreach (var n in nights.Where(n => !n.IsRequested))
            {
                var d = Parse(n.Date);
                if (d.HasValue && d.Value.Date >= today)
                {
                    attention.Add(new AttentionItem
                    {
                        Icon     = "fa-satellite-dish",
                        Severity = "warning",
                        Text     = $"{(n.CelestialObject?.Name ?? "A night")} on {d.Value:MMM d} has no equipment yet.",
                        Url      = Url.Action("Create", "Loans", new { nightId = n.Id }) ?? "#"
                    });
                }
            }

            vm.NeedsAttention = attention.Take(6).ToList();

            // ---- AI planning breakdown ----
            vm.AiBreakdown = new[] { "IDEAL", "ADECUADO", "NO_RECOMENDABLE" }
                .Select(k => new AiCount { Indicator = k, Count = requests.Count(r => r.AiIndicator == k) })
                .ToList();

            // ---- Stargazer level ----
            (vm.StargazerLevel, vm.NextLevelAt, vm.NextLevelName) = StargazerLevel(vm.NightsLogged);

            return vm;
        }

        private static (string Level, int NextAt, string NextName) StargazerLevel(int nights) => nights switch
        {
            >= 30 => ("Cosmonaut", 0, ""),
            >= 15 => ("Voyager", 30, "Cosmonaut"),
            >= 5  => ("Navigator", 15, "Voyager"),
            >= 1  => ("Observer", 5, "Navigator"),
            _     => ("Cadet", 1, "Observer")
        };

        // ------------------------------------------------------------------ //
        //  Admin dashboard aggregation                                       //
        // ------------------------------------------------------------------ //
        private AdminDashboardVM BuildAdminDashboard()
        {
            var vm    = new AdminDashboardVM();
            var token = HttpContext.Session.GetString("Token");
            var today = DateTime.Today;
            var now   = DateTime.Now;

            var users   = new List<UserVM>();
            var equip   = new List<EquipmentVM>();
            var tickets = new List<LoanTicketVM>();
            var pending = new List<PendingLoanRequestVM>();
            var nights  = new List<ObservationNightVM>();
            var logs    = new List<LogEventDto>();
            bool usersOk = false, equipOk = false, ticketsOk = false,
                 pendingOk = false, nightsOk = false, logsOk = false;

            try
            {
                users = _auxiliarHttp.EnviarYDeserializar<List<UserVM>>(
                    "api/v1/users", "GET", token: token) ?? new();
                usersOk = true;
            }
            catch { }

            try
            {
                equip = _auxiliarHttp.EnviarYDeserializar<List<EquipmentVM>>(
                    "api/v1/equipment", "GET", token: token) ?? new();
                equipOk = true;
            }
            catch { }

            try
            {
                tickets = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                    "api/v1/loantickets", "GET", token: token) ?? new();
                ticketsOk = true;
            }
            catch { }

            try
            {
                pending = _auxiliarHttp.EnviarYDeserializar<List<PendingLoanRequestVM>>(
                    "api/v1/loanrequests/pending", "GET", token: token) ?? new();
                pendingOk = true;
            }
            catch { }

            try
            {
                nights = _auxiliarHttp.EnviarYDeserializar<List<ObservationNightVM>>(
                    "api/v1/observationnights", "GET", token: token) ?? new();
                nightsOk = true;
            }
            catch { }

            try
            {
                logs = _auxiliarHttp.EnviarYDeserializar<List<LogEventDto>>(
                    "api/v1/logs", "GET", token: token) ?? new();
                logsOk = true;
            }
            catch { }

            vm.PartialLoad = !(usersOk && equipOk && ticketsOk && pendingOk && nightsOk && logsOk);
            vm.HasAnyData  = users.Count > 0 || equip.Count > 0 || tickets.Count > 0
                             || pending.Count > 0 || nights.Count > 0 || logs.Count > 0;

            static DateTime? Parse(string? s) => DateTime.TryParse(s, out var d) ? d : null;

            // ---- KPI tiles ----
            vm.MemberCount        = users.Count(u => u.role == "Member");
            vm.CoordinatorCount   = users.Count(u => u.role == "Coordinator");
            vm.EquipmentUnits     = equip.Sum(e => e.Quantity);
            vm.EquipmentAvailable = equip.Sum(e => e.availableQuantity);
            vm.OnLoanNow          = tickets.Count(t => string.Equals(t.Status, "Loaned", StringComparison.OrdinalIgnoreCase));
            vm.OverdueLoans       = tickets.Count(t => t.IsOverdue);
            vm.PendingRequests    = pending.Count;
            vm.ErrorsThisWeek     = logs.Count(l => l.IsError && Parse(l.Date) is { } d && d >= today.AddDays(-7));

            vm.ClubSizeNote = $"{vm.MemberCount} member{(vm.MemberCount == 1 ? "" : "s")}, " +
                              $"{vm.CoordinatorCount} coordinator{(vm.CoordinatorCount == 1 ? "" : "s")}";

            // ---- Club activity over time (last 12 months, zero-filled) ----
            // Bars = observation nights logged club-wide that month.
            // Line = cumulative observation nights all-time up to the end of that month.
            var nightDates = nights.Select(n => Parse(n.Date)).Where(d => d.HasValue).Select(d => d!.Value).ToList();
            for (int i = 11; i >= 0; i--)
            {
                var m = today.AddMonths(-i);
                var monthEndExclusive = new DateTime(m.Year, m.Month, 1).AddMonths(1);
                vm.ActivityByMonth.Add(new MonthBucket
                {
                    Label      = m.Month == 1 ? m.ToString("MMM yyyy") : m.ToString("MMM"),
                    Count      = nightDates.Count(d => d.Year == m.Year && d.Month == m.Month),
                    Cumulative = nightDates.Count(d => d < monthEndExclusive)
                });
            }

            // ---- Equipment utilization by type ----
            foreach (var type in new[] { "Telescope", "Mount", "Camera", "Eyepiece" })
            {
                var forType = equip.Where(e => string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase)).ToList();
                if (forType.Count == 0) continue;
                int total = forType.Sum(e => e.Quantity);
                int avail = forType.Sum(e => e.availableQuantity);
                vm.EquipmentUtilization.Add(new UtilizationBar
                {
                    Type      = type,
                    Available = avail,
                    OnLoan    = Math.Max(0, total - avail)
                });
            }

            // ---- Role mix ----
            vm.RoleMix = new[]
                {
                    ("Member", "--color-accent"),
                    ("Coordinator", "--color-info"),
                    ("Admin", "--color-warning")
                }
                .Select(r => new RoleSlice { Role = r.Item1, Count = users.Count(u => u.role == r.Item1), ColorVar = r.Item2 })
                .Where(s => s.Count > 0)
                .ToList();

            // ---- Request queue (soonest target night first) ----
            vm.RequestQueue = pending
                .Select(r => new { Req = r, Date = Parse(r.ObservationNight?.Date) })
                .OrderBy(x => x.Date ?? DateTime.MaxValue)
                .Take(5)
                .Select(x => new QueueItem
                {
                    RequestId   = x.Req.RequestId,
                    Requester   = x.Req.RequestingUser?.username ?? "—",
                    NightDate   = x.Date?.ToString("MMM d, yyyy") ?? "Date TBD",
                    ObjectName  = x.Req.ObservationNight?.CelestialObject?.Name ?? "—",
                    AiIndicator = x.Req.AiIndicator,
                    Url         = Url.Action("Index", "Loans") ?? "#"
                })
                .ToList();

            // ---- AI risk of the current queue ----
            vm.AiBreakdown = new[] { "IDEAL", "ADECUADO", "NO_RECOMENDABLE" }
                .Select(k => new AiCount { Indicator = k, Count = pending.Count(r => r.AiIndicator == k) })
                .ToList();

            // ---- Needs attention ----
            var attention = new List<AttentionItem>();

            foreach (var t in tickets.Where(t => t.IsOverdue))
            {
                string who  = t.LoanRequest?.RequestingUser?.username ?? "A member";
                string what = t.LoanRequest?.ObservationNight?.CelestialObject?.Name ?? "a loan";
                string due  = Parse(t.EndDate)?.ToString("MMM d") ?? "recently";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-triangle-exclamation",
                    Severity = "danger",
                    Text     = $"{who}'s {what} loan was due {due} and is still out.",
                    Url      = Url.Action("LoanAudit", "Reports") ?? "#"
                });
            }

            foreach (var r in pending
                .Select(r => new { Req = r, Date = Parse(r.ObservationNight?.Date) })
                .Where(x => x.Date.HasValue
                            && x.Date.Value.Date >= today
                            && x.Date.Value.Date <= today.AddDays(2))
                .Take(3))
            {
                string what = r.Req.ObservationNight?.CelestialObject?.Name ?? "A night";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-hourglass-half",
                    Severity = "warning",
                    Text     = $"{what} on {r.Date!.Value:MMM d} is almost here and its request is still pending.",
                    Url      = Url.Action("Index", "Loans") ?? "#"
                });
            }

            foreach (var g in equip
                .GroupBy(e => e.Type)
                .Where(g => g.Sum(e => e.availableQuantity) == 0 && g.Sum(e => e.Quantity) > 0))
            {
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-box-open",
                    Severity = "warning",
                    Text     = $"No {g.Key.ToLowerInvariant()} is free right now. Every unit is in use.",
                    Url      = Url.Action("Index", "Equipment") ?? "#"
                });
            }

            foreach (var l in logs
                .Where(l => l.IsError && Parse(l.Date) is { } d && d >= today.AddDays(-7))
                .OrderByDescending(l => Parse(l.Date))
                .Take(2))
            {
                string op = string.IsNullOrWhiteSpace(l.Operation) ? "An action" : l.Operation;
                string tbl = string.IsNullOrWhiteSpace(l.TableName) ? "" : $" on {l.TableName}";
                string when = Parse(l.Date)?.ToString("MMM d") ?? "recently";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-circle-exclamation",
                    Severity = "danger",
                    Text     = $"{op}{tbl} hit an error {when}.",
                    Url      = Url.Action("Logs", "Reports") ?? "#"
                });
            }

            vm.NeedsAttention = attention.Take(6).ToList();

            // ---- Coordinator throughput ----
            var monthStart = new DateTime(today.Year, today.Month, 1);
            vm.CoordinatorThroughput = tickets
                .Where(t => !string.IsNullOrWhiteSpace(t.CoordinatorName))
                .GroupBy(t => t.CoordinatorName)
                .Select(g => new CoordinatorStat
                {
                    Name      = g.Key,
                    ThisMonth = g.Count(t => Parse(t.StartDate) is { } d && d >= monthStart),
                    AllTime   = g.Count()
                })
                .OrderByDescending(c => c.ThisMonth)
                .ThenByDescending(c => c.AllTime)
                .Take(8)
                .ToList();

            // ---- Recent activity ----
            static string OpIcon(string? op) => (op ?? "").ToUpperInvariant() switch
            {
                "CREATE" or "POST" => "fa-plus",
                "UPDATE" or "PUT"  => "fa-pen",
                "DELETE"           => "fa-trash",
                _                  => "fa-circle-info"
            };
            // The audit log records reads too (every list fetch → "… retrieved
            // successfully"), which would swamp the digest. Keep only writes and
            // anything that errored; the full stream stays one click away.
            static bool IsMutation(string? op) => (op ?? "").ToUpperInvariant()
                is "CREATE" or "UPDATE" or "DELETE" or "POST" or "PUT" or "PATCH";
            string Rel(DateTime? d)
            {
                if (!d.HasValue) return "";
                var span = now - d.Value;
                if (span.TotalMinutes < 1)  return "just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
                if (span.TotalHours < 24)   return $"{(int)span.TotalHours}h ago";
                if (span.TotalDays < 30)    return $"{(int)span.TotalDays}d ago";
                return d.Value.ToString("MMM d");
            }

            vm.RecentActivity = logs
                .Where(l => l.IsError || IsMutation(l.Operation))
                .Select(l => new { Log = l, Date = Parse(l.Date) })
                .OrderByDescending(x => x.Date ?? DateTime.MinValue)
                .Take(6)
                .Select(x =>
                {
                    var l = x.Log;
                    string text = !string.IsNullOrWhiteSpace(l.Message)
                        ? l.Message
                        : $"{(string.IsNullOrWhiteSpace(l.Operation) ? "Action" : l.Operation)} on {(string.IsNullOrWhiteSpace(l.TableName) ? "record" : l.TableName)}";
                    return new AuditEntry
                    {
                        Icon    = OpIcon(l.Operation),
                        Text    = text,
                        Actor   = l.Email,
                        When    = Rel(x.Date),
                        IsError = l.IsError,
                        Url     = Url.Action("Logs", "Reports") ?? "#"
                    };
                })
                .ToList();

            return vm;
        }

        // ------------------------------------------------------------------ //
        //  Coordinator dashboard aggregation                                 //
        // ------------------------------------------------------------------ //
        private CoordinatorDashboardVM BuildCoordinatorDashboard()
        {
            var vm    = new CoordinatorDashboardVM();
            var token = HttpContext.Session.GetString("Token");
            var today = DateTime.Today;
            int coordinatorId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var pending   = new List<PendingLoanRequestVM>();
            var myTickets = new List<LoanTicketVM>();
            bool pendingOk = false, ticketsOk = false;

            try
            {
                pending = _auxiliarHttp.EnviarYDeserializar<List<PendingLoanRequestVM>>(
                    "api/v1/loanrequests/pending", "GET", token: token) ?? new();
                pendingOk = true;
            }
            catch { }

            try
            {
                // loantickets/coordinator/{id} is Admin-only (403 for the coordinator
                // themselves), so read the full list and keep this coordinator's own.
                var allTickets = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                    "api/v1/loantickets", "GET", token: token) ?? new();
                myTickets = allTickets.Where(t => t.CoordinatorId == coordinatorId).ToList();
                ticketsOk = true;
            }
            catch { }

            vm.PartialLoad = !(pendingOk && ticketsOk);
            vm.HasAnyData  = pending.Count > 0 || myTickets.Count > 0;
            vm.HasQueue    = pending.Count > 0;
            vm.HasTickets  = myTickets.Count > 0;

            static DateTime? Parse(string? s) => DateTime.TryParse(s, out var d) ? d : null;

            bool IsLoaned(LoanTicketVM t) => string.Equals(t.Status, "Loaned", StringComparison.OrdinalIgnoreCase);

            // ---- KPI tiles ----
            vm.PendingRequests = pending.Count;
            vm.FlaggedByAi     = pending.Count(r => r.AiIndicator == "NO_RECOMENDABLE");
            vm.OnLoanNow       = myTickets.Count(IsLoaned);
            vm.DueThisWeek     = myTickets.Count(t => IsLoaned(t)
                                    && Parse(t.EndDate) is { } d
                                    && d.Date >= today && d.Date <= today.AddDays(7));
            vm.OverdueLoans    = myTickets.Count(t => t.IsOverdue);
            vm.IssuedAllTime   = myTickets.Count;

            vm.QueueNote = pending.Count == 0
                ? "Queue clear"
                : $"{pending.Count} waiting in the queue";

            // ---- Approvals over time (last 12 months, zero-filled) ----
            // Bars = tickets this coordinator issued that month (by StartDate).
            // Line = cumulative tickets issued all-time up to the end of that month.
            var issuedDates = myTickets.Select(t => Parse(t.StartDate))
                                       .Where(d => d.HasValue).Select(d => d!.Value).ToList();
            for (int i = 11; i >= 0; i--)
            {
                var m = today.AddMonths(-i);
                var monthEndExclusive = new DateTime(m.Year, m.Month, 1).AddMonths(1);
                vm.ApprovalsByMonth.Add(new MonthBucket
                {
                    Label      = m.Month == 1 ? m.ToString("MMM yyyy") : m.ToString("MMM"),
                    Count      = issuedDates.Count(d => d.Year == m.Year && d.Month == m.Month),
                    Cumulative = issuedDates.Count(d => d < monthEndExclusive)
                });
            }

            // ---- Queue risk: pending requests grouped by AI indicator ----
            vm.QueueRisk = new[] { "IDEAL", "ADECUADO", "NO_RECOMENDABLE" }
                .Select(k => new AiCount { Indicator = k, Count = pending.Count(r => r.AiIndicator == k) })
                .ToList();
            int unrated = pending.Count(r => string.IsNullOrEmpty(r.AiIndicator));
            if (unrated > 0)
                vm.QueueRisk.Add(new AiCount { Indicator = "UNRATED", Count = unrated });

            // ---- Request queue (soonest target night first) ----
            vm.RequestQueue = pending
                .Select(r => new { Req = r, Date = Parse(r.ObservationNight?.Date) })
                .OrderBy(x => x.Date ?? DateTime.MaxValue)
                .Take(6)
                .Select(x => new QueueItem
                {
                    RequestId   = x.Req.RequestId,
                    Requester   = x.Req.RequestingUser?.username ?? "—",
                    NightDate   = x.Date?.ToString("MMM d, yyyy") ?? "Date TBD",
                    ObjectName  = x.Req.ObservationNight?.CelestialObject?.Name ?? "—",
                    AiIndicator = x.Req.AiIndicator,
                    Url         = Url.Action("Index", "Loans") ?? "#"
                })
                .ToList();

            // ---- Returns due soon (their loaned tickets, soonest first) ----
            vm.ReturnsDue = myTickets
                .Where(IsLoaned)
                .Select(t => new { Ticket = t, Date = Parse(t.EndDate) })
                .OrderBy(x => x.Date ?? DateTime.MaxValue)
                .Take(5)
                .Select(x => new ReturnDue
                {
                    Member     = x.Ticket.LoanRequest?.RequestingUser?.username ?? "—",
                    ObjectName = x.Ticket.LoanRequest?.ObservationNight?.CelestialObject?.Name ?? "—",
                    DueDate    = x.Date?.ToString("MMM d, yyyy") ?? "Date TBD",
                    IsOverdue  = x.Ticket.IsOverdue,
                    Url        = Url.Action("Index", "Loans") ?? "#"
                })
                .ToList();

            // ---- Needs attention ----
            var attention = new List<AttentionItem>();

            foreach (var t in myTickets.Where(t => t.IsOverdue))
            {
                string who  = t.LoanRequest?.RequestingUser?.username ?? "A member";
                string what = t.LoanRequest?.ObservationNight?.CelestialObject?.Name ?? "a loan";
                string due  = Parse(t.EndDate)?.ToString("MMM d") ?? "recently";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-triangle-exclamation",
                    Severity = "danger",
                    Text     = $"{who}'s {what} loan was due {due} and is still out.",
                    Url      = Url.Action("Index", "Loans") ?? "#"
                });
            }

            if (vm.FlaggedByAi > 0)
            {
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-robot",
                    Severity = "warning",
                    Text     = vm.FlaggedByAi == 1
                        ? "1 pending request was flagged by the AI. Give it a closer look."
                        : $"{vm.FlaggedByAi} pending requests were flagged by the AI. Give them a closer look.",
                    Url      = Url.Action("Index", "Loans") ?? "#"
                });
            }

            foreach (var x in pending
                .Select(r => new { Req = r, Date = Parse(r.ObservationNight?.Date) })
                .Where(x => x.Date.HasValue && x.Date.Value.Date >= today && x.Date.Value.Date <= today.AddDays(3))
                .Take(3))
            {
                string what = x.Req.ObservationNight?.CelestialObject?.Name ?? "A night";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-hourglass-half",
                    Severity = "warning",
                    Text     = $"{what} on {x.Date!.Value:MMM d} is almost here and its request is still pending.",
                    Url      = Url.Action("Index", "Loans") ?? "#"
                });
            }

            foreach (var t in myTickets
                .Where(t => IsLoaned(t) && !t.IsOverdue
                            && Parse(t.EndDate) is { } d && d.Date >= today && d.Date <= today.AddDays(2))
                .Take(3))
            {
                string what = t.LoanRequest?.ObservationNight?.CelestialObject?.Name ?? "A loan";
                string due  = Parse(t.EndDate)?.ToString("MMM d") ?? "soon";
                attention.Add(new AttentionItem
                {
                    Icon     = "fa-clock",
                    Severity = "info",
                    Text     = $"{what} is due back {due}.",
                    Url      = Url.Action("Index", "Loans") ?? "#"
                });
            }

            vm.NeedsAttention = attention.Take(6).ToList();

            return vm;
        }
    }
}
