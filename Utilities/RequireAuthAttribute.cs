using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tripfinity.Utilities;

public class RequireAuthAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Session.GetInt32("userId") == null)
        {
            context.Result = new RedirectToActionResult("SignIn", "Auth", null);
            return;
        }
        await base.OnActionExecutionAsync(context, next);
    }
}