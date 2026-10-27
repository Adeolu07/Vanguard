using Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tripfinity.Utilities;

/// <summary>
/// Guards JSON endpoints. Unlike the MVC redirect filters this returns 401/403
/// so callers (fetch/XHR) receive a machine-readable status instead of HTML.
/// </summary>
public sealed class ApiSessionAuthAttribute : ActionFilterAttribute
{
    public string? Role { get; set; }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("userId");

        if (userId is null)
        {
            context.Result = new UnauthorizedObjectResult(new { success = false, error = "Not signed in." });
            return;
        }

        if (!string.IsNullOrEmpty(Role))
        {
            var authorised = Role == ApiRoles.Admin
                ? await IsAdminAsync(context, userId.Value)
                : string.Equals(session.GetString("Role"), Role, StringComparison.Ordinal);

            if (!authorised)
            {
                context.Result = new ObjectResult(new { success = false, error = "Forbidden." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        context.HttpContext.Items["userId"] = userId.Value;
        await next();
    }

    private static async Task<bool> IsAdminAsync(ActionExecutingContext context, int userId)
    {
        var adminService = context.HttpContext.RequestServices.GetService(typeof(IAdminService)) as IAdminService;
        return adminService is not null && await adminService.IsAdminAsync(userId);
    }
}

public static class ApiRoles
{
    public const string Admin = "Admin";
    public const string Marshal = "Marshal";
    public const string Passenger = "Passenger";
}
