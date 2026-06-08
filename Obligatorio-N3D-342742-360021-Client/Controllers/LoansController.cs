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
                var token = HttpContext.Session.GetString("Token");
                var requests = _auxiliarHttp
                    .EnviarYDeserializar<List<PendingLoanRequestVM>>("api/v1/loanrequests", "GET", token: token)
                    ?? new List<PendingLoanRequestVM>();

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
            var nights = new List<ObservationNightVM>();
            if (userId.HasValue)
            {
                try
                {
                    var token = HttpContext.Session.GetString("Token");
                    var all = _auxiliarHttp
                        .EnviarYDeserializar<List<ObservationNightVM>>($"api/v1/users/nights/{userId}", "GET", token: token)
                        ?? new List<ObservationNightVM>();
                    nights = all.Where(n => !n.IsRequested).ToList();
                }
                catch { }
            }
            ViewBag.Nights = nights;
            ViewBag.SelectedNightId = nightId;
            return View();
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
                _auxiliarHttp.EnviarSolicitud("api/v1/loanrequests", "POST", dto, token);
                TempData["Success"] = "Loan request submitted.";
                return RedirectToAction("Index");
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
            catch (Exception)
            {
                ViewBag.msg = "Couldn't approve that loan. Something drifted off course.";
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
                ViewBag.msg = "Couldn't reject that loan. Something went sideways out there.";
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
    }
}
