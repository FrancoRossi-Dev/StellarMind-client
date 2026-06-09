using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Filters;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Services.Http;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class ReportsController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Ranking()
        {
            var token = HttpContext.Session.GetString("Token");
            var nights = new List<ObservationNightVM>();
            try
            {
                nights = _auxiliarHttp.EnviarYDeserializar<List<ObservationNightVM>>(
                    "api/v1/observationnights", "GET", token: token)
                    ?? new List<ObservationNightVM>();
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load the ranking. The stars are shy tonight.";
            }

            var ranking = nights
                .Where(n => n.CelestialObject != null)
                .GroupBy(n => n.CelestialObject!.Id)
                .Select(g => new CelestialRankingItem
                {
                    ObjectId = g.Key,
                    Name = g.First().CelestialObject!.Name,
                    Type = g.First().CelestialObject!.Type,
                    Count = g.Count()
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            return View(ranking);
        }

        [AccessFilter("Admin")]
        public IActionResult ByTelescope(int? telescopeId = null)
        {
            var token = HttpContext.Session.GetString("Token");

            var equipment = new List<EquipmentVM>();
            try
            {
                equipment = _auxiliarHttp.EnviarYDeserializar<List<EquipmentVM>>(
                    "api/v1/equipment", "GET", token: token)
                    ?? new List<EquipmentVM>();
            }
            catch { }

            ViewBag.Telescopes = equipment.Where(e => e.Type == "Telescope").ToList();
            ViewBag.SelectedTelescopeId = telescopeId;

            var members = new List<UserVM>();
            if (telescopeId.HasValue)
            {
                try
                {
                    members = _auxiliarHttp.EnviarYDeserializar<List<UserVM>>(
                        $"api/v1/users/list/{telescopeId}/telescope", "GET", token: token)
                        ?? new List<UserVM>();
                }
                catch (Exception)
                {
                    ViewBag.msg = "Member list couldn't be loaded. Something got in the way.";
                }
            }

            return View(members);
        }

        [AccessFilter("Admin")]
        public IActionResult LoanAudit(int? coordinatorId = null)
        {
            var token = HttpContext.Session.GetString("Token");

            var users = new List<UserVM>();
            try
            {
                users = _auxiliarHttp.EnviarYDeserializar<List<UserVM>>(
                    "api/v1/users", "GET", token: token)
                    ?? new List<UserVM>();
            }
            catch { }

            ViewBag.Coordinators = users
                .Where(u => u.role == "Coordinator" || u.role == "Admin")
                .OrderBy(u => u.fullName)
                .ToList();
            ViewBag.SelectedCoordinatorId = coordinatorId;

            var tickets = new List<LoanTicketVM>();
            try
            {
                if (coordinatorId.HasValue)
                {
                    tickets = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                        $"api/v1/loantickets/coordinator/{coordinatorId}", "GET", token: token)
                        ?? new List<LoanTicketVM>();
                }
                else
                {
                    tickets = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                        "api/v1/loantickets", "GET", token: token)
                        ?? new List<LoanTicketVM>();
                }
            }
            catch (Exception)
            {
                ViewBag.msg = "The audit log drifted out of range. Try again.";
            }

            return View(tickets
                .OrderByDescending(t => DateTime.TryParse(t.StartDate, out var d) ? d : DateTime.MinValue)
                .ToList());
        }

        [AccessFilter("Admin, Coordinator")]
        public IActionResult Logs()
        {
            var token = HttpContext.Session.GetString("Token");
            var logs  = new List<LogEventDto>();
            try
            {
                logs = _auxiliarHttp.EnviarYDeserializar<List<LogEventDto>>(
                    "api/v1/logs", "GET", token: token)
                    ?? new List<LogEventDto>();
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load the event log. Something drifted out of range.";
            }
            return View(logs);
        }

        [AccessFilter("Admin")]
        public IActionResult AuditDetail(int id)
        {
            var token = HttpContext.Session.GetString("Token");

            LoanTicketVM? ticket = null;
            var logs = new List<LogEventDto>();

            try
            {
                ticket = _auxiliarHttp.EnviarYDeserializar<LoanTicketVM>(
                    $"api/v1/loantickets/{id}", "GET", token: token);
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't retrieve the ticket. It may have wandered off.";
            }

            try
            {
                logs = _auxiliarHttp.EnviarYDeserializar<List<LogEventDto>>(
                    $"api/v1/logs/loanticket/{id}", "GET", token: token)
                    ?? new List<LogEventDto>();
            }
            catch (Exception ex)
            {
                ViewBag.LogError = ex.Message;
            }

            ViewBag.Ticket = ticket;
            ViewBag.Logs = logs;
            return View();
        }
    }
}
