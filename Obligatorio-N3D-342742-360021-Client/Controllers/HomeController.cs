using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
