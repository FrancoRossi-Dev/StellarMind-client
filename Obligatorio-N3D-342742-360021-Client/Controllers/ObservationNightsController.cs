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

                string url = (role == "Member" && userId.HasValue)
                    ? $"api/v1/users/nights/{userId}"
                    : "api/v1/observationnights";

                var nights = _auxiliarHttp
                    .EnviarYDeserializar<List<ObservationNightVM>>(url, "GET")
                    ?? new List<ObservationNightVM>();

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
                var night = _auxiliarHttp
                    .EnviarYDeserializar<ObservationNightVM>($"api/v1/observationnights/{id}", "GET");

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
                var nights = _auxiliarHttp
                    .EnviarYDeserializar<List<ObservationNightVM>>($"api/v1/observationnights/user/{userId}", "GET")
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
            return View();
        }

        [HttpPost("ObservationNights/Create")]
        public IActionResult Create(int requestingUserId, int celestialObjectId, string date, string details)
        {
            try
            {
                var dto = new CreateObservationNightDto(requestingUserId, celestialObjectId, date, details);
                _auxiliarHttp.EnviarSolicitud("api/v1/observationnights", "POST", dto);
                TempData["Success"] = "Observation plan created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View();
            }
        }

        [HttpGet("ObservationNights/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            try
            {
                var night = _auxiliarHttp
                    .EnviarYDeserializar<ObservationNightVM>($"api/v1/observationnights/{id}", "GET");

                return View(night);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost("ObservationNights/Edit/{id}")]
        public IActionResult Edit(int id, int requestingUserId, int celestialObjectId, string date, string details)
        {
            try
            {
                var dto = new CreateObservationNightDto(requestingUserId, celestialObjectId, date, details);
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "PUT", dto);
                TempData["Success"] = "Observation plan updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View();
            }
        }

        [HttpPost("ObservationNights/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "DELETE");
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
