using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
        _logger.LogError(exception, "Unhandled system error occured: {Message}", exception.Message);

        var (statusCode, title, detail) = ClassifyException(exception);

        // API requests keep the JSON ProblemDetails response
        if (IsApiRequest(httpContext))
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }

        // Browser request → redirect to our error page
        var url = $"/Home/Error?statusCode={statusCode}&title={Uri.EscapeDataString(title)}&detail={Uri.EscapeDataString(detail)}";
        httpContext.Response.Redirect(url);
        return true;
    }

    private static bool IsApiRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) 
               || context.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    private static (int Status, string Title, string Detail) ClassifyException(Exception exception)
    {
        var ex = exception;
        while (ex != null)
        {
            if (ex is SqlException sqlEx && IsConnectionFailure(sqlEx))
            {
                return (StatusCodes.Status503ServiceUnavailable,
                    "Service Unavailable",
                    "We're experiencing technical difficulties. Please try again in a few minutes.");
            }
            ex = ex.InnerException;
        }

        return (StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            "Something went wrong on our end. Please try again later or contact support if the problem persists.");
    }

    private static bool IsConnectionFailure(SqlException sqlEx) 
        => sqlEx.Number is 10061 or 10060 or 53 or 11001 or 10054;
}