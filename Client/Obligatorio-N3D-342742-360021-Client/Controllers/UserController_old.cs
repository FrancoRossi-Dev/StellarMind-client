//using Client.Services.Http;
//using Microsoft.AspNetCore.Mvc;
//using Obligatorio_N3D_342742_360021.Filters;


//namespace Obligatorio_N3D_342742_360021.Controllers
//{
//    public class UserController : Controller
//    {
//        private readonly AuxiliarClienteHttp _auxHttp;

//        public UserController(AuxiliarClienteHttp auxHttp)
//        {
//            _auxHttp = auxHttp;
//        }

//        //[LoggedUserFilter]
//        public ActionResult Index()
//        {
//            try
//            {
//                var users = _auxHttp.EnviarYDeserializar<List<UserVM>>("/api/v1/User", "GET", null, HttpContext.Session.GetString("UserToken"));

//                return View(users);
//            }
//            catch (Exception)
//            {
//                ViewBag.msg = "no hay";
//                return View();
//            }
//        }

//        // GET: UserController/Details/5
//        //[LoggedUserFilter]
//        public ActionResult Details(int id)
//        {
//            return View();
//        }

//        // GET: UserController/Create
//        //[LoggedUserFilter]
//        public ActionResult Create()
//        {
//            return View();
//        }

//        // POST: UserController/Create
//        [HttpPost]
//        //[ValidateAntiForgeryToken]
//        //[LoggedUserFilter]
//        public ActionResult Create(UserVM userData)
//        {
//            return View();
//            //    try
//            //    {
//            //        RegisterUserDTO RUserDTO = UserMapper.VMtoRegisterDTO(userData);
//            //        _registerUser.Execute(RUserDTO);
//            //        return RedirectToAction("Index");
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        ViewBag.Msg = ex.Message;
//            //    }
//        }

//        // GET: UserController/Edit/5
//        [LoggedUserFilter]
//        public ActionResult Edit(int id)
//        {
//            return View();
//        }

//        // POST: UserController/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [LoggedUserFilter]
//        public ActionResult Edit(int id, IFormCollection collection)
//        {
//            try
//            {
//                return RedirectToAction(nameof(Index));
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        // GET: UserController/Delete/5
//        [LoggedUserFilter]
//        [AccessFilter("Admin")]
//        public ActionResult Delete(int id)
//        {
//            return View();
//        }

//        // POST: UserController/Delete/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [LoggedUserFilter]
//        [AccessFilter("Admin")]
//        public ActionResult Delete(int id, IFormCollection collection)
//        {
//            try
//            {
//                return RedirectToAction(nameof(Index));
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        // GET: UserController/Login
//        public ActionResult Login()

//        {
//            //IEnumerable<UserVM> users = _listAllUsers.Execute();
//            //if (HttpContext.Session.GetInt32("UserId") != null)
//            //{
//            //    return RedirectToAction("Index", "Home");
//            //}

//            return View();
//        }

//        // POST: UserController/Login
//        //    [HttpPost]
//        //    [ValidateAntiForgeryToken]
//        //    public ActionResult Login(LogInUserVM model)
//        //    {
//        //        try
//        //        {
//        //            if (model == null)
//        //            {
//        //                return View(model);
//        //            }

//        //            // Armar el DTO usando el ViewModel
//        //            LogInUserDto credentials = new LogInUserDto(model.UserName, model.Password);

//        //            // Ejecutar el caso de uso
//        //            LoggedUserDTO loggedUser = _loginUser.Execute(credentials);

//        //            if (loggedUser == null)
//        //            {
//        //                ViewBag.Msg = "Usuario o contraseña incorrectos.";
//        //                return View(model);
//        //            }

//        //            HttpContext.Session.SetInt32("UserId", loggedUser.UserId);
//        //            HttpContext.Session.SetString("UserName", loggedUser.UserName);
//        //            HttpContext.Session.SetString("UserRole", loggedUser.UserRole);

//        //            return RedirectToAction("Index", "Home");
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            ViewBag.Msg = ex.Message;
//        //            return View(model);
//        //        }
//        //    }
//        //    [LoggedUserFilter]

//        //    public ActionResult Logout()
//        //    {
//        //        HttpContext.Session.Clear();
//        //        return RedirectToAction("Login");
//        //    }
//    }
//}
