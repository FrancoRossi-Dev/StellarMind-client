using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Obligatorio_N3D_342742_360021.Filters
{
    public class AccessFilter : ActionFilterAttribute

    {
        // example [AccessFilter("Admin, Coordinator")]
        internal string AllowedRoles { get; }

        public AccessFilter(string rolesString)
        {
            AllowedRoles = rolesString;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? rolesString = context.HttpContext.Session.GetString("UserRole");

            List<string> allowedRoles = AllowedRoles
                .Split(',')
                .Select(r => r.Trim())
                .ToList();

            List<string> userRoles = rolesString?
                .Split(',')
                .Select(r => r.Trim())
                .ToList() ?? new List<string>();

            if (!userRoles.Any(r => allowedRoles.Contains(r)))
            {
                context.Result = new RedirectResult("~/usuario/notauthorized");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
