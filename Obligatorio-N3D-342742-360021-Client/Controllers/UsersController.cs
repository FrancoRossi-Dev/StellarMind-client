using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    public class UsersController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var users = _auxiliarHttp
                    .EnviarYDeserializar<List<UserVM>>("api/v1/Users", "GET")
                    ?? new List<UserVM>();

                return View(users);
            }
            catch (Exception)
            {
                ViewBag.msg = "nada";
                return View();
            }
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public IActionResult Create(string fullName, string username, string phoneNumber, string email, string password, string address, string role)
        {
            try
            {
                var user = new UserVM(fullName, username, phoneNumber, email, password, address, role);
                _auxiliarHttp.EnviarSolicitud("api/v1/Users/create", "POST", user);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error creating user.";
                return View();
            }
        }
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            //TODO GetUserById
            return View();
        }
        [HttpPut("Edit/{id}")]
        public IActionResult Edit(int id, UserVM user)
        {
            try
            {
                if (id <= 0 || user == null)
                {
                    ViewBag.msg = "Invalid user data.";
                    return View();
                }
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/edit/{id}", "PUT", user);
                return RedirectToAction("Index");

            }
            catch (Exception)
            {
                ViewBag.msg = "Error updating user.";
                return View();
            }
        }
        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    ViewBag.msg = "Invalid user ID.";
                    return View();
                }
                _auxiliarHttp.EnviarSolicitud($"api/v1/Users/delete/{id}", "DELETE", null);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error deleting user.";
                return View();
            }
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login(LogInUserDto dto)
        {
            try
            {
                LoginResponseDto? payload = _auxiliarHttp.EnviarYDeserializar<LoginResponseDto>("api/v1/auth/login", "POST", dto);
                string? token = payload.Token;
                LoggedUserDTO? user = payload.User;

                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.UserRole);
                HttpContext.Session.SetString("Email", user.Email);

                HttpContext.Session.SetString("Token", token);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
                return (View());
            }
        }

        public IActionResult Logout()
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud("api/v1/auth/logout", "POST", null);
                // TODO: Clear any client-side session or authentication tokens if necessary
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
                return (View());
            }
        }
    }
}