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

            // Admin / Coordinator keep the navigation-card grid for now (Phase 2 / 3).
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
                    Label = m.Month == 1 ? m.ToString("MMM yy") : m.ToString("MMM"),
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
    }
}
