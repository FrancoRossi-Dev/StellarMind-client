using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Filters;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Services.Http;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    public class UsersController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        [LoggedUserFilter]
        public IActionResult Index(string? search, string? role)
        {
            try
            {
                var users = _auxiliarHttp
                    .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET")
                    ?? new List<UserVM>();

                if (!string.IsNullOrWhiteSpace(search))
                    users = users.Where(u =>
                        u.fullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(role) && role != "All")
                    users = users.Where(u => u.role == role).ToList();

                ViewBag.Search = search;
                ViewBag.Role = role;

                return View(users);
            }
            catch (Exception)
            {
                ViewBag.message = "The member list seems to have wandered off. Try again in a moment.";
                return View(new List<UserVM>());
            }
        }

        [LoggedUserFilter]
        [AccessFilter("Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [LoggedUserFilter]
        [AccessFilter("Admin")]
        [HttpPost]
        public IActionResult Create(UserVM user)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud("api/v1/Users/create", "POST", user);
                TempData["Success"] = "Member added to the observatory.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.message = "Something got lost in the cosmos. The member couldn't be added.";
                return View();
            }
        }

        [LoggedUserFilter]
        [AccessFilter("Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var users = _auxiliarHttp
                    .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET")
                    ?? new List<UserVM>();

                var user = users.FirstOrDefault(u => u.UserId == id);
                if (user == null)
                {
                    ViewBag.message = "We searched the whole sky and couldn't find that member.";
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (Exception)
            {
                ViewBag.message = "Something drifted out of orbit. Couldn't load that member's details.";
                return RedirectToAction("Index");
            }
        }

        [LoggedUserFilter]
        [AccessFilter("Admin")]
        [HttpPost]
        public IActionResult Edit(int id, UserVM user)
        {
            try
            {
                if (id <= 0)
                {
                    ViewBag.message = "A few things seem off, check the details and try again.";
                    return View(user);
                }
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/update/{id}", "POST", user);
                TempData["Success"] = "Member details updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.message = "The update got lost between the stars. Please try again.";
                return View(user);
            }
        }

        [LoggedUserFilter]
        [AccessFilter("Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    ViewBag.message = "Something seems off with that request. Please try again.";
                    return RedirectToAction("Index");
                }
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/delete/{id}", "POST", null);
                TempData["Success"] = "Member removed from the roster.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.message = "Couldn't remove that member. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LogInUserDto dto)
        {
            try
            {
                LoginResponseDto? payload = _auxiliarHttp.EnviarYDeserializar<LoginResponseDto>("api/v1/auth/login", "POST", dto);
                LoggedUserDTO? user = payload!.User;

                HttpContext.Session.SetInt32("UserId", user!.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.UserRole);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Token", payload.Token!);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Hmm, the stars didn't align. Check your credentials and try again.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
