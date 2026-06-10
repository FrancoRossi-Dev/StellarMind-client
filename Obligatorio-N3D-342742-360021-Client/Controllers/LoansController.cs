using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class LoansController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        [AccessFilter("Admin, Coordinator")]
        public IActionResult Index()
        {
            try
            {
                var token    = HttpContext.Session.GetString("Token");
                var requests = _auxiliarHttp
                    .EnviarYDeserializar<List<PendingLoanRequestVM>>("api/v1/loanrequests/pending", "GET", token: token)
                    ?? new List<PendingLoanRequestVM>();

                try
                {
                    var tickets = _auxiliarHttp
                        .EnviarYDeserializar<List<LoanTicketVM>>("api/v1/loantickets", "GET", token: token)
                        ?? new List<LoanTicketVM>();
                    ViewBag.RequestTicketMap = tickets
                        .Where(t => t.LoanRequest != null)
                        .GroupBy(t => t.LoanRequest!.RequestId)
                        .ToDictionary(g => g.Key, g => g.First().Id);
                }
                catch { ViewBag.RequestTicketMap = new Dictionary<int, int>(); }

                return View(requests);
            }
            catch (Exception)
            {
                ViewBag.msg = "The loan list seems to be orbiting somewhere else. Try again.";
                return View(new List<PendingLoanRequestVM>());
            }
        }

        [HttpGet("Loans/Create")]
        public IActionResult Create(int? nightId = null)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var token   = HttpContext.Session.GetString("Token");
            var nights  = new List<ObservationNightVM>();

            if (userId.HasValue)
            {
                try
                {
                    var all = _auxiliarHttp
                        .EnviarYDeserializar<List<ObservationNightVM>>($"api/v1/users/nights/{userId}", "GET", token: token)
                        ?? new List<ObservationNightVM>();
                    nights = all.Where(n => !n.IsRequested).ToList();
                }
                catch { }
            }

            try
            {
                var allEquip = _auxiliarHttp
                    .EnviarYDeserializar<List<EquipmentVM>>("api/v1/equipment", "GET", token: token)
                    ?? new List<EquipmentVM>();

                ViewBag.Telescopes = allEquip.Where(e => e.Type == "Telescope").ToList();
                ViewBag.Mounts     = allEquip.Where(e => e.Type == "Mount").ToList();
                ViewBag.Eyepieces  = allEquip.Where(e => e.Type == "Eyepiece").ToList();
                ViewBag.Cameras    = allEquip.Where(e => e.Type == "Camera").ToList();
            }
            catch
            {
                ViewBag.Telescopes = new List<EquipmentVM>();
                ViewBag.Mounts     = new List<EquipmentVM>();
                ViewBag.Eyepieces  = new List<EquipmentVM>();
                ViewBag.Cameras    = new List<EquipmentVM>();
            }

            ViewBag.Nights = nights;
            ViewBag.SelectedNightId = nightId;
            return View();
        }

        [HttpPost("Loans/Assess")]
        public IActionResult Assess(int observationNightId, int telescopeId, int mountId, int? eyepieceId, int? cameraId)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                var dto = new CreateLoanRequestDto
                {
                    ObservationNightId = observationNightId,
                    TelescopeId = telescopeId,
                    MountId = mountId,
                    EyepieceId = eyepieceId,
                    CameraId = cameraId
                };
                var result = _auxiliarHttp.EnviarYDeserializar<AiAssessmentDto>(
                    "api/v1/loanrequests/evaluate", "POST", dto, token);
                return Json(new { aiIndicator = result?.Indicator, aiDetail = result?.Detail });
            }
            catch (Exception)
            {
                return StatusCode(503, new { error = true });
            }
        }

        [HttpPost("Loans/Create")]
        public IActionResult Create(int observationNightId, int telescopeId, int mountId, int? eyepieceId, int? cameraId)
        {
            try
            {
                var dto = new CreateLoanRequestDto
                {
                    ObservationNightId = observationNightId,
                    TelescopeId = telescopeId,
                    MountId = mountId,
                    EyepieceId = eyepieceId,
                    CameraId = cameraId
                };
                var token = HttpContext.Session.GetString("Token");
                var result = _auxiliarHttp.EnviarYDeserializar<PendingLoanRequestVM>("api/v1/loanrequests", "POST", dto, token);
                TempData["Success"] = "Loan request submitted.";
                if (result?.AiIndicator is not null)
                {
                    TempData["AiIndicator"] = result.AiIndicator;
                    TempData["AiDetail"]    = result.AiDetail;
                }
                return RedirectToAction("MyLoans");
            }
            catch (Exception)
            {
                ViewBag.msg = "The request got lost in the void. Please try again.";
                return View();
            }
        }

        [HttpPost("Loans/Approve/{id}")]
        public IActionResult Approve(int id, string endDate)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                int coordinatorId = HttpContext.Session.GetInt32("UserId") ?? 0;
                string isoEnd = DateTime.TryParse(endDate, out var dt) ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : endDate;
                var body = new { endDate = isoEnd, coordinatorId };
                _auxiliarHttp.EnviarSolicitud($"api/v1/loanrequests/{id}/approve", "PUT", body, token);
                TempData["Success"] = "Loan request approved.";
                return RedirectToAction("Index");
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                TempData["Error"] = "That request seems to have drifted off — it may no longer exist.";
                return RedirectToAction("Index");
            }
            catch (ApiException ex) when (ex.StatusCode == 409)
            {
                TempData["Error"] = "This request couldn't be approved — the equipment may have been claimed or the request has already moved on.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't approve that loan. Something drifted off course.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Reject/{id}")]
        public IActionResult Reject(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loanrequests/{id}/reject", "PUT", token: token);
                TempData["Success"] = "Loan request rejected.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't reject that loan. Something went sideways out there.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Return/{id}")]
        public IActionResult Return(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loantickets/{id}/return", "PUT", token: token);
                TempData["Success"] = "Loan marked as returned.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The return couldn't be logged. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loantickets/{id}/cancel", "PUT", token: token);
                TempData["Success"] = "Loan canceled.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't cancel that loan. Try again in a moment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/CancelRequest/{id}")]
        public IActionResult CancelRequest(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loanrequests/{id}/cancel", "PUT", token: token);
                TempData["Success"] = "Request canceled.";
                return RedirectToAction("MyLoans");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't cancel that request. Try again in a moment.";
                return RedirectToAction("MyLoans");
            }
        }

        [HttpGet("Loans/MyLoans")]
        [AccessFilter("Member")]
        public IActionResult MyLoans()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var token   = HttpContext.Session.GetString("Token");

            var requests = new List<PendingLoanRequestVM>();
            var tickets  = new List<LoanTicketVM>();

            if (userId.HasValue)
            {
                try
                {
                    requests = _auxiliarHttp
                        .EnviarYDeserializar<List<PendingLoanRequestVM>>($"api/v1/loanrequests/user/{userId}", "GET", token: token)
                        ?? new List<PendingLoanRequestVM>();
                }
                catch { }

                try
                {
                    tickets = _auxiliarHttp
                        .EnviarYDeserializar<List<LoanTicketVM>>($"api/v1/loantickets/user/{userId}", "GET", token: token)
                        ?? new List<LoanTicketVM>();
                }
                catch { }
            }

            ViewBag.Requests = requests;
            ViewBag.Tickets  = tickets;
            return View();
        }

        [HttpPost("Loans/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loanrequests/{id}", "DELETE", token: token);
                TempData["Success"] = "Loan request removed.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The request couldn't be removed. Something got in the way.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/DirectIssue/{requestId}")]
        [AccessFilter("Admin, Coordinator")]
        public IActionResult DirectIssue(int requestId, string startDate, string endDate)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                int coordinatorId = HttpContext.Session.GetInt32("UserId") ?? 0;
                string isoStart = DateTime.TryParse(startDate, out var ds) ? ds.ToString("yyyy-MM-ddTHH:mm:ss") : startDate;
                string isoEnd   = DateTime.TryParse(endDate, out var de)   ? de.ToString("yyyy-MM-ddTHH:mm:ss") : endDate;
                var dto = new CreateLoanTicketDto
                {
                    LoanRequestId = requestId,
                    CoordinatorId = coordinatorId,
                    StartDate     = isoStart,
                    EndDate       = isoEnd
                };
                _auxiliarHttp.EnviarSolicitud("api/v1/loantickets", "POST", dto, token);
                TempData["Success"] = "Ticket issued directly.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't issue the ticket. Something drifted off course.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/DeleteTicket/{id}")]
        public IActionResult DeleteTicket(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loantickets/{id}", "DELETE", token: token);
                TempData["Success"] = "Approval undone. The request is back to pending.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't undo that approval. The ticket may no longer be in the right state.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Loans/LoanHistory")]
        [AccessFilter("Member")]
        public IActionResult LoanHistory(int? month, int? year)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var token   = HttpContext.Session.GetString("Token");

            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear  = year;

            if (!userId.HasValue || !month.HasValue || !year.HasValue)
                return View(new List<LoanTicketVM>());

            try
            {
                var loans = _auxiliarHttp.EnviarYDeserializar<List<LoanTicketVM>>(
                    $"api/v1/loanrequests/user/{userId}/{month}/{year}", "GET", token: token)
                    ?? new List<LoanTicketVM>();

                return View(loans);
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load the loan history. The records drifted out of range.";
                return View(new List<LoanTicketVM>());
            }
        }
    }
}
