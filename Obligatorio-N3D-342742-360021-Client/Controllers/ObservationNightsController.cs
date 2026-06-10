using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Filters;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Services.Http;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class ObservationNightsController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            try
            {
                string? role = HttpContext.Session.GetString("UserRole");
                int? userId = HttpContext.Session.GetInt32("UserId");
                var token = HttpContext.Session.GetString("Token");

                string url = (role == "Member" && userId.HasValue)
                    ? $"api/v1/users/nights/{userId}"
                    : "api/v1/observationnights";

                var nights = _auxiliarHttp
                    .EnviarYDeserializar<List<ObservationNightVM>>(url, "GET", token: token)
                    ?? new List<ObservationNightVM>();

                if (role == "Member" && userId.HasValue)
                {
                    try
                    {
                        var userRequests = _auxiliarHttp
                            .EnviarYDeserializar<List<PendingLoanRequestVM>>($"api/v1/loanrequests/user/{userId}", "GET", token: token)
                            ?? new List<PendingLoanRequestVM>();

                        ViewBag.NightRequestMap = userRequests
                            .Where(r => r.ObservationNight != null)
                            .ToDictionary(r => r.ObservationNight!.Id, r => r.RequestId);
                    }
                    catch { ViewBag.NightRequestMap = new Dictionary<int, int>(); }
                }
                else
                {
                    try
                    {
                        var users = _auxiliarHttp
                            .EnviarYDeserializar<List<UserVM>>("api/v1/users", "GET", token: token)
                            ?? new List<UserVM>();
                        ViewBag.UserNames = users.ToDictionary(u => u.UserId, u => u.username);
                    }
                    catch { ViewBag.UserNames = new Dictionary<int, string>(); }
                }

                return View(nights);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View(new List<ObservationNightVM>());
            }
        }

        [HttpGet("ObservationNights/Details/{id}")]
        public IActionResult Details(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                var night = _auxiliarHttp
                    .EnviarYDeserializar<ObservationNightVM>($"api/v1/observationnights/{id}", "GET", token: token);

                return View(night);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("ObservationNights/ByUser/{userId}")]
        public IActionResult ByUser(int userId)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                var nights = _auxiliarHttp
                    .EnviarYDeserializar<List<ObservationNightVM>>($"api/v1/users/nights/{userId}", "GET", token: token)
                    ?? new List<ObservationNightVM>();

                return View("Index", nights);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View("Index", new List<ObservationNightVM>());
            }
        }

        [HttpGet("ObservationNights/Create")]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("Token");
            ViewBag.CelestialObjects = FetchCelestialObjects(token);
            return View();
        }

        [HttpPost("ObservationNights/Create")]
        public IActionResult Create(int requestingUserId, int celestialObjectId, string date, string details)
        {
            var token = HttpContext.Session.GetString("Token");
            try
            {
                string isoDate = DateTime.TryParse(date, out var d) ? d.ToString("yyyy-MM-ddT00:00:00") : date;
                var dto = new CreateObservationNightDto(requestingUserId, celestialObjectId, isoDate, details);
                _auxiliarHttp.EnviarSolicitud("api/v1/observationnights", "POST", dto, token);
                TempData["Success"] = "Observation plan created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                ViewBag.CelestialObjects = FetchCelestialObjects(token);
                return View();
            }
        }

        [HttpGet("ObservationNights/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var token = HttpContext.Session.GetString("Token");
            try
            {
                var night = _auxiliarHttp
                    .EnviarYDeserializar<ObservationNightVM>($"api/v1/observationnights/{id}", "GET", token: token);

                ViewBag.CelestialObjects = FetchCelestialObjects(token);
                return View(night);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost("ObservationNights/Edit/{id}")]
        public IActionResult Edit(int id, int celestialObjectId, string date, string details)
        {
            var token = HttpContext.Session.GetString("Token");
            try
            {
                string isoDate = DateTime.TryParse(date, out var d) ? d.ToString("yyyy-MM-ddT00:00:00") : date;
                var dto = new LoanObservationNightDto { CelestialObjectId = celestialObjectId, Date = isoDate, Details = details };
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "PUT", dto, token);
                TempData["Success"] = "Observation plan updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                ViewBag.CelestialObjects = FetchCelestialObjects(token);
                return View();
            }
        }

        private List<CelestialObjectVM> FetchCelestialObjects(string? token) =>
            (_auxiliarHttp.EnviarYDeserializar<List<CelestialObjectVM>>("api/v1/celestialobjects", "GET", token: token)
            ?? new List<CelestialObjectVM>())
            .OrderBy(o => o.Name)
            .ToList();

        [HttpPost("ObservationNights/CancelRequest/{requestId}")]
        public IActionResult CancelRequest(int requestId)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/loanrequests/{requestId}/cancel", "PUT", token: token);
                TempData["Success"] = "Equipment request canceled. Your night is back in planning.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost("ObservationNights/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "DELETE", token: token);
                TempData["Success"] = "Observation plan removed.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
