using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace _Tripfinity.Utilities;
public class AdminOnlyFilter : IAsyncActionFilter
{
    private readonly IAdminService _adminService;

    public AdminOnlyFilter(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.Session.GetInt32("userId");

        if (userId == null)
        {
            context.Result = new RedirectToActionResult("SignIn", "Auth", null);
            return;
        }

        var isAdmin = await _adminService.IsAdminAsync(userId.Value);
        if (!isAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}