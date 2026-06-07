using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Filters
{
    public class AlreadyLoggedFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("UserId") != null)
            {
                string? role = context.HttpContext.Session.GetString("UserRole");
                context.Result = role == "Coordinator"
                    ? new RedirectToActionResult("Index", "Loans", null)
                    : new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
