using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class ObservationNightsController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var nights = _auxiliarHttp
                    .EnviarYDeserializar<List<ObservationNightVM>>("api/v1/observationnights", "GET")
                    ?? new List<ObservationNightVM>();

                return View(nights);
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load the observation nights. The skies seem cloudy right now.";
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
            catch (Exception)
            {
                ViewBag.msg = "Couldn't find that night. It may have drifted off the map.";
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
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load this member's nights — try again in a moment.";
                return View("Index", new List<ObservationNightVM>());
            }
        }

        [HttpGet("ObservationNights/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("ObservationNights/Create")]
        public IActionResult Create(int userId, string date, string location, string notes)
        {
            try
            {
                var dto = new ObservationNightVM
                {
                    UserId = userId,
                    Date = date,
                    Location = location,
                    Notes = notes
                };
                _auxiliarHttp.EnviarSolicitud("api/v1/observationnights", "POST", dto);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't log that night. Something got in the way.";
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
            catch (Exception)
            {
                ViewBag.msg = "Couldn't load that session. Something drifted off.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("ObservationNights/Edit/{id}")]
        public IActionResult Edit(int id, int userId, string date, string location, string notes)
        {
            try
            {
                var dto = new ObservationNightVM
                {
                    Id = id,
                    UserId = userId,
                    Date = date,
                    Location = location,
                    Notes = notes
                };
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "PUT", dto);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The update got lost somewhere. Please try again.";
                return View();
            }
        }

        [HttpPost("ObservationNights/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/observationnights/{id}", "DELETE");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't remove that night. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }
    }
}
