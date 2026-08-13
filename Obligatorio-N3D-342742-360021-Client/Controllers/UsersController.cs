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
                var token = HttpContext.Session.GetString("Token");
                var users = _auxiliarHttp
                    .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET", token: token)
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
            catch (ApiException ex)
            {
                ViewBag.message = ex.Message;
                return View(new List<UserVM>());
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
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud("api/v1/Users/create", "POST", user, token);
                TempData["Success"] = "Member added to the observatory.";
                return RedirectToAction("Index");
            }
            catch (ApiException ex)
            {
                ViewBag.message = ex.Message;
                return View();
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
                var token = HttpContext.Session.GetString("Token");
                var users = _auxiliarHttp
                    .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET", token: token)
                    ?? new List<UserVM>();

                var user = users.FirstOrDefault(u => u.UserId == id);
                if (user == null)
                {
                    TempData["Error"] = "We searched the whole sky and couldn't find that member.";
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Something drifted out of orbit. Couldn't load that member's details.";
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
                var token = HttpContext.Session.GetString("Token");

                // ASP.NET Core suppresses value on type="password" inputs — if the form
                // submitted no password, fetch the current one from the API to preserve it.
                string effectivePassword = user.password ?? string.Empty;
                if (string.IsNullOrWhiteSpace(effectivePassword))
                {
                    var allUsers = _auxiliarHttp
                        .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET", token: token)
                        ?? new List<UserVM>();
                    effectivePassword = allUsers.FirstOrDefault(u => u.UserId == id)?.password ?? string.Empty;
                }

                var body = new {
                    userId      = user.UserId,
                    fullName    = user.fullName,
                    username    = user.username,
                    email       = user.email,
                    phoneNumber = user.phoneNumber,
                    address     = user.address,
                    role        = user.role,
                    password    = effectivePassword
                };
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/update/{id}", "POST", body, token);
                TempData["Success"] = "Member details updated successfully.";
                return RedirectToAction("Index");
            }
            catch (ApiException ex)
            {
                ViewBag.message = ex.Message;
                return View(user);
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
                    TempData["Error"] = "Something seems off with that request. Please try again.";
                    return RedirectToAction("Index");
                }
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/delete/{id}", "POST", token: token);
                TempData["Success"] = "Member removed from the roster.";
                return RedirectToAction("Index");
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't remove that member. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }

        [AlreadyLoggedFilter]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AlreadyLoggedFilter]
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
                return user.UserRole == "Coordinator"
                    ? RedirectToAction("Index", "Loans")
                    : RedirectToAction("Index", "Home");
            }
            catch (ApiException ex)
            {
                ViewBag.message = ex.Message;
                return View();
            }
            catch (TaskCanceledException)
            {
                // The API on Render was likely asleep and the cold start took longer than
                // even our extended HttpClient timeout. Ask the user to retry instead of
                // implying their credentials were wrong.
                ViewBag.message = "The observatory is still waking up. Please wait a few seconds and try again.";
                return View();
            }
            catch (Exception)
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
