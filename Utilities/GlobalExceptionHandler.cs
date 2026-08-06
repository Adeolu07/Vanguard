using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Utilities;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception,"Unhandled system error occured: {Message}", exception.Message);

        var errorMessage = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal server error",
            Detail = "An unexpected error occurred on our end. Please contact support.",
            Instance = httpContext.Request.Path
        };
        
        httpContext.Response.StatusCode = errorMessage.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(errorMessage, cancellationToken);
        return true;
    }
}