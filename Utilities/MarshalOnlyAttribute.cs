using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace _Tripfinity.Utilities;

public class MarshalOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var marshalId = context.HttpContext.Session.GetInt32("marshalId");

        if (marshalId == null)
        {
            context.Result = new RedirectToActionResult(
                "MarshalSignIn", "Auth", null);
            return;
        }

        // Optional: pass the id to the controller via a property
        context.HttpContext.Items["MarshalId"] = marshalId.Value;

        base.OnActionExecuting(context);
    }
}