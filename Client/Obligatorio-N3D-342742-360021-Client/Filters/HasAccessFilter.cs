using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Obligatorio_N3D_342742_360021.Filters
{
    public class UserHasAccessFilter : ActionFilterAttribute
    {
        internal string allowdRoles { get; }

        public UserHasAccessFilter(string rol)
        {
            allowdRoles = rol;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            string? roles = context.HttpContext.Session.GetString("UserRol");

            if (roles == null)
            {
                context.Result = new RedirectResult("~/user/Login");
                return;
            }
            List<string> allowedRolesList = allowdRoles.Split(',')
                .Select(r => r.Trim()).ToList();

            if (!allowedRolesList.Contains(roles))
            {
                context.Result = new RedirectResult("~/Forbidden");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}