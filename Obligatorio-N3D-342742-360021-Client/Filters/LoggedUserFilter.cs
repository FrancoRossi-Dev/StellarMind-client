using Microsoft.AspNetCore.Mvc.Filters;


namespace Obligatorio_N3D_342742_360021.Filters
{
    public class LoggedUserFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("UserId") == null)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "User", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Session.GetInt32("UserId") == null)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "User", null);
            }
        }
    }
}
